using System;
using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBar : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private Image reloadBar;
    [SerializeField] private CanvasFader canvasFader;
    
    [Title("\n<b><color=#8880ff>Input Events", 15, 5, false)] 
    [SerializeField] private FloatEventChannelSO onUpdateReloadProgress;

    private bool active;

    private void OnEnable()
    {
        onUpdateReloadProgress.OnEventRaised += UpdateReloadBar;
    }

    private void OnDisable()
    {
        onUpdateReloadProgress.OnEventRaised -= UpdateReloadBar;
    }


    private void UpdateReloadBar(float progress)
    {
        // Skip if already full
        if (!active && progress > 1)
            return;
        
        // Start reload bar
        if (!active && progress is > 0 and <= 1)
        {
            active = true;
            canvasFader.PlayIn();
        }
        
        // End reload bar
        if (active && progress > 1)
        {
            active = false;
            canvasFader.PlayOut();
        }

        reloadBar.fillAmount = progress;
    }
}
