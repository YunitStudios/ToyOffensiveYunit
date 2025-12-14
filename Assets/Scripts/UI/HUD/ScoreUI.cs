using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private CanvasFader fader;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text typesText;
    
    private void OnEnable()
    {
        ScoreTrackerSO.OnScoreAdded += NewScore;
    }

    private void OnDisable()
    {
        ScoreTrackerSO.OnScoreAdded -= NewScore;
    }

    private void NewScore(List<ScoreTrackerSO.ScoreTypes> types, float value)
    {
        fader.PlayFull();
        valueText.text = "+" + value;

        string typesString = "";
        foreach (ScoreTrackerSO.ScoreTypes type in types)
            typesString += ScoreTrackerSO.TypeToString(type) + "\n";
        typesText.text = typesString;
    }


}    
 