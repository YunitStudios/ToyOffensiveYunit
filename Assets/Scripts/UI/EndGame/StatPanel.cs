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
        // If value is float, round to 2 DP
        if (property.PropertyType == typeof(float))
        {
            float value = (float)(property.GetValue(stats) ?? 0f);
            statValue.text = value.ToString("F2");
        }
        else
        {
            statValue.text = property.GetValue(stats)?.ToString() ?? "N/A";
        }
        
    }
}
