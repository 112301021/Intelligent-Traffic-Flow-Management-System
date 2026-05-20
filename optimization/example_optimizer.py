"""
example_optimizer.py
--------------------
Example external optimizer that writes signal timing to Input.txt.
The Unity simulation reads this file and updates signal durations in real-time.

This script demonstrates the optimizer-simulation interface.
Replace the timing logic with your actual optimization strategy:
  - YOLO-based vehicle counting
  - Reinforcement learning policy
  - Density-weighted allocation
  - Manual testing values

Interface contract:
  File: config/Input.txt
  Format:
    Line 1: L-R-F
    Line 2: <left_green>-<right_green>-<front_green>
  Example:
    L-R-F
    3.5-2.0-4.0
"""

import time
import os


CONFIG_PATH = os.path.join(os.path.dirname(__file__), "config", "Input.txt")


def write_timings(left: float, right: float, front: float, path: str = CONFIG_PATH) -> None:
    """
    Write signal timing values to the config file.
    Unity simulation picks this up within one frame (~16ms at 60fps).

    Args:
        left:  Green duration for Left lane (seconds)
        right: Green duration for Right lane (seconds)
        front: Green duration for Front lane (seconds)
        path:  Path to Input.txt (defaults to config/Input.txt)
    """
    with open(path, "w") as f:
        f.write("L-R-F\n")
        f.write(f"{left:.2f}-{right:.2f}-{front:.2f}\n")
    print(f"[Optimizer] Wrote timings → L={left:.2f}s  R={right:.2f}s  F={front:.2f}s")


def density_weighted_timing(
    count_left: int,
    count_right: int,
    count_front: int,
    min_green: float = 1.5,
    max_green: float = 8.0,
    total_cycle: float = 18.0
) -> tuple[float, float, float]:
    """
    Compute signal durations proportional to lane vehicle counts.
    
    Allocates green time from a fixed total cycle duration.
    Lanes with more vehicles get proportionally longer green phases.
    All lanes guaranteed a minimum green duration.

    Args:
        count_left:  Number of vehicles waiting in Left lane
        count_right: Number of vehicles waiting in Right lane
        count_front: Number of vehicles waiting in Front lane
        min_green:   Minimum green duration per lane (seconds)
        max_green:   Maximum green duration per lane (seconds)
        total_cycle: Total cycle duration to distribute (seconds)

    Returns:
        Tuple of (left, right, front) green durations in seconds
    """
    total = count_left + count_right + count_front

    if total == 0:
        # Equal distribution when no vehicles detected
        base = total_cycle / 3
        return base, base, base

    # Proportional allocation
    left_time  = max(min_green, min(max_green, (count_left  / total) * total_cycle))
    right_time = max(min_green, min(max_green, (count_right / total) * total_cycle))
    front_time = max(min_green, min(max_green, (count_front / total) * total_cycle))

    return left_time, right_time, front_time


# ── Example Usage ──────────────────────────────────────────────────────────────

if __name__ == "__main__":
    print("Traffic Signal Optimizer — Example Script")
    print(f"Writing to: {CONFIG_PATH}\n")

    # Example 1: Static equal timing (baseline)
    print("=== Test 1: Equal timing (3s each) ===")
    write_timings(left=3.0, right=3.0, front=3.0)
    time.sleep(5)

    # Example 2: Bias toward Left lane (simulating congestion)
    print("\n=== Test 2: Left lane congested ===")
    write_timings(left=6.0, right=2.0, front=2.0)
    time.sleep(8)

    # Example 3: Density-proportional (simulated counts)
    print("\n=== Test 3: Density-weighted (L=8, R=2, F=5 vehicles) ===")
    simulated_left  = 8
    simulated_right = 2
    simulated_front = 5

    l, r, f = density_weighted_timing(simulated_left, simulated_right, simulated_front)
    write_timings(l, r, f)
    time.sleep(10)

    # Reset to balanced
    print("\n=== Reset: Equal timing ===")
    write_timings(3.0, 3.0, 3.0)
    print("\nDone. Unity simulation updated.")
