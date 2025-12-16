using System;
using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private CoreObjectiveSO objective;
    [SerializeField] private TMP_Text text;


    private void OnEnable()
    {
        if (objective == null)
            return;
        
        objective.OnObjectiveUpdated += UpdateText;
        objective.OnObjectiveCompleted += UpdateText;
    }
    private void OnDisable()
    {
        if (objective == null)
            return;
        
        objective.OnObjectiveUpdated -= UpdateText;
        objective.OnObjectiveCompleted -= UpdateText;
    }

    private void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = objective.GetObjectiveText();
        text.color = !objective.Completed ? Color.white : Color.green;
    }
}
