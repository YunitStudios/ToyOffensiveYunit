using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ScoreTracker", fileName = "ScoreTracker")]
public class ScoreTrackerSO : ScriptableObject
{
    [Header("References")] 
    [SerializeField] private IngameStats stats;

    [Header("Score Values")] 
    [SerializeField] private SerializedDictionary<ScoreTypes, int> scoreValues;

    [Header("Multikill Attributes")] 
    [SerializeField] private float multikillBonus = 50;
    [SerializeField, Tooltip("How many seconds before the multikill expired")] private float multiKillTimeThreshold = 1;
    [SerializeField] private int multiKillMaxKills = 5;

    [Header("Bonus Attributes")] 
    [SerializeField] private float timerStartBonus = 6000f;
    [SerializeField] private float timerMinusPerSecond = 20f;
    [SerializeField] private float healthBonusPerPoints = 20f;
    [SerializeField] private float accuracyBonusPerPercentage = 20f;
    [SerializeField] private float allBonusObjectivesMultiplier = 1.25f;

    public float CurrentScore { get; private set; }

    public void AddScore(ScoreTypes type)
    {
        float newScore = 0;
        newScore += scoreValues.GetValueOrDefault(type);

        newScore +=  CalculateMultiKill();

        CurrentScore = newScore;
    }

    private float CalculateMultiKill()
    {
        return 0;
    }

    
    // Store values from each part of the scoring system
    public float TotalTimerBonus { get; private set; }
    public float TotalHealthBonus { get; private set; }
    public float TotalAccuracyBonus { get; private set; }

    private void ApplyBonus()
    {
        // Time Bonus
        int totalSeconds = Mathf.FloorToInt(stats.ElapsedTime);
        float timerReduction = totalSeconds * timerMinusPerSecond;
        // Limit to 0
        TotalTimerBonus = Mathf.Max(timerStartBonus - timerReduction, 0);
        
        // Health Bonus
        float finalHealth = GameManager.PlayerData.CurrentHealth;
        int flooredHealth = Mathf.FloorToInt(finalHealth);
        TotalHealthBonus = flooredHealth * healthBonusPerPoints;
        
        // Accuracy Bonus
        float accuracy = stats.Accuracy;
        float accuracyPercentage = accuracy * 100f;
        TotalAccuracyBonus = accuracyPercentage * accuracyBonusPerPercentage;

        CurrentScore = CurrentScore + TotalTimerBonus + TotalHealthBonus + TotalAccuracyBonus;

        // All clear multiplier


    }

    public enum ScoreTypes
    {
        MainObjective,
        BonusObjective1,
        BonusObjective2,
        BonusObjective3,
        GenericKill,
        ParachutingKill,
        EnvironmentalKill,
        BreakCamKill
    }
}
