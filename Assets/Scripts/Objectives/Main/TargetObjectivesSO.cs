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
    
    [SerializeField, DisableInEditMode, DisableInPlayMode] private SerializedDictionary<ObjectiveTarget, bool> targetStates = new();
    
    public int CompletedCount => targetStates.Values.Count(state => state);
    public int TargetCount => allTargets ? MaxTargetCount : goalCount;
    public int MaxTargetCount => targetStates.Count;

    public override string ProgressText => $" {CompletedCount}/{TargetCount}";
    
    public void RegisterTarget(ObjectiveTarget target)
    {
        targetStates.Add(target, false);
    }
    [Button]
    public void ClearTargets()
    {
        targetStates.Clear();
    }
    
    public void CompleteTarget(ObjectiveTarget target)
    {
        if (targetStates.ContainsKey(target))
            targetStates[target] = true;
        
        OnObjectiveUpdated?.Invoke();
        CheckCompleted();
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
