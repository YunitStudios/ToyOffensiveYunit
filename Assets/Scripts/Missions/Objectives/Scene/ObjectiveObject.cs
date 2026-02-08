using System;
using UnityEngine;

public class ObjectiveObject : MonoBehaviour
{
    [SerializeField] private CoreObjectiveSO objective;

    private void Awake()
    {
        objective.RegisterObject(this);
    }

    private void OnDestroy()
    {
        objective.UnregisterObject(this);
    }
}
