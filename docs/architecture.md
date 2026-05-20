# System Architecture

## Design Philosophy

The simulation is architected around a single principle: **the optimizer and the simulator must be completely decoupled**.

This means:
- The simulation never makes optimization decisions
- The optimizer never touches simulation internals
- The interface between them is a plain text file

This architecture allows the optimizer to be swapped — rule-based, ML-based, manual, or remote — without changing a single line of simulation code.

---

## Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│                  OPTIMIZATION LAYER                 │
│                  (External Process)                 │
│                                                     │
│  Input: camera feed / density estimate / model      │
│  Output: wait_L, wait_R, wait_F (float seconds)     │
│                                                     │
│  Examples:                                          │
│  - Python YOLO vehicle counter                      │
│  - Reinforcement learning agent                     │
│  - Manual input for testing                         │
└──────────────────────┬──────────────────────────────┘
                       │
                  Input.txt
                  (plain text IPC)
                       │
┌──────────────────────▼──────────────────────────────┐
│                  SIMULATION LAYER                   │
│                     (Unity)                         │
│                                                     │
│  ReadInput ──► TrafficLightManager ──► TrafficLight │
│                                                     │
│  ObjectPooler ──► CarSpawner ──► Paths              │
│                                                     │
│  PathManager (waypoint data)                        │
│  Junction (intersection override)                   │
│  Lane Colliders (density tracking)                  │
└─────────────────────────────────────────────────────┘
```

---

## Component Responsibilities

### ReadInput.cs
- **Role**: IPC bridge between external optimizer and Unity simulation
- **Mechanism**: Polls `File.GetLastWriteTime()` each frame; re-reads on change
- **Output**: `wait_L`, `wait_R`, `wait_F` float values
- **Dependency**: Flat text file at configured path

### TrafficLightManager.cs
- **Role**: Signal cycle controller
- **Mechanism**: Round-robin rotation across 3 lanes; delay per lane set from `ReadInput`
- **State**: Current active lane index `j`, cycle timer
- **Interface**: `trafficlights[i].TurnGreen()` / `TurnRed()`

### TrafficLight.cs
- **Role**: Per-lane signal state container
- **State**: `isRed`, `isGreen` booleans
- **Consumers**: `Paths.cs` (vehicle stop logic), `TrafficLightsColour.cs` (UI)

### PathManager.cs
- **Role**: Static routing data store
- **Data**: `Transform[6, 4]` — 6 paths × 4 waypoints
- **Access pattern**: Read-only after `Awake()`; indexed by vehicle's assigned `pathno`

### Paths.cs
- **Role**: Per-vehicle behavior — movement, stopping, lane assignment
- **FSM**: `Moving` ↔ `Stopped`
- **Stop triggers**: red light proximity, leading vehicle collision, junction zone
- **Data**: `pathno` (0–5), `lanetype` enum, `speed`, `c` (waypoint index)

### ObjectPooler.cs
- **Role**: Vehicle lifecycle management
- **Pattern**: Pre-instantiated pool per tag; `Dictionary<string, Queue<GameObject>>`
- **Gate**: Checks `waitingCarsCount[laneType] < maxWaitingCarsPerLane` before spawning

### CarSpawner.cs
- **Role**: Timed spawn trigger
- **Mechanism**: Frame timer; calls `ObjectPooler.SpawnFromPool("Car")` at interval

### Lane Collider Scripts (Front.cs, Left.cs, Right.cs)
- **Role**: Density tracking
- **Mechanism**: `OnTriggerEnter`/`Exit` increment/decrement `waitingCarsCount` in `PathManager`

### Junction.cs
- **Role**: Intersection override zone
- **Behavior**: Sets `isinjunc = true` on vehicles inside junction; disables red-light stopping

---

## Data Flow — Vehicle Lifecycle

```
CarSpawner.Update()
    └─► ObjectPooler.SpawnFromPool("Car")
            ├─► Check waitingCarsCount[lane] < max
            ├─► Dequeue from pool
            ├─► Set position to spawner transform
            ├─► SetActive(true)
            ├─► Call IPooledObject.OnObjectSpawn()
            └─► Enqueue back (circular pool)

Paths.FixedUpdate() [per vehicle, each physics tick]
    ├─► CheckForRed()
    │       ├─► CheckForDistance() → within stop zone?
    │       └─► trafficlights[lane].isRed → stop or go
    ├─► MoveToWayPoint()
    │       ├─► Vector3.MoveTowards(current, target, speed)
    │       ├─► Quaternion.Lerp(rotation)
    │       └─► Distance < 0.2f → advance waypoint index c++
    └─► On c == 4 (end of path):
            ├─► SetActive(false) → return to pool conceptually
            └─► Reset pathno and c for next spawn
```

---

## Data Flow — Signal Timing

```
External Process writes Input.txt:
    "L-R-F\n3.5-2.0-4.0\n"

ReadInput.Update() [each frame]:
    ├─► GetLastWriteTime() == lastModifiedTime? → skip
    └─► Changed → ReadTextFile()
            ├─► Parse line 2: split on '-'
            └─► Set wait_L, wait_R, wait_F

TrafficLightManager.Update() [each frame]:
    ├─► timer += deltaTime
    └─► timer > delay?
            ├─► Rotate(j): TurnRed(j-1), TurnGreen(j)
            ├─► SetDelay(j): delay = Input.wait_[lane]
            └─► j++ (mod 3), timer = 0
```

---

## Intersection Path Map

```
         [FRONT LANE]
              │
        F1────┼────F2────F3
              │
    ──────────┼──────────
   L1   L2   [JUNCTION]   R2   R1
    ──────────┼──────────
              │
        (junction zone)
```

**Path routing table:**

| Path No | From  | To    | Waypoints                    |
|---------|-------|-------|------------------------------|
| 0       | Left  | Front | L1 → L1_F2 → F2 → F3        |
| 1       | Left  | Right | L1 → R2 → R3 → R3           |
| 2       | Right | Front | R1 → R1_F2 → F2 → F3        |
| 3       | Right | Left  | R1 → L2 → L3 → L3           |
| 4       | Front | Left  | F1 → F1_L2 → L2 → L3        |
| 5       | Front | Right | F1 → F1_R2 → R2 → R3        |

---

## IPC Interface Specification

**File:** `config/Input.txt`

**Format:**
```
L-R-F
<left_green_seconds>-<right_green_seconds>-<front_green_seconds>
```

**Example:**
```
L-R-F
4.0-2.5-3.5
```

**Constraints:**
- Values are positive floats (seconds)
- All three values required
- File must exist before simulation starts
- Simulation checks modification timestamp each frame

**Writer contract:** Any process may write this file. The simulation will pick up changes within one frame (~16ms at 60fps).

---

## Known Architecture Constraints

1. **Single junction** — `PathManager` encodes exactly one intersection topology. Extending to multiple junctions requires refactoring the path matrix into a graph structure.

2. **File polling overhead** — `GetLastWriteTime()` is called every `Update()` frame. On most OSes this is a lightweight syscall, but a proper socket or named pipe would be more robust.

3. **Hardcoded path** — `ReadInput.cs` line 7 contains an absolute path. Must be changed to `Application.dataPath + "/../config/Input.txt"` for portability.

4. **Circular pool semantics** — Vehicles are enqueued back immediately after spawn in `ObjectPooler`. This means the pool is effectively a rotating sequence, not a true pool. With a small pool size, vehicles may share identity across overlapping lifetimes.
