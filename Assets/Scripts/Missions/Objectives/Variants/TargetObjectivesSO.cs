using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/TargetObjectives", fileName = "TargetObjectives")]
public class TargetObjectivesSO : CoreObjectiveSO
{
    [Header("Target")] 
    [SerializeField, Tooltip("If the objective requires all targets for completion")] private bool allTargets = true;
    [SerializeField, HideField(nameof(allTargets)), Tooltip("Number of targets to be completed before completing the objective")] private int goalCount;
    
    [SerializeField, HideInEditMode, DisableInPlayMode] protected SerializedDictionary<SerializableInterface<IObjectiveTarget>, bool> targetStates = new();
    
    [Header("Output Events")] 
    [SerializeField] private VoidEventChannelSO onFirstTargetComplete;
    [SerializeField] private VoidEventChannelSO onTargetComplete;
    
    public int CompletedCount => targetStates.Values.Count(state => state);
    public int TargetCount => allTargets ? MaxTargetCount : goalCount;
    public int MaxTargetCount => targetStates.Count;

    public override string ObjectiveText => objectiveDesc;
    public override string ProgressText => $"{CompletedCount}/{TargetCount}";

    private bool completedFirstTarget;
    
    public void RegisterTarget(SerializableInterface<IObjectiveTarget> target)
    {
        targetStates.Add(target, false);
    }

    public override void SetupObjective()
    {
        base.SetupObjective();
        completedFirstTarget = false;
    }

    public override void ResetObjective()
    {
        base.ResetObjective();
        targetStates.Clear();
    }
    
    public void CompleteTarget(SerializableInterface<IObjectiveTarget> target)
    {
        if (targetStates.ContainsKey(target))
            targetStates[target] = true;
        
        onTargetComplete?.Invoke();
        if (!completedFirstTarget)
        {
            onFirstTargetComplete?.Invoke();
            completedFirstTarget = true;
        }
        
        CheckCompleted();

        OnObjectiveUpdated?.Invoke();
    }

    private void CheckCompleted()
    {
        int completedCount = 0;
        
        // If any targets are still false
        foreach(bool state in targetStates.Values)
            if (state)
                completedCount++;

        if (allTargets && completedCount == MaxTargetCount)
            CompleteObjective();

        if (!allTargets &&
            (completedCount >= goalCount ||
             // Failsafe in case the target count is more than the number of targets
             completedCount >= MaxTargetCount))
            CompleteObjective();
    }

}
