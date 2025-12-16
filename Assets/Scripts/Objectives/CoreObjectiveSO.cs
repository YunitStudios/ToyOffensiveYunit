using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CoreObjectiveSO : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string objectiveName;
    [SerializeField] private string objectiveDesc;

    [Header("Settings")] 
    [SerializeField] private bool isMainObjective = true;
    
    
    [field: SerializeField] public bool Active { get; private set; }
    [field: SerializeField] public bool Completed { get; private set; }

    public Action OnObjectiveUpdated;
    public Action OnObjectiveCompleted;

    private List<ObjectiveObject> objectiveObjects;

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

    public void SetupObjective()
    {
        ToggleObjects(true);
    }

    public void ClearObjective()
    {
        ToggleObjects(false);
    }

    public string GetObjectiveText()
    {
        return objectiveDesc + ProgressText;
    }

    public void CompleteObjective()
    {
        Completed = true;
        
        // TODO Give score
        
        OnObjectiveCompleted?.Invoke();
    }
}

