using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Void Event Channel", fileName = "VoidEventChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    [SerializeField] private string description;
    
    [Tooltip("The action to perform")]
    public UnityAction OnEventRaised;

    public void Invoke()
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke();
    }
}

