using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public abstract class CoreObjectiveSO : ScriptableObject
{
    [Header("Current Progress")]
    [field: SerializeField, HideInEditMode, DisableInPlayMode] public bool Active { get; private set; }
    public bool InProgress => Active && !(Completed || Failed);
    [field: SerializeField, HideInEditMode, DisableInPlayMode] public bool Completed { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode] public bool Failed { get; private set; }
    
    [Header("Info")]
    [SerializeField] protected string objectiveName;
    [SerializeField] protected string objectiveDesc;

    public Action OnObjectiveUpdated;
    public Action OnObjectiveCompleted;
    public Action OnObjectiveFailed;

    private List<ObjectiveObject> objectiveObjects = new();

    public abstract string ObjectiveText { get;}
    public abstract string ProgressText { get;}

    public void RegisterObject(ObjectiveObject obj)
    {
        objectiveObjects.Add(obj);
    }

    public void UnregisterObject(ObjectiveObject obj)
    {
        objectiveObjects.Remove(obj);
    }

    private void ToggleObjects(bool value)
    {
        foreach(ObjectiveObject obj in objectiveObjects)
            obj.gameObject.SetActive(value);
    }

    public virtual void SetupObjective()
    {
        ToggleObjects(true);
        Active = true;
        Completed = false;
        Failed = false;
    }

    public virtual void ResetObjective()
    {
        ToggleObjects(false);
        Active = false;
        Completed = false;
        Failed = false;
    }

    public string GetObjectiveText()
    {
        return ObjectiveText + GetProgressText();
    }

    private string GetProgressText()
    {
        if (ProgressText != string.Empty)
            return " [" + ProgressText + "]";

        return "";
    }

    public void CompleteObjective()
    {
        if (Failed || Completed)
            return;
        
        Completed = true;
        
        // TODO Give score
        
        OnObjectiveCompleted?.Invoke();
    }

    public void FailObjective()
    {
        if (Failed || Completed)
            return;
        
        Failed = true;
        Completed = false;
        
        OnObjectiveFailed?.Invoke();
    }
}

