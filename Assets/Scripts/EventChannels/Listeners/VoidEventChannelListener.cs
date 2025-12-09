using UnityEngine;
using UnityEngine.Events;

public class VoidEventChannelListener : MonoBehaviour
{
    [Header("Listening")]
    [SerializeField] private VoidEventChannelSO eventChannel;
    [Header("Listening")]
    [SerializeField] private UnityEvent onRaised;

    private void OnEnable()
    {
        if (eventChannel != null)
            eventChannel.OnEventRaised += OnEventRaised;
    }

    private void OnDisable()
    {
        if (eventChannel != null)
            eventChannel.OnEventRaised -= OnEventRaised;
    }

    // Raises an event after a delay
    private void OnEventRaised()
    {
        onRaised.Invoke();
    }

}
