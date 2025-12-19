using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private CoreObjectiveSO objective;
    private RectTransform rectTransform;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(CoreObjectiveSO newObjective)
    {
        if (newObjective == null)
            return;
        objective = newObjective;
        
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
