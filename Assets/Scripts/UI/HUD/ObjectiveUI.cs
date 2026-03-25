using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private CoreObjectiveSO objective;
    private RectTransform rectTransform;

    private bool isMain;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(CoreObjectiveSO newObjective, bool isMain)
    {
        if (newObjective == null)
            return;
        objective = newObjective;

        this.isMain = isMain;
        
        objective.OnObjectiveUpdated += UpdateText;
        objective.OnObjectiveCompleted += UpdateText;
        UpdateText();
    }
    private void OnDisable()
    {
        if (objective == null)
            return;
        
        objective.OnObjectiveUpdated -= UpdateText;
        objective.OnObjectiveCompleted -= UpdateText;
    }

    private void UpdateText()
    {
        string outputText = objective.GetObjectiveText();
        string prefix = isMain ? "<b><line-height=125%><size=125%>" : "";
        outputText = prefix + outputText;
        text.text = outputText;
        text.color = GetColor();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private Color GetColor()
    {
        if (objective.Failed) return Color.red;
        if (objective.Completed) return Color.green;
        return Color.white;
    }
}
