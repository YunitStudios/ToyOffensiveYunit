using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Void Event Channel", fileName = "VoidEventChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    [SerializeField, TextArea(2,4) ] private string description;
    
    [Tooltip("The action to perform")]
    public UnityAction OnEventRaised;

    public void Invoke()
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke();
    }
}

