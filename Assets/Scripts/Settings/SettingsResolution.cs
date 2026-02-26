using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingsResolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private GameSettings settings;

    private List<Resolution> filteredResolutions;
    private int currentResolutionIndex = 0;

void Start()
    {
        Resolution[] allResolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        HashSet<string> seenResolutions = new HashSet<string>();
        
        resolutionDropdown.ClearOptions();

        // Create set of all unique resolutions available
        for (int i = 0; i < allResolutions.Length; i++)
        {
            string resKey = allResolutions[i].width + "x" + allResolutions[i].height;
            if (!seenResolutions.Contains(resKey))
            {
                filteredResolutions.Add(allResolutions[i]);
                seenResolutions.Add(resKey);
            }
        }

        // Sort from highest to lowest
        filteredResolutions.Sort((a, b) => {
            if (a.width != b.width) return b.width.CompareTo(a.width);
            return b.height.CompareTo(a.height);
        });
        
        List<string> options = new List<string>();
        
        
        int savedWidth = settings.resolutionWidth;
        int savedHeight = settings.resolutionHeight;

        // Find current resolution
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            options.Add(filteredResolutions[i].width + "x" + filteredResolutions[i].height);

            // Find current resolution set in settings
            if (filteredResolutions[i].width == savedWidth && 
                filteredResolutions[i].height == savedHeight)
            {
                currentResolutionIndex = i;
            }
            // Or check if it matches the current resolution
            else if (savedWidth == 0 && 
                     filteredResolutions[i].width == Screen.currentResolution.width && 
                     filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        settings.resolutionHeight = resolution.height;
        settings.resolutionWidth = resolution.width;
        
        if(SettingsManager.Instance)
            SettingsManager.Instance.ApplySettingsToScene();
    }
}