using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectiveTarget : MonoBehaviour
{
    private TargetObjectivesSO[] objectives;
    private SerializableInterface<IObjectiveTarget> target;

    private void Awake()
    {
        if(TryGetComponent<IObjectiveTarget>(out var newTarget))
        {
            SerializableInterface<IObjectiveTarget> serializedTarget = new();
            serializedTarget.Instance = newTarget;
            target = serializedTarget;
        }
    }

    public void Setup(TargetObjectivesSO[] objectives)
    {
        if (target.Instance.Equals(null))
        {
            enabled = false;
            return;
        }

        this.objectives = objectives;

        target.Instance.OnTargetComplete += CompleteTarget;
        foreach (var objective in objectives)
            objective.RegisterTarget(target);
    }

    private void OnDisable()
    {
        target.Instance.OnTargetComplete -= CompleteTarget;
    }

    public void CompleteTarget()
    {
        if(objectives is { Length: > 0 })
            foreach (var objective in objectives)
                objective.CompleteTarget(target);
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
