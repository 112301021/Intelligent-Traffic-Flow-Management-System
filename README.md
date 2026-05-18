# 🚦 Intelligent Traffic Flow Optimization System

An AI-powered smart traffic management and adaptive signal optimization platform that combines computer vision, machine learning, embedded systems, and real-time traffic simulation to dynamically optimize urban traffic flow.

---

# 📌 Overview

The **Intelligent Traffic Flow Optimization System** is a smart-city traffic engineering platform designed to reduce road congestion through adaptive traffic-light control using real-time traffic density estimation.

The system integrates:

* Computer vision-based vehicle detection
* Real-time traffic-density estimation
* YOLO-based object detection
* Embedded camera systems
* Unity traffic simulation
* Adaptive signal optimization
* IoT-style data workflows

The platform demonstrates how AI and embedded systems can be integrated into intelligent transportation infrastructure for dynamic congestion management.

---

# 🏗️ System Architecture

## High-Level Workflow

```text
ESP32-CAM / CCTV
        ↓
Image Snapshot Acquisition
        ↓
Python ML Inference Pipeline
        ↓
YOLO Vehicle Detection
        ↓
Traffic Density Estimation
        ↓
Signal Timing Optimization
        ↓
Unity Traffic Simulation
        ↓
Adaptive Traffic-Light Control
```

---

# ⚙️ Core Features

## 🚗 Real-Time Vehicle Detection

* YOLO-based object detection
* Vehicle-density estimation
* Multi-lane traffic analysis
* Snapshot-based inference workflows

## 🧠 Intelligent Signal Optimization

* Dynamic green-signal allocation
* Adaptive congestion handling
* Real-time wait-time computation
* Lane-priority optimization

## 🛰️ Embedded & IoT Integration

* ESP32-CAM image acquisition
* Raspberry Pi-compatible workflow
* Automated snapshot transfer pipeline
* Lightweight edge-computing architecture

## 🎮 Simulation Infrastructure

* Unity-based traffic simulation
* C# traffic-control engine
* Signal-state synchronization
* Congestion behavior validation

---

# 🧩 Repository Structure

```text
Intelligent-Traffic-Flow-Management-System/
│
├── Assets/
│   ├── Scripts/                    # Unity traffic simulation scripts
│   ├── TrafficChecker/             # Traffic-density detection logic
│   └── Colliders For Lanes/        # Lane-based collision handling
│
├── TRAFFIC FLOW MANAGEMENT SYSTEM.docx
├── Video-1.mp4
├── Video-2.mp4
├── Video-3.mp4
└── README.md
```

---

# 🧠 Computer Vision Pipeline

The traffic optimization workflow uses computer vision and ML inference to estimate congestion levels.

## Workflow

1. ESP32-CAM captures traffic snapshots
2. Images are transferred to the processing pipeline
3. YOLO detects vehicles in each lane
4. Vehicle count is converted into congestion metrics
5. Signal duration is dynamically optimized
6. Unity simulation updates traffic-light states

---

# 🎮 Unity Simulation Engine

The project includes a Unity-based traffic simulation environment built in C#.

## Key Simulation Components

### Traffic System

* Vehicle spawning
* Lane movement
* Traffic-light interaction
* Collision handling

### Signal Management

* Adaptive signal timing
* Dynamic lane prioritization
* Congestion-aware control logic

### Scripts Included

```text
CarSpawner.cs
TrafficLight.cs
TrafficLightManager.cs
Traffic.cs
PathManager.cs
ReadInput.cs
ObjectPooler.cs
```

---

# 🧪 Technologies Used

## AI & Computer Vision

* YOLO
* TensorFlow
* OpenCV
* Python

## Embedded & IoT

* ESP32-CAM
* Raspberry Pi
* Snapshot streaming

## Simulation

* Unity
* C#

## Systems Engineering

* Real-time processing
* Adaptive control systems
* Smart traffic infrastructure

---

# 📂 Important Components

## `TrafficLightManager.cs`

Controls signal timing and adaptive traffic-light workflows.

## `Traffic.cs`

Handles traffic movement simulation.

## `ReadInput.cs`

Reads dynamically generated congestion metrics.

## `CarSpawner.cs`

Manages traffic generation within the simulation.

---

# 🚀 Setup & Execution

## Requirements

* Unity Engine
* Python 3.x
* TensorFlow
* OpenCV
* ESP32-CAM module

---

## Unity Setup

1. Open project in Unity.
2. Load the traffic simulation scene.
3. Run the Unity environment.

---

## ML Pipeline Setup

Install dependencies:

```bash
pip install tensorflow opencv-python numpy
```

Run the traffic inference pipeline:

```bash
python traffic_detection.py
```

---

# 📊 Adaptive Signal Logic

The adaptive traffic-light system dynamically adjusts:

* Green-light duration
* Lane priority
* Traffic wait time
* Congestion balancing

based on:

* Vehicle count
* Lane density
* Real-time traffic conditions

---

# 📈 Engineering Objectives

This project explores:

* Intelligent transportation systems
* Smart-city infrastructure
* Real-time AI systems
* Edge-computing workflows
* Adaptive optimization systems
* Embedded computer vision
* Traffic engineering simulation

---

# 🔬 Research Inspiration

The project was inspired by challenges in:

* Urban congestion management
* Inefficient fixed-time traffic systems
* Dynamic traffic optimization
* Intelligent infrastructure automation

---

# 🛠️ Future Improvements

Potential future enhancements:

* Live CCTV integration
* Multi-intersection optimization
* Cloud-hosted inference
* Reinforcement learning-based control
* Real-time analytics dashboard
* Edge-device acceleration
* Distributed traffic orchestration

---

# 👨‍💻 Team

* Ajay Kumar Mallameeda
* Jake Mathew
* Hemanth
* Aravind K
* Ruthvi M

---

# 📜 License

This project is intended for academic and research exploration purposes.
