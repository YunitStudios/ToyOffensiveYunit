using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ClimbingPromptUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        OnTogglePrompt += TogglePrompt;
    }

    private void OnDisable()
    {
        OnTogglePrompt -= TogglePrompt;
    }

    // I would make a proper modular prompt system but AHHH DEADLINES

    public static Action<bool> OnTogglePrompt;


    private void TogglePrompt(bool value)
    {
        canvasGroup.alpha = value ? 1 : 0;
    }
    
}
