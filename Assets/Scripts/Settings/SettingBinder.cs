using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using TMPro;

public class SettingBinder : MonoBehaviour
{
    public string settingFieldName; // Match this to the field in GameSettings
    public TextMeshProUGUI label;
    
    private GameSettings settings;
    private FieldInfo fieldInfo;

    private void Start()
    {
        settings = SettingsManager.Instance.GetSettings;

        fieldInfo = typeof(GameSettings).GetField(settingFieldName);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field '{settingFieldName}' not found in GameSettings.");
            return;
        }

        // Bind to the correct UI type
        if (TryGetComponent(out Toggle toggle))
        {
            toggle.isOn = (bool)fieldInfo.GetValue(settings);
            if(label)
                label.text = toggle.isOn ? "On" : "Off";
            toggle.onValueChanged.AddListener(val =>
            {
                fieldInfo.SetValue(settings, val);
                if(label)
                    label.text = val ? "On" : "Off";
                SettingsManager.Instance.ApplySettingsToScene();
            });
        }
        else if (TryGetComponent(out Slider slider))
        {
            slider.value = (float)fieldInfo.GetValue(settings);
            if(label)
                label.text = slider.value.ToString("0.000");
            slider.onValueChanged.AddListener(val =>
            {
                fieldInfo.SetValue(settings, val);
                if(label)
                    label.text = val.ToString("0.000");
                SettingsManager.Instance.ApplySettingsToScene();
            });
        }
        else if (TryGetComponent(out TMP_Dropdown dropdown))
        {
            dropdown.value = (int)fieldInfo.GetValue(settings);
            if(label && dropdown.options.Count > dropdown.value)
                label.text = dropdown.options[dropdown.value].text;
            dropdown.onValueChanged.AddListener(val =>
            {
                fieldInfo.SetValue(settings, val);
                if(label)
                    label.text = dropdown.options[val].text;
                SettingsManager.Instance.ApplySettingsToScene();
            });
        }
        else
        {
            Debug.LogWarning("No supported UI component found on: " + gameObject.name);
        }
    }
}
