using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Mission", fileName = "Mission")]
public class MissionSO : ScriptableObject
{
    [SerializeField] private CoreObjectiveSO mainObjective;

    public CoreObjectiveSO MainObjective => mainObjective;
    [SerializeField] private List<CoreObjectiveSO> bonusObjectives;
    public List<CoreObjectiveSO> BonusObjectives => bonusObjectives;

    [HideInEditMode, DisableInPlayMode] public List<MissionPOI> startPoints = new();
    [HideInEditMode, DisableInPlayMode] public List<MissionPOI> extractPoints = new();


    public void Init()
    {
        mainObjective.SetupObjective();
        foreach(var obj in bonusObjectives)
            obj.SetupObjective();
        
        foreach(var poi in startPoints)
            poi.SetupPOI();
        foreach(var poi in extractPoints)
            poi.SetupPOI();
    }

    public void Clear()
    {
        mainObjective.ResetObjective();
        foreach(var obj in bonusObjectives)
            obj.ResetObjective();
        
        startPoints = new();
        extractPoints = new();
    }
    
    public Vector3 GetStartPosition => startPoints.Count > 0 ? startPoints[Random.Range(0, startPoints.Count)].GetPoint() : PlayerMovement.NULL_POSITION;
}