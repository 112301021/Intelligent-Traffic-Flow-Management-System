using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for Unity GameObjects.
/// Pre-instantiates a fixed pool of vehicles at scene load and recycles them
/// via a Queue per tag — avoiding Instantiate/Destroy overhead during simulation.
///
/// Spawn gating: checks per-lane waitingCarsCount before activating a vehicle.
/// If a lane is at capacity (maxWaitingCarsPerLane), the spawn is rejected.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("Spawn Points")]
    public GameObject LS; // Left lane spawner transform
    public GameObject RS; // Right lane spawner transform
    public GameObject FS; // Front lane spawner transform

    [Header("Pool Configuration")]
    public List<Pool> pools;

    [Header("Lane Capacity")]
    [Tooltip("Maximum vehicles allowed waiting in any single lane before spawn is rejected.")]
    public int maxWaitingCarsPerLane = 3;

    // Singleton — only one ObjectPooler per scene
    public static ObjectPooler Instance;

    private PathManager pm;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;
        pm = GameObject.FindWithTag("Paths").GetComponent<PathManager>();
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// Attempts to spawn a vehicle from the named pool.
    /// Rejects spawn if the vehicle's destination lane is at capacity.
    /// Returns the spawned GameObject, or null if spawn was rejected.
    /// </summary>
    public GameObject SpawnFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"[ObjectPooler] Pool with tag '{tag}' not found.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        Paths paths = objectToSpawn.GetComponent<Paths>();
        int pathno = paths.pathno;
        Paths.LaneType laneType = DetermineLaneType(pathno);

        // Reject spawn if lane is at capacity
        if (pm.waitingCarsCount[laneType] >= maxWaitingCarsPerLane)
        {
            poolDictionary[tag].Enqueue(objectToSpawn);
            return null;
        }

        // Position vehicle at the correct spawner
        Transform spawnPoint = GetSpawnPoint(pathno);
        if (spawnPoint == null)
        {
            Debug.LogError($"[ObjectPooler] No spawn point for pathno {pathno}");
            poolDictionary[tag].Enqueue(objectToSpawn);
            return null;
        }

        objectToSpawn.transform.position = spawnPoint.position;
        objectToSpawn.transform.rotation = spawnPoint.rotation;
        objectToSpawn.SetActive(true);

        // Notify pooled object of spawn event
        IPooledObject pooledObject = objectToSpawn.GetComponent<IPooledObject>();
        pooledObject?.OnObjectSpawn();

        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    /// <summary>Maps a path number to its originating lane type.</summary>
    private Paths.LaneType DetermineLaneType(int pathno)
    {
        switch (pathno)
        {
            case 0: case 1: return Paths.LaneType.LeftLane;
            case 2: case 3: return Paths.LaneType.RightLane;
            case 4: case 5: return Paths.LaneType.FrontLane;
            default:        return Paths.LaneType.Unknown;
        }
    }

    /// <summary>Returns the spawn point transform for a given path number.</summary>
    private Transform GetSpawnPoint(int pathno)
    {
        switch (pathno)
        {
            case 0: case 1: return LS.transform;
            case 2: case 3: return RS.transform;
            case 4: case 5: return FS.transform;
            default:        return null;
        }
    }
}
