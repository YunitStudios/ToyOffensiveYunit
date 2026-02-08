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


    public void Init()
    {
        mainObjective.SetupObjective();
        foreach(var obj in bonusObjectives)
            obj.SetupObjective();
    }

    public void Clear()
    {
        mainObjective.ResetObjective();
        foreach(var obj in bonusObjectives)
            obj.ResetObjective();
    }
}
