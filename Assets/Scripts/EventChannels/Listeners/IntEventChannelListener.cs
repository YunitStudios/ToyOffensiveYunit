using UnityEngine;
using UnityEngine.Events;

public class IntEventChannelListener : MonoBehaviour
{

    [Header("Listen to Event Channels")]
    [SerializeField] private IntEventChannelSO m_EventChannel;
    [Space]
    [Tooltip("Responds to receiving signal from Event Channel")]
    [SerializeField] private UnityEvent<int> m_Response;

    private void OnEnable()
    {
        if (m_EventChannel != null)
            m_EventChannel.OnEventRaised += OnEventRaised;
    }

    private void OnDisable()
    {
        if (m_EventChannel != null)
            m_EventChannel.OnEventRaised -= OnEventRaised;
    }

    public void OnEventRaised(int value)
    {
        if (m_Response != null)
            m_Response.Invoke(value);
    }

}
