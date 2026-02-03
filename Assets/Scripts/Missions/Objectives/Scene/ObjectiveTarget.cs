using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectiveTarget : MonoBehaviour
{
    [SerializeField] private TargetObjectivesSO objective;
    [SerializeField] private SerializableInterface<IObjectiveTarget> target;

    private void OnEnable()
    {
        if (target.Instance.Equals(null))
        {
            enabled = false;
            return;
        }

        target.Instance.OnTargetComplete += CompleteTarget;
        objective.RegisterTarget(this);
    }

    private void OnDisable()
    {
        target.Instance.OnTargetComplete -= CompleteTarget;
    }

    public void CompleteTarget()
    {
        if(objective) 
            objective.CompleteTarget(this);
    }


    private void Reset()
    {
        StartCoroutine(FindInterfaceCoroutine());
    }

    private IEnumerator FindInterfaceCoroutine()
    {
        yield return null;
        
        if (TryGetComponent(out IObjectiveTarget targetInterface))
        {
            target.Instance = targetInterface;
        }
        else
            Debug.LogError("ObjectiveTarget script added to object with no IObjectiveTarget interface. You must manually assign the interface."); 
    }
}
