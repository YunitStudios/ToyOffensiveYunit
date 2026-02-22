using EditorAttributes;
using UnityEngine;

public class HealthThresholdTriggerObjectiveSO : TriggerObjectivesSO
{

    [SerializeField, Range(0, 1)] private float healthThreshold;
    
    public override void TriggeredComplete()
    {
        // Dont trigger if health is too low
        if (GameManager.PlayerData.HealthPercentage < healthThreshold)
            return;
        
        base.TriggeredComplete();
    }
}
