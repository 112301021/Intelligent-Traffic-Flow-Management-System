using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Reads adaptive signal timing values from an external config file.
/// Acts as the IPC bridge between an external optimizer process and the Unity simulation.
///
/// Interface contract:
///   File format — Line 1: "L-R-F" (header)
///                 Line 2: "float-float-float" (green durations in seconds)
///   Example:      "L-R-F\n3.5-2.0-4.0"
///
/// The file is polled each frame by last-modified timestamp.
/// Any external process (Python optimizer, manual edit, etc.) can write this file
/// to update signal timing without modifying simulation code.
/// </summary>
public class ReadInput : MonoBehaviour
{
    [Tooltip("Path to the signal timing config file. Defaults to config/Input.txt in the project root.")]
    [SerializeField] private string relativeConfigPath = "config/Input.txt";

    private string filepath;
    private System.DateTime lastModifiedTime;

    /// <summary>Green duration for the Left lane (seconds).</summary>
    public float wait_L;

    /// <summary>Green duration for the Right lane (seconds).</summary>
    public float wait_R;

    /// <summary>Green duration for the Front lane (seconds).</summary>
    public float wait_F;

    void Awake()
    {
        // Resolve path relative to project root (one level above Assets/)
        filepath = Path.Combine(Application.dataPath, "..", relativeConfigPath);
        filepath = Path.GetFullPath(filepath);

        if (!File.Exists(filepath))
        {
            Debug.LogError($"[ReadInput] Config file not found at: {filepath}\n" +
                           "Create config/Input.txt with format: L-R-F\\n3.0-3.0-3.0");
            return;
        }

        lastModifiedTime = File.GetLastWriteTime(filepath);
        ReadConfigFile(filepath);
    }

    void Update()
    {
        if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
            return;

        System.DateTime currentModifiedTime = File.GetLastWriteTime(filepath);
        if (currentModifiedTime != lastModifiedTime)
        {
            ReadConfigFile(filepath);
            lastModifiedTime = currentModifiedTime;
        }
    }

    /// <summary>
    /// Parses the config file and updates wait time values.
    /// Expected format:
    ///   Line 1: L-R-F
    ///   Line 2: 3.5-2.0-4.0
    /// </summary>
    private void ReadConfigFile(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);

            if (lines.Length < 2)
            {
                Debug.LogWarning("[ReadInput] Config file has fewer than 2 lines. Expected: header + values.");
                return;
            }

            // Skip line 0 (header "L-R-F"), parse line 1
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = lines[i].Split('-');
                if (values.Length != 3)
                {
                    Debug.LogWarning($"[ReadInput] Line {i} has {values.Length} values, expected 3. Format: wait_L-wait_R-wait_F");
                    return;
                }

                if (float.TryParse(values[0], out float l) &&
                    float.TryParse(values[1], out float r) &&
                    float.TryParse(values[2], out float f))
                {
                    wait_L = Mathf.Max(0.1f, l);
                    wait_R = Mathf.Max(0.1f, r);
                    wait_F = Mathf.Max(0.1f, f);
                }
                else
                {
                    Debug.LogWarning($"[ReadInput] Failed to parse timing values from: '{lines[i]}'");
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"[ReadInput] IO error reading config: {e.Message}");
        }
    }
}
