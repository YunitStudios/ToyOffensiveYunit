using System;
using System.Reflection;
using EditorAttributes;
using TMPro;
using UnityEngine;

public class StatPanel : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private IngameStats stats;
    [SerializeField] private TMP_Text statTitle;
    [SerializeField] private TMP_Text statValue;
    
    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private string statName;

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
        

    }

    private void Update()
    {
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        statTitle.text = statName;
        statValue.text = property.GetValue(stats)?.ToString() ?? "N/A";
    }
}
