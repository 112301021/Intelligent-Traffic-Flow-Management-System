using UnityEngine;

/// <summary>
/// Per-vehicle behavior controller.
/// Manages waypoint-following movement and stop/go state transitions.
///
/// State machine:
///   Moving  ↔  Stopped
///
/// Stop triggers:
///   1. Red light proximity — vehicle is within stop distance of its lane's stop line
///   2. Leading vehicle collision — same-lane vehicle ahead is stopped
///   3. Junction override — vehicles inside the junction zone always move (isinjunc = true)
///
/// Routing:
///   pathno (0–5) assigned randomly on spawn → indexes into PathManager.path[pathno, c]
///   c is the current waypoint index (0–3); vehicle deactivates when c reaches 4
/// </summary>
public class Paths : MonoBehaviour, IPooledObject
{
    // ── Lane Type Enum ─────────────────────────────────────────────────────────

    public enum LaneType
    {
        LeftLane,
        RightLane,
        FrontLane,
        Unknown
    }

    // ── State ──────────────────────────────────────────────────────────────────

    private enum CarState { Moving, Stopped }
    private CarState currentState = CarState.Moving;

    // ── References ─────────────────────────────────────────────────────────────

    private PathManager pm;
    private TrafficLightManager traffic;

    // ── Inspector-Assigned Stop Line References ────────────────────────────────

    [SerializeField] public GameObject left;   // Left lane stop line transform
    [SerializeField] public GameObject right;  // Right lane stop line transform
    [SerializeField] public GameObject front;  // Front lane stop line transform

    // ── Public State ───────────────────────────────────────────────────────────

    /// <summary>True while this vehicle is inside the junction collision zone.</summary>
    public bool isinjunc = false;

    /// <summary>True when this vehicle has been commanded to stop for a red light.</summary>
    public bool itsred = false;

    // ── Movement Parameters ─────────────────────────────────────────────────────

    public float speed;     // Current movement speed (units/frame)
    public float sp;        // Default speed (restored on green)
    public float rot;       // Rotation lerp factor

    // ── Routing ────────────────────────────────────────────────────────────────

    /// <summary>Index into PathManager.path rows (0–5). Assigned randomly on spawn.</summary>
    public int pathno;

    /// <summary>Current waypoint index along assigned path (0–3).</summary>
    private int c = 0;

    /// <summary>Which lane group this vehicle belongs to (for signal checking).</summary>
    public LaneType lanetype;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────

    void Awake()
    {
        traffic = GameObject.FindWithTag("TrafficLight").GetComponent<TrafficLightManager>();
        sp = speed;
        pathno = GetRandomPath();
        AssignLaneType();

        pm = GameObject.FindWithTag("Paths").GetComponent<PathManager>();
        if (pm == null)
            Debug.LogError("[Paths] PathManager not found on 'Paths' tagged object.");
    }

    void FixedUpdate()
    {
        // Junction override: vehicles inside intersection always move
        if (isinjunc)
        {
            currentState = CarState.Moving;
            itsred = false;
        }

        CheckForRed();

        if (currentState != CarState.Stopped)
            MoveToWayPoint();
    }

    // ── IPooledObject ──────────────────────────────────────────────────────────

    /// <summary>Called by ObjectPooler immediately after this vehicle is activated from pool.</summary>
    public void OnObjectSpawn()
    {
        pm = GameObject.FindWithTag("Paths").GetComponent<PathManager>();
        if (pm == null)
            Debug.LogError("[Paths] PathManager not found on spawn.");
    }

    // ── Collision Triggers ─────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
            AdjustSpeed(other.gameObject, isColliding: true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            AdjustSpeed(other.gameObject, isColliding: false);
            itsred = false;
            currentState = CarState.Moving;
        }
    }

    // ── Movement ───────────────────────────────────────────────────────────────

    private void MoveToWayPoint()
    {
        if (c < 4)
        {
            Vector3 target = pm.path[pathno, c].position;

            transform.position = Vector3.MoveTowards(transform.position, target, speed);
            transform.rotation = Quaternion.Lerp(transform.rotation, pm.path[pathno, c].rotation, rot);

            if (Vector3.Distance(target, transform.position) < 0.2f)
                c++;
        }
        else
        {
            // End of path — deactivate and reset for next use
            gameObject.SetActive(false);
            pathno = GetRandomPath();
            AssignLaneType();
            c = 0;
        }
    }

    // ── Speed Control ──────────────────────────────────────────────────────────

    private void AdjustSpeed(GameObject other, bool isColliding)
    {
        Paths otherPaths = other.GetComponent<Paths>();

        if (isColliding)
        {
            if (otherPaths.lanetype == this.lanetype)
            {
                // Same lane: stop the vehicle that's closer to the stop line
                if (CheckForDistance(other.gameObject))
                {
                    currentState = CarState.Stopped;
                    speed = 0f;
                    itsred = true;
                }
                else if (CheckForDistance(this.gameObject))
                {
                    otherPaths.currentState = CarState.Stopped;
                    otherPaths.speed = 0f;
                    itsred = true;
                }
                else if (otherPaths.itsred)
                {
                    // Queue behind a stopped vehicle
                    speed = 0;
                    itsred = true;
                }
            }
            else
            {
                // Different lane: yield if other vehicle is moving
                if (otherPaths.speed != 0)
                    speed = 0;
            }
        }
        else
        {
            // Collision ended — restore default speed
            speed = sp;
        }
    }

    // ── Signal Checking ────────────────────────────────────────────────────────

    private void CheckForRed()
    {
        if (!CheckForDistance(this.gameObject))
            return;

        int laneIndex = GetLaneIndex();
        if (traffic.trafficlights[laneIndex].isRed)
        {
            itsred = true;
            currentState = CarState.Stopped;
        }
        else
        {
            itsred = false;
            currentState = CarState.Moving;
        }
    }

    /// <summary>
    /// Returns true if this vehicle is within stop-line proximity distance
    /// AND is not currently inside the junction zone.
    /// </summary>
    private bool CheckForDistance(GameObject target)
    {
        if (isinjunc) return false;

        GameObject stopLine = GetStopLine();
        if (stopLine == null) return false;

        return Vector3.Distance(target.transform.position, stopLine.transform.position) < 3f;
    }

    // ── Lane Helpers ───────────────────────────────────────────────────────────

    private int GetRandomPath() => Random.Range(0, 6);

    private void AssignLaneType()
    {
        switch (pathno)
        {
            case 0: case 1: lanetype = LaneType.LeftLane;  break;
            case 2: case 3: lanetype = LaneType.RightLane; break;
            case 4: case 5: lanetype = LaneType.FrontLane; break;
        }
    }

    private GameObject GetStopLine()
    {
        switch (pathno)
        {
            case 0: case 1: return left;
            case 2: case 3: return right;
            case 4: case 5: return front;
            default:        return null;
        }
    }

    private int GetLaneIndex()
    {
        switch (pathno)
        {
            case 0: case 1: return 1; // Left → index 1 in trafficlights list
            case 2: case 3: return 0; // Right → index 0
            case 4: case 5: return 2; // Front → index 2
            default:        return 0;
        }
    }
}
