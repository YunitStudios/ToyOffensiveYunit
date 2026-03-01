using UnityEngine;
using UnityEngine.Events;

public class Vector3EventChannelListener : MonoBehaviour
{

    [Header("Listen to Event Channels")]
    [SerializeField] private Vector3EventChannelSO m_EventChannel;
    [Space]
    [Tooltip("Responds to receiving signal from Event Channel")]
    [SerializeField] private UnityEvent<Vector3> m_Response;

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

    public void OnEventRaised(Vector3 value)
    {
        if (m_Response != null)
            m_Response.Invoke(value);
    }

}
