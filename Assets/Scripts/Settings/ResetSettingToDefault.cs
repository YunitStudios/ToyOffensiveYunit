using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetSettingToDefault : MonoBehaviour
{
    public string settingFieldName; // Match this to the field in GameSettings
    
    private GameSettings settings;
    private FieldInfo fieldInfo;

    public void Trigger()
    {
        settings = SettingsManager.Instance.GetSettings;

        fieldInfo = typeof(GameSettings).GetField(settingFieldName);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field '{settingFieldName}' not found in GameSettings.");
            return;
        }

        var defaultSettings = SettingsManager.Instance.GetDefaultSettings;
        var defaultValue = fieldInfo.GetValue(defaultSettings);
        fieldInfo.SetValue(settings, defaultValue);
        
        // Adjust UI on object
        if (TryGetComponent(out Toggle toggle))
        {
            toggle.isOn = (bool)fieldInfo.GetValue(settings);
        }
        else if (TryGetComponent(out Slider slider))
        {
            slider.value = (float)fieldInfo.GetValue(settings);
        }
        else if (TryGetComponent(out TMP_Dropdown dropdown))
        {
            dropdown.value = (int)fieldInfo.GetValue(settings);
        }
    }
}
