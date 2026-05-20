using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all intersection routing data as a waypoint matrix.
///
/// Path layout (6 origin-destination combinations x 4 waypoints):
///   Path 0: Left  → Front  (L1 → L1_F2 junction → F2 → F3)
///   Path 1: Left  → Right  (L1 → R2 → R3)
///   Path 2: Right → Front  (R1 → R1_F2 junction → F2 → F3)
///   Path 3: Right → Left   (R1 → L2 → L3)
///   Path 4: Front → Left   (F1 → F1_L2 junction → L2 → L3)
///   Path 5: Front → Right  (F1 → F1_R2 junction → R2 → R3)
///
/// Vehicles index into this matrix by their assigned pathno.
/// PathManager is read-only after Awake().
/// </summary>
public class PathManager : MonoBehaviour
{
    [Header("Lane Entry/Exit Waypoints")]
    public GameObject L1, L2, L3;
    public GameObject R1, R2, R3;
    public GameObject F1, F2, F3;

    [Header("Junction Intermediate Waypoints")]
    public GameObject L1_F2;  // Left → Front junction
    public GameObject R1_F2;  // Right → Front junction
    public GameObject F1_L2;  // Front → Left junction
    public GameObject F1_R2;  // Front → Right junction

    /// <summary>
    /// Waypoint matrix: path[pathno, waypointIndex]
    /// Dimensions: [6 paths, 4 waypoints per path]
    /// </summary>
    public Transform[,] path;

    /// <summary>Current number of vehicles waiting at each lane entrance.</summary>
    public Dictionary<Paths.LaneType, int> waitingCarsCount;

    void Awake()
    {
        // Initialize lane density counters
        waitingCarsCount = new Dictionary<Paths.LaneType, int>
        {
            { Paths.LaneType.LeftLane,  0 },
            { Paths.LaneType.RightLane, 0 },
            { Paths.LaneType.FrontLane, 0 }
        };

        path = new Transform[6, 4];
        SetWayPoints();
    }

    private void SetWayPoints()
    {
        // Path 0: Left → Front
        path[0, 0] = L1.transform;
        path[0, 1] = L1_F2.transform;
        path[0, 2] = F2.transform;
        path[0, 3] = F3.transform;

        // Path 1: Left → Right
        path[1, 0] = L1.transform;
        path[1, 1] = R2.transform;
        path[1, 2] = R3.transform;
        path[1, 3] = R3.transform; // duplicate end waypoint — vehicle deactivates at c==4

        // Path 2: Right → Front
        path[2, 0] = R1.transform;
        path[2, 1] = R1_F2.transform;
        path[2, 2] = F2.transform;
        path[2, 3] = F3.transform;

        // Path 3: Right → Left
        path[3, 0] = R1.transform;
        path[3, 1] = L2.transform;
        path[3, 2] = L3.transform;
        path[3, 3] = L3.transform;

        // Path 4: Front → Left
        path[4, 0] = F1.transform;
        path[4, 1] = F1_L2.transform;
        path[4, 2] = L2.transform;
        path[4, 3] = L3.transform;

        // Path 5: Front → Right
        path[5, 0] = F1.transform;
        path[5, 1] = F1_R2.transform;
        path[5, 2] = R2.transform;
        path[5, 3] = R3.transform;
    }

    /// <summary>Called by lane entry colliders when a vehicle enters the waiting zone.</summary>
    public void IncrementWaitingCarsCount(Paths.LaneType laneType)
    {
        if (waitingCarsCount.ContainsKey(laneType))
        {
            waitingCarsCount[laneType]++;
        }
        else
        {
            Debug.LogError($"[PathManager] IncrementWaitingCarsCount: Unknown lane type '{laneType}'");
        }
    }

    /// <summary>Called by lane entry colliders when a vehicle exits the waiting zone.</summary>
    public void DecrementWaitingCarsCount(Paths.LaneType laneType)
    {
        if (waitingCarsCount.ContainsKey(laneType) && waitingCarsCount[laneType] > 0)
        {
            waitingCarsCount[laneType]--;
        }
        else
        {
            Debug.LogError($"[PathManager] DecrementWaitingCarsCount: Unknown lane type or count already zero for '{laneType}'");
        }
    }
}
