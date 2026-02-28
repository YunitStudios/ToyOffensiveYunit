using System;
using EditorAttributes;
using TMPro;
using UnityEngine;

public class MissionSetupUI : MonoBehaviour
{
    [Tooltip("Since we're only having 1 mission in this version I'll just have you manually set the mission here")]
    [SerializeField] private MissionSO activeMission;

    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private TMP_Text missionBrief;
    [SerializeField] private EntryPointUI[] entryPoints;
    [SerializeField] private TMP_Text entryPointIndexText;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField]
    private string missionBriefFormat = "Mission Brief\n" +
                                        "Main Objective:\n" +
                                        "{0}\n" +
                                        "Bonus Objectives:\n" +
                                        "{1}";

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        foreach(var entry in entryPoints)
            entry.SetMainScript(this);
        
        missionBrief.text = string.Format(
            missionBriefFormat, 
            activeMission.MainObjective.ObjectiveText, 
            activeMission.BonusObjectives.Count > 0 ? string.Join("\n", activeMission.BonusObjectives.ConvertAll(bonus => "- " + bonus.ObjectiveText)) : "None");
        
        Refresh();
    }

    public void Refresh()
    {
        for(int i = 0; i < entryPoints.Length; i++)
            entryPoints[i].ToggleBaseAlpha(i == activeMission.StartPointIndex);
        
        entryPointIndexText.text = ""+(activeMission.StartPointIndex+1);
    }


    public void SelectEntryPoint(EntryPointUI entryPoint)
    {
        int index = Array.IndexOf(entryPoints, entryPoint);
        if (index == -1)
        {
            Debug.LogError("Selected entry point is not in the list of entry points.");
            return;
        }

        activeMission.StartPointIndex = index;
        
        Refresh();
    }

    public void AdjustEntryPoint(int offset)
    {
        activeMission.StartPointIndex = Mathf.Clamp(activeMission.StartPointIndex + offset, 0, entryPoints.Length - 1);
        Refresh();
    }
    
}
