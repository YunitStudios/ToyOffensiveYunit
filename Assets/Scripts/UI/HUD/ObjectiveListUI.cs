using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveListUI : MonoBehaviour
{

    [SerializeField] private RectTransform objectiveUIRoot;
    [SerializeField] private GameObject objectiveUIPrefab;

    private List<ObjectiveUI> objectiveUIs = new();


    private void Start()
    {
        MissionSO mission = MissionManager.CurrentMission;
        if (mission == null)
            return;
        
        CreateObjective(mission.MainObjective);
        
        foreach(var bonus in mission.BonusObjectives)
            CreateObjective(bonus);
    }

    private void CreateObjective(CoreObjectiveSO objective)
    {
        ObjectiveUI newUI = Instantiate(objectiveUIPrefab, objectiveUIRoot).GetComponent<ObjectiveUI>();
        newUI.Setup(objective);
        objectiveUIs.Add(newUI);
    }
}
