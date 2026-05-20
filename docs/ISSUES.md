# GitHub Issues — Technical Debt & Roadmap

This file documents known issues and planned improvements.
Each item below corresponds to a GitHub Issue that should be created.

---

## Bug Reports

### Issue 1 — Hardcoded file path in ReadInput.cs
**Label:** `bug` `good first issue`

ReadInput.cs originally contained a hardcoded absolute path:
```
C:/Users/jake0/My files/Unity projects/Traffic/Assets/Text/Input.txt
```
This breaks on all machines except the original developer's.

**Fix applied:** ReadInput.cs now uses `Application.dataPath + "/../config/Input.txt"` 
with a `SerializeField` override for custom paths.

**Status:** Fixed in current version.

---

### Issue 2 — Active Debug.Log in Right_TF.cs Update()
**Label:** `bug` `performance`

`Right_TF.cs` calls `Debug.Log(timer)` inside `Update()` — executing every frame.
This adds console noise and minor overhead during simulation.

**Fix:** Remove or replace with conditional `#if UNITY_EDITOR` guard.

**Status:** Open.

---

### Issue 3 — Stub scripts with empty bodies (Left_TF.cs, Front_TF.cs)
**Label:** `cleanup`

`Left_TF.cs` and `Front_TF.cs` contain commented-out implementations.
They attach to scene objects but perform no function.

**Options:**
- Implement full traffic checker logic
- Remove from scene and delete scripts

**Status:** Open.

---

### Issue 4 — Circular pool semantics in ObjectPooler
**Label:** `bug` `architecture`

Vehicles are enqueued back to the pool immediately after spawn (not after deactivation).
With a small pool size, a vehicle can be selected for spawn while its previous instance
is still active in the scene.

**Fix:** Track active vs. inactive pool slots; only enqueue on `SetActive(false)`.

**Status:** Open.

---

## Feature Requests

### Issue 5 — Replace file polling with socket IPC
**Label:** `enhancement` `architecture`

`ReadInput.cs` polls `File.GetLastWriteTime()` every frame.
A named pipe or UDP socket would provide lower latency and cleaner semantics.

**Priority:** Low (file polling works adequately for the simulation use case)

---

### Issue 6 — Metrics export (queue depth + throughput logging)
**Label:** `enhancement` `observability`

Add a CSV logger that records per-lane queue depth and signal state at each cycle.
This enables offline analysis of optimizer effectiveness.

**Output format:**
```
timestamp, signal_green, left_count, right_count, front_count, wait_L, wait_R, wait_F
```

---

### Issue 7 — Priority-based signal scheduling
**Label:** `enhancement`

Replace round-robin rotation with demand-weighted scheduling:
assign green time proportional to waiting vehicle counts per lane.

This would allow the simulation to validate optimizer strategies that
compute `wait_L/R/F` dynamically based on real density estimates.

---

### Issue 8 — Multi-intersection support
**Label:** `enhancement` `architecture`

`PathManager` encodes exactly one 3-way intersection topology.
Extending to a network of intersections requires:
- Graph-based path representation
- Per-intersection `TrafficLightManager` instances
- Coordinated signal timing across adjacent junctions

---

## Documentation

### Issue 9 — Add scene setup guide
**Label:** `documentation`

Add a `docs/scene-setup.md` explaining how to wire GameObjects to script references
in the Unity Inspector. New contributors cannot currently reproduce the scene
from code alone.

---

### Issue 10 — Record simulation GIF for README
**Label:** `documentation`

Convert existing Video-1.mp4 to an optimized GIF and embed in README
to demonstrate simulation behavior without requiring Unity installation.
