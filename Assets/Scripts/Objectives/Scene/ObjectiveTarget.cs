using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectiveTarget : MonoBehaviour
{
    [SerializeField] private TargetObjectivesSO objective;

    private void Awake()
    {
        objective.RegisterTarget(this);
    }

    public void CompleteTarget()
    {
        if(objective) 
            objective.CompleteTarget(this);
    }

}
