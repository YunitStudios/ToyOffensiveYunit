using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class monobeaviourscript : MonoBehaviour
{
    public string targetString;

    private List<char> buffer = new();

    public UnityEvent OnToggle;
    public UnityEvent OnUntoggle;
    private bool toggled;
    
    private void OnEnable()
    {
        Keyboard.current.onTextInput += CheckInput;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= CheckInput;
    }

    private void CheckInput(char c)
    {
        // Force lowercase
        c = char.ToLower(c);
        
        buffer.Add(c);

        // Check if buffer matches target string
        if (buffer.Count > targetString.Length)
            buffer.RemoveAt(0);

        if (new string(buffer.ToArray()) != targetString)
            return;

        if (!toggled)
        {
            toggled = true;
            OnToggle?.Invoke();
        }
        else
        {
            toggled = false;
            OnUntoggle?.Invoke();
        }

        buffer.Clear();
    }
}
