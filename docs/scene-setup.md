# Unity Scene Setup Guide

This document explains how to configure the Unity scene from scratch —
wiring all GameObjects to their script references in the Inspector.

---

## Required Scene Objects

You need the following tagged GameObjects in the scene:

| GameObject Name | Required Tag | Script Attached |
|---|---|---|
| PathsObject | `Paths` | `PathManager.cs` |
| TrafficLightObject | `TrafficLight` | `TrafficLightManager.cs` |
| LeftSpawner | — | — |
| RightSpawner | — | — |
| FrontSpawner | — | — |

---

## Waypoint Objects

Create empty GameObjects for each waypoint. Position them at the correct
intersection locations in the scene:

**Lane Endpoints:**
- `L1` — Left lane entry point
- `L2` — Left lane mid point
- `L3` — Left lane exit point
- `R1` — Right lane entry point
- `R2` — Right lane mid point
- `R3` — Right lane exit point
- `F1` — Front lane entry point
- `F2` — Front lane mid point
- `F3` — Front lane exit point

**Junction Intermediate Points:**
- `L1_F2` — Waypoint between L1 and F2 (Left→Front path)
- `R1_F2` — Waypoint between R1 and F2 (Right→Front path)
- `F1_L2` — Waypoint between F1 and L2 (Front→Left path)
- `F1_R2` — Waypoint between F1 and R2 (Front→Right path)

---

## PathManager Inspector Setup

Select the `PathsObject` (tagged `Paths`) and assign in the Inspector:

```
PathManager (Script)
├── L1  → [L1 GameObject]
├── L2  → [L2 GameObject]
├── L3  → [L3 GameObject]
├── R1  → [R1 GameObject]
├── R2  → [R2 GameObject]
├── R3  → [R3 GameObject]
├── F1  → [F1 GameObject]
├── F2  → [F2 GameObject]
├── F3  → [F3 GameObject]
├── L1_F2 → [L1_F2 GameObject]
├── R1_F2 → [R1_F2 GameObject]
├── F1_L2 → [F1_L2 GameObject]
└── F1_R2 → [F1_R2 GameObject]
```

---

## TrafficLightManager Inspector Setup

Select the `TrafficLightObject` and assign:

```
TrafficLightManager (Script)
├── Trafficlights  → [List, size 3 — pre-populated from lights list]
├── Lights
│   ├── [0] → RightLaneLight GameObject (has TrafficLight.cs)
│   ├── [1] → LeftLaneLight GameObject (has TrafficLight.cs)
│   └── [2] → FrontLaneLight GameObject (has TrafficLight.cs)
└── Input → [ReadInput.cs component reference]
```

---

## ObjectPooler Inspector Setup

```
ObjectPooler (Script)
├── LS → [LeftSpawner GameObject]
├── RS → [RightSpawner GameObject]
├── FS → [FrontSpawner GameObject]
├── Max Waiting Cars Per Lane → 3
└── Pools
    └── [0]
        ├── Tag → "Car"
        ├── Prefab → [Car Prefab]
        └── Size → 12  (or your desired pool size)
```

---

## Car Prefab Setup

The Car prefab must have:
- `Paths.cs` component attached
- `Rigidbody` component (for physics)
- `Collider` (trigger-enabled) for proximity detection
- Tag set to `"Car"`
- Inspector fields on Paths.cs:
  - `Left` → Left lane stop line GameObject
  - `Right` → Right lane stop line GameObject
  - `Front` → Front lane stop line GameObject
  - `Speed` → 0.05 (adjust to taste)
  - `Rot` → 0.05 (rotation lerp factor)

---

## Lane Collider Setup

Create trigger colliders at each lane entrance.
Attach the corresponding script:

| Collider Position | Script | Purpose |
|---|---|---|
| Left lane entrance | `Left.cs` | Counts Left lane waiting vehicles |
| Right lane entrance | `Right.cs` | Counts Right lane waiting vehicles |
| Front lane entrance | `Front.cs` | Counts Front lane waiting vehicles |

Each collider must have `Is Trigger = true`.

---

## Junction Zone Setup

Create a large trigger collider covering the center intersection area.
Attach `Junction.cs`. Set `Is Trigger = true`.

Vehicles inside this zone have `isinjunc = true` and bypass red light stopping.

---

## ReadInput Setup

```
ReadInput (Script)
└── Relative Config Path → "config/Input.txt"
    (this resolves to: [ProjectRoot]/config/Input.txt)
```

Ensure `config/Input.txt` exists with default content:
```
L-R-F
3.0-3.0-3.0
```

---

## Layer & Tag Setup

Required Tags (add in Edit → Project Settings → Tags and Layers):
- `Car`
- `Paths`
- `TrafficLight`
- `test` (legacy — can be removed if Right_TF.cs is cleaned up)

---

## Testing the Setup

1. Press Play in Unity
2. Open the Console — should see no errors
3. Vehicles should spawn from 3 spawner points
4. Traffic lights should cycle: Right → Left → Front → Right
5. Vehicles should stop at red lights and resume on green
6. Edit `config/Input.txt` while playing — signal durations should update
