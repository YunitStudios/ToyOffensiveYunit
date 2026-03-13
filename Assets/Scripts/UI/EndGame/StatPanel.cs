using System;
using System.Reflection;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatPanel : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private IngameStats stats;
    [SerializeField] private TMP_Text statTitle;
    [SerializeField] private TMP_Text statValue;
    [SerializeField] private Image divider;
    
    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private string statName;
    [SerializeField] private Color dividerColor;
    [SerializeField] private string targetString;
    [SerializeField] private string formatString;
    [SerializeField] private float scale = 1;
    
    private PropertyInfo property;


    private void Start()
    {
        // Attempt to bind stat name with field in ingamestats 
        Type statsType = typeof(IngameStats);
        property = statsType.GetProperty(statName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            Debug.LogError($"StatPanel: No field named {statName} found in IngameStats.");
            Destroy(gameObject);
            return;
        }

        divider.color = dividerColor;
    }

    private void Update()
    {
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        if (property == null) return;

        object value = property.GetValue(stats);
        
        // Scale the value
        if (value is float fValue)
        {
            value = fValue * scale;
        }

        string formattedValue;

        if (!string.IsNullOrEmpty(formatString) && value is IFormattable formattableValue)
        {
            formattedValue = formattableValue.ToString(formatString, null);
        }
        else
        {
            formattedValue = value.ToString();
        }

        if (!string.IsNullOrEmpty(targetString))
        {
            statValue.text = string.Format(targetString, formattedValue);
        }
        else
        {
            statValue.text = formattedValue;
        }
        
    }
}
