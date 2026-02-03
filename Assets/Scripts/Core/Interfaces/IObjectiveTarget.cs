using System;
using UnityEngine;

public interface IObjectiveTarget
{
    public event Action OnTargetComplete;
}
