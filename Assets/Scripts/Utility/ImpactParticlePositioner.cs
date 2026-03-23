using System;
using UnityEngine;
using UnityEngine.Events;

public class ImpactParticlePositioner : MonoBehaviour
{
    [SerializeField] private Vector3EventChannelSO inputEvent;
    [SerializeField] private ParticleSystem particles;

    private void OnEnable()
    {
        inputEvent.OnEventRaised += OnPositionObject;
    }

    private void OnDisable()
    {
        inputEvent.OnEventRaised -= OnPositionObject;
    }

    private void OnPositionObject(Vector3 pos)
    {
        transform.position = pos;
        var particle = Instantiate(particles, transform.position, Quaternion.identity);
        particle.Play();
        particle.transform.parent = null;
    }
}
