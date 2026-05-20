using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls traffic signal rotation across all intersection lanes.
///
/// Cycle behavior:
///   - Rotates green signal through [Right, Left, Front] in round-robin order
///   - Duration per lane is dynamically set from ReadInput (external optimizer interface)
///   - On each rotation: previous lane turns red, next lane turns green, delay reloaded
///
/// Signal index mapping:
///   trafficlights[0] = Right lane
///   trafficlights[1] = Left lane
///   trafficlights[2] = Front lane
/// </summary>
public class TrafficLightManager : MonoBehaviour
{
    [Header("Traffic Light Components")]
    public List<TrafficLight> trafficlights;
    public List<GameObject> lights;

    [Header("External Timing Input")]
    public ReadInput Input;

    // Cycle state
    public float delay;   // Current green duration (seconds) — set from ReadInput
    public float timer;   // Time elapsed in current green phase

    private int currentLaneIndex = 0;
    private int laneCount = 3;

    void Start()
    {
        // Initialize all signals to red
        for (int i = 0; i < laneCount; i++)
        {
            trafficlights[i] = lights[i].GetComponent<TrafficLight>();
            trafficlights[i].TurnRed();
        }

        // Start with lane 0 (Right) green
        currentLaneIndex = 0;
        trafficlights[currentLaneIndex].TurnGreen();
        SetDelay(currentLaneIndex);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > delay)
        {
            AdvanceToNextLane();
            timer = 0;
        }
    }

    private void AdvanceToNextLane()
    {
        int previousIndex = currentLaneIndex;
        currentLaneIndex = (currentLaneIndex + 1) % laneCount;

        trafficlights[previousIndex].TurnRed();
        trafficlights[currentLaneIndex].TurnGreen();
        SetDelay(currentLaneIndex);
    }

    /// <summary>
    /// Returns the name of the currently green lane (for UI display).
    /// </summary>
    public string GreenLane()
    {
        if (trafficlights[0].isGreen) return "Right";
        if (trafficlights[1].isGreen) return "Left";
        if (trafficlights[2].isGreen) return "Front";
        return "Unknown";
    }

    /// <summary>
    /// Loads green duration for the specified lane from ReadInput.
    /// Called on each signal rotation so durations reflect latest optimizer output.
    /// </summary>
    private void SetDelay(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: delay = Input.wait_R; break;
            case 1: delay = Input.wait_L; break;
            case 2: delay = Input.wait_F; break;
            default:
                Debug.LogWarning($"[TrafficLightManager] SetDelay called with unknown index: {laneIndex}");
                break;
        }
    }
}
