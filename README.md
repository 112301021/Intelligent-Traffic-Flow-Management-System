# Traffic Intersection Simulation with Adaptive Signal Control

A Unity/C# simulation of a 3-way road intersection designed as a validation testbed for adaptive traffic signal timing. An external optimizer process writes signal durations to a watched config file; the simulation reads and applies them in real-time — decoupling the optimization logic from the simulation environment.

![Status](https://img.shields.io/badge/Status-Active-059669?style=flat)
![License](https://img.shields.io/badge/License-CC0--1.0-2563EB?style=flat)
![Last Updated](https://img.shields.io/badge/Last%20Updated-2026--07-6B7280?style=flat)

![C#](https://img.shields.io/badge/C%23-0D9488?style=flat&logo=csharp&logoColor=white)
![Unity](https://img.shields.io/badge/Unity-FFFFFF?style=flat&logo=unity&logoColor=black)
![Python](https://img.shields.io/badge/Python-0D9488?style=flat&logo=python&logoColor=white)
![OpenCV](https://img.shields.io/badge/OpenCV-0D9488?style=flat&logo=opencv&logoColor=white)

---

## Overview

Fixed-time traffic signals are inefficient by design — they allocate green time equally regardless of actual lane demand. This simulation models a 3-way intersection where signal durations are dynamically injected from an external source, allowing any optimization strategy (density-based, ML-based, or manual) to drive signal timing without modifying the simulation itself.

The architecture enforces a clean boundary: the simulation is a pure validator. The optimizer is external and replaceable.

---

## System Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    EXTERNAL LAYER                            │
│                                                              │
│   Python Optimizer / Manual Input / Any External Process     │
│              │                                               │
│              │  writes  wait_L - wait_R - wait_F (floats)   │
│              ▼                                               │
│          Input.txt  (flat config file)                       │
└──────────────────────┬───────────────────────────────────────┘
                       │  polls on last-modified timestamp
┌──────────────────────▼───────────────────────────────────────┐
│                    UNITY SIMULATION                          │
│                                                              │
│   ReadInput.cs ──▶ TrafficLightManager.cs                    │
│   (file watcher)    (round-robin signal controller)          │
│                            │                                 │
│                            ▼                                 │
│                     TrafficLight.cs                          │
│                     (per-lane state: Red / Green)            │
│                            │                                 │
│              ┌─────────────┼──────────────┐                  │
│              ▼             ▼              ▼                  │
│           Left Lane    Right Lane    Front Lane              │
│                                                              │
│   ObjectPooler.cs ──▶ CarSpawner.cs ──▶ Paths.cs            │
│   (pool queue)         (spawn gate)    (vehicle FSM)         │
│                              │                               │
│                              ▼                               │
│                       PathManager.cs                         │
│                    (Transform[6,4] waypoint matrix)          │
└──────────────────────────────────────────────────────────────┘
```

---

## Key Engineering Decisions

### 1. File-Based IPC (Decoupled Optimizer Interface)

`ReadInput.cs` polls `Input.txt` on `File.GetLastWriteTime()` change. Any external process — Python density estimator, reinforcement learning agent, or manual input — can drive signal timing by writing three float values. The simulation does not care what computed them.

This separation means the optimizer and simulator can be developed, tested, and replaced independently.

### 2. Object Pooling (`ObjectPooler.cs`)

Vehicles are pre-instantiated at scene load and recycled via a `Dictionary<string, Queue<GameObject>>`. Avoids repeated `Instantiate`/`Destroy` calls during simulation, eliminating GC spikes that would corrupt timing measurements.

```csharp
// Pool structure
Dictionary<string, Queue<GameObject>> poolDictionary;

// Spawn: dequeue → reposition → activate
// Despawn: deactivate → enqueue back
```

### 3. Waypoint Routing Matrix (`PathManager.cs`)

All 6 intersection paths (L→F, L→R, R→F, R→L, F→L, F→R) are encoded as a `Transform[6, 4]` matrix — 6 origin-destination pairs × 4 waypoints. Vehicles index into this matrix by path number, keeping movement logic in `Paths.cs` clean and path data centralized.

```
Path 0: Left  → Front  (via L1 → L1_F2 junction → F2 → F3)
Path 1: Left  → Right  (via L1 → R2 → R3)
Path 2: Right → Front  (via R1 → R1_F2 junction → F2 → F3)
Path 3: Right → Left   (via R1 → L2 → L3)
Path 4: Front → Left   (via F1 → F1_L2 junction → L2 → L3)
Path 5: Front → Right  (via F1 → F1_R2 junction → R2 → R3)
```

### 4. Lane Density Gating

Trigger colliders at each lane entrance track `waitingCarsCount` per lane. The spawner checks this count before activating a vehicle — enforcing a configurable maximum queue depth per lane (`maxWaitingCarsPerLane`). This prevents infinite queue buildup and makes congestion observable.

### 5. Vehicle State Machine (`Paths.cs`)

Each vehicle runs a two-state FSM: `Moving` / `Stopped`. State transitions are driven by:
- Proximity to a red-light stop line (`CheckForDistance`)
- Collision trigger with a leading vehicle in the same lane (`AdjustSpeed`)
- Junction override — vehicles inside the junction continue regardless of signal state

---

## Repository Structure

```
Traffic-Intersection-Simulation/
│
├── Assets/
│   └── Scripts/
│       ├── CarSpawner.cs              # Timed vehicle generation
│       ├── ObjectPooler.cs            # Pool management (Dictionary + Queue)
│       ├── PathManager.cs             # Waypoint matrix (Transform[6,4])
│       ├── Paths.cs                   # Per-vehicle FSM + movement
│       ├── TrafficLight.cs            # Per-lane Red/Green state
│       ├── TrafficLightManager.cs     # Round-robin signal controller
│       ├── Traffic.cs                 # UI display (current green lane + timer)
│       ├── ReadInput.cs               # File watcher for external timing input
│       ├── Junction.cs                # Junction zone override logic
│       ├── IPooledObject.cs           # Pool interface
│       └── Colliders For Lanes/
│           ├── Front.cs               # Front lane density counter
│           ├── Left.cs                # Left lane density counter
│           └── Right.cs               # Right lane density counter
│
├── config/
│   └── Input.txt                      # Signal timing config (external interface)
│
├── docs/
│   ├── architecture.md                # System architecture documentation
│   └── diagrams/                      # Architecture diagrams
│
└── README.md
```

---

## Technology Stack

| Technology | Purpose |
|------------|---------|
| C# | Unity scripting and simulation logic |
| Unity 2021.3 LTS | 3D simulation environment |
| Python | External optimizer (signal timing computation) |
| OpenCV | Computer vision for traffic density estimation |
| .NET Standard 2.1 | Cross-platform compatibility |

---

## Setup & Running

### Requirements

- Unity 2021.3 LTS or newer
- .NET Standard 2.1 (included with Unity)

### Step 1 — Open the Project

```
File → Open Project → select repository root
```

### Step 2 — Configure the Input File Path

Open `Assets/Scripts/ReadInput.cs` and update the filepath to point to `config/Input.txt` in your local repository:

```csharp
// Replace this:
private string filepath = "C:/Users/jake0/My files/.../Input.txt";

// With your path:
private string filepath = Application.dataPath + "/../config/Input.txt";
```

### Step 3 — Set Initial Signal Timings

Edit `config/Input.txt`:

```
L-R-F
3.0-3.0-3.0
```

Format: `wait_Left - wait_Right - wait_Front` (float, seconds)

### Step 4 — Run

Open the traffic scene in Unity and press Play. The simulation reads `Input.txt` on startup and on every file modification — change the values while running to see signal timing update live.

### Step 5 — Drive with External Optimizer (Optional)

Any script that writes to `Input.txt` will control signal timing:

```python
# Example: Python optimizer writing computed timings
import time

def write_timings(left, right, front, path="config/Input.txt"):
    with open(path, "w") as f:
        f.write("L-R-F\n")
        f.write(f"{left}-{right}-{front}\n")

# Example: bias toward the lane with most vehicles
write_timings(left=5.0, right=2.0, front=3.0)
```

---

## Signal Timing Interface

The optimizer-simulation interface is a single flat file:

```
Line 1: L-R-F          (header — lane order)
Line 2: 3.5-2.0-4.0    (green durations in seconds)
```

`TrafficLightManager` reads these on file change via `ReadInput` and calls `SetDelay(laneIndex)` to update the round-robin cycle. The simulation does not validate or optimize these values — it executes them exactly. Optimization logic is entirely external.

---

## Known Limitations

| Issue | Location | Impact |
|---|---|---|
| Hardcoded absolute file path | `ReadInput.cs:7` | Breaks on all machines except original dev | 
| File polling (not event-driven) | `ReadInput.cs:Update()` | Minor overhead each frame |
| Stub scripts with no implementation | `Left_TF.cs`, `Front_TF.cs` | Dead code, should be removed |
| Active debug logs in production | `Right_TF.cs:Update()` | Console noise, minor perf |
| No multi-intersection support | Architecture | Single junction only |
| Round-robin only | `TrafficLightManager.cs` | No priority/demand-based scheduling |

---

## Future Work

- **Externalize config path** — use `Application.dataPath` or a Unity `ScriptableObject`
- **Socket/pipe IPC** — replace file polling with a proper IPC channel for lower latency
- **Priority scheduling** — replace round-robin with demand-weighted signal allocation
- **Multi-intersection** — extend `PathManager` to support networked junction graphs
- **Metrics export** — log per-lane queue depth and throughput to CSV for optimizer feedback
- **Reinforcement learning loop** — feed simulation state back to Python optimizer for closed-loop training

---

## Team

| Name | Contribution |
|---|---|
| Ajay Kumar Mallameeda | Traffic simulation architecture, Unity implementation |and traffic visualization |
| Jake Mathew | Unity scripting, signal control system |
| Hemanth | Path routing system | UI and traffic visualization |
| Aravind K | Vehicle spawning and pooling |


---

Screenshots of the simulation are available in the `docs/diagrams/` directory.

---

## Lessons Learned

- **File-based IPC is simple but fragile** — The file-polling approach made the optimizer-simulation boundary trivially testable (any process writing to a file), but introduced per-frame I/O overhead and race conditions under high write frequency. A proper IPC channel (named pipe, socket) would be more robust.
- **Decoupling optimization from simulation was the right call** — Being able to replace the signal timing optimizer without touching a line of Unity code accelerated iteration significantly. The `Input.txt` contract forced a clean interface that both sides could develop against independently.
- **Object pooling eliminated GC spikes** — Pre-instantiating vehicles and recycling them via `Dictionary<string, Queue<GameObject>>` removed `Instantiate`/`Destroy` calls from the hot path, keeping frame times stable during simulation. This matters for any real-time system.
- **Documenting limitations honestly builds trust** — Publishing known issues (hardcoded paths, dead code, single intersection) in the README made the project's scope clear and gave collaborators immediate starting points for improvement.

---

## License & Author

**License:** CC0 1.0 Universal — Public Domain. See the [LICENSE](./LICENSE) file for details.

**Author:** [Ajaykumar Mallameeda](https://github.com/Ajaykumar-Mallameeda) · Indian Institute of Technology Palakkad

---

*Built at IIT Palakkad as part of a continuous learning journey in AI and Backend Engineering.*
