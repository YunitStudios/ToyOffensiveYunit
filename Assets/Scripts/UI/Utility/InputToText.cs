using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputToText : MonoBehaviour
{
    public InputActionReference action;
    public string prefix;
    public string suffix;
        
    private TMP_Text text;

    private void Awake()
    {
        if(!text)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        SetText();
    }

    private void OnValidate()
    {
        if(!text)
            text = GetComponent<TMP_Text>();
        SetText();
    }

    private void SetText()
    {
        if (!text || !action || !PlayerInput.GetPlayerByIndex(0))
            return;
        
        int bindingIndex = action.action.GetBindingIndex(group: PlayerInput.GetPlayerByIndex(0).currentControlScheme); 
        var displayString = action.action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);
        text.text = prefix + displayString + suffix;  
    }
}
