using System;
using System.Linq;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    private bool showPanel = false;
    private float deltaTime;
    private readonly int[] fpsOptions = { 30, 60, 120, 144, 240 };
    private int selectedFPSIndex = 1;
    private bool vSyncOn;

    private bool isPaused = false;

    private void Start()
    {
        InputManager.Instance.OnDebug += TogglePanel;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnDebug -= TogglePanel;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // set fps
        Application.targetFrameRate = fpsOptions[selectedFPSIndex];

        // set vsync
        QualitySettings.vSyncCount = vSyncOn ? 1 : 0;

        // pause or unpause time
        Time.timeScale = isPaused ? 0f : 1f;
    }

    void OnGUI()
    {
        if (!showPanel) return;

        int x = 10, y = 10;
        GUI.Box(new Rect(x - 5, y - 5, 300, 320), "Debug Panel");

        // show fps
        float fps = 1.0f / deltaTime;
        GUI.Label(new Rect(x, y, 200, 20), $"FPS: {fps:0}");

        // show frametime
        float cpuFrameTime = Time.deltaTime * 1000f;
        GUI.Label(new Rect(x, y + 20, 200, 20), $"CPU Frame Time: {cpuFrameTime:0.00} ms");

        // different fps limits
        GUI.Label(new Rect(x, y + 45, 200, 20), "FPS Limit:");
        selectedFPSIndex = GUI.SelectionGrid(
            new Rect(x, y + 65, 220, 30),
            selectedFPSIndex,
            fpsOptions.Select(f => f.ToString()).ToArray(),
            fpsOptions.Length
        );

        // vsync toggle
        vSyncOn = GUI.Toggle(new Rect(x, y + 100, 200, 20), vSyncOn, "VSync");

        // pause time toggle
        if (GUI.Button(new Rect(x, y + 130, 100, 30), isPaused ? "Unpause" : "Pause"))
        {
            isPaused = !isPaused;
        }
    }

    public void TogglePanel()
    {
        showPanel = !showPanel;
        // isPaused = showPanel;

        if (showPanel)
        {
            InputManager.Instance.ToggleCursor(true);
        }
        else
        {
            InputManager.Instance.ToggleCursor(false);
        }
    }
}
