using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class ObjectiveListUI : MonoBehaviour
{

    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private RectTransform objectiveUIRoot;
    [SerializeField] private GameObject objectiveUIPrefab;
    [SerializeField] private VoidEventChannelSO onMissionStart;
    
    private List<ObjectiveUI> objectiveUIs = new();

    private void OnEnable()
    {
        onMissionStart.OnEventRaised += SetupObjectives;
    }
    
    private void OnDisable()
    {
        onMissionStart.OnEventRaised -= SetupObjectives;
    }


    private void SetupObjectives()
    {
        MissionSO mission = MissionManager.CurrentMission;
        if (mission == null)
            return;
        
        CreateObjective(mission.MainObjective, true);
        
        foreach(var bonus in mission.BonusObjectives)
            CreateObjective(bonus, false);
    }

    private void CreateObjective(CoreObjectiveSO objective, bool isMain)
    {
        ObjectiveUI newUI = Instantiate(objectiveUIPrefab, objectiveUIRoot).GetComponent<ObjectiveUI>();
        newUI.Setup(objective, isMain);
        objectiveUIs.Add(newUI);
    }
}
