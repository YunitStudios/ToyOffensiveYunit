using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PrimeTween;

public class ScoreUI : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private CanvasFader fader;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text typesText;

    private bool active;
    private float currentScore;
    private Dictionary<ScoreTrackerSO.ScoreTypes, int> currentTypes;

    private void OnEnable()
    {
        ScoreTrackerSO.OnScoreAdded += NewScore;
        fader.OnFadeOutEnd += DisplayEnd;
    }

    private void OnDisable()
    {
        ScoreTrackerSO.OnScoreAdded -= NewScore;
        fader.OnFadeOutEnd -= DisplayEnd;
    }

    private void NewScore(List<ScoreTrackerSO.ScoreTypes> types, float value)
    {
        if (!active)
        {
            currentScore = 0;
            currentTypes = new();
        }

        currentScore += value;
        foreach(var type in types)
        {
            if (currentTypes.ContainsKey(type))
                currentTypes[type]++;
            else
                currentTypes.Add(type, 1);
        }

        fader.Play(CanvasFader.FadeType.Full);

        Tween.PunchScale(valueText.transform, new ShakeSettings(Vector3.one * 2, 0.2f, 1, true, Ease.Default, 0, 1, 0, 0, true));

        valueText.text = "+" + NumberUtility.FormatNumber(currentScore);

        string typesString = "";
        foreach (var typeData in currentTypes)
            typesString += ScoreTrackerSO.TypeToString(typeData.Key) + " x" + typeData.Value + "\n";
        typesText.text = typesString;

        active = true;
    }

    private void DisplayEnd()
    {
        active = false;
    }


}    
 