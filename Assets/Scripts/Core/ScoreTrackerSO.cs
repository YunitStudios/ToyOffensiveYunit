using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AYellowpaper.SerializedCollections;
using PrimeTween;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ScoreTracker", fileName = "ScoreTracker")]
public class ScoreTrackerSO : ScriptableObject
{
    [Header("References")] 
    [SerializeField] private IngameStats stats;

    [Header("Settings")] 
    [SerializeField, Tooltip("Time before a multi-kill is no longer valid")] private float multiKillTimeThreshold = 3f;

    [Header("Score Values")] 
    [SerializeField] private SerializedDictionary<ScoreTypes, int> scoreValues;
    private int GetScoreValue(ScoreTypes type) => scoreValues.GetValueOrDefault(type);

    [Header("Bonus Attributes")] 
    [SerializeField] private float timerStartBonus = 6000f;
    [SerializeField] private float timerMinusPerSecond = 20f;
    [SerializeField] private float healthBonusPerPoints = 20f;
    [SerializeField] private float accuracyBonusPerPercentage = 20f;
    [SerializeField] private float allBonusObjectivesMultiplier = 1.25f;

    public static Action<List<ScoreTypes>, float> OnScoreAdded;

    public float CurrentScore { get; private set; }

    private Dictionary<IDamageSource, ActiveSourceData> activeDamageSources = new();

    public void AddScore(ScoreTypes type)
    {
        int value = GetScoreValue(type);
        AddScore(value);

        List<ScoreTypes> scoreTypes = new() { type };
        OnScoreAdded?.Invoke(scoreTypes, value);
    }
    private void AddScore(int value)
    {
        CurrentScore += value;
    }

    public void RegisterKill(KillTypes killType, IDamageSource source)
    {
        List<ScoreTypes> scoreTypes = new() { ScoreTypes.GenericKill };

        int killScore = GetScoreValue(ScoreTypes.GenericKill);

        if (killType != KillTypes.Generic)
        {
            ScoreTypes killScoreKill = killScoreTypes.GetValueOrDefault(killType);
            killScore += GetScoreValue(killScoreKill);
            scoreTypes.Add(killScoreKill);
        }
        
        if(source != null)
        {
            RegisterDamageSource(source);
            int multiKillCount = activeDamageSources[source].count;
            int multiKillScore = multiKillCount * GetScoreValue(ScoreTypes.MultiKill);
            killScore += multiKillScore;
            
            Debug.Log("Multi-Kill Count: " + multiKillCount + " | Multi-Kill Score: " + multiKillScore);
            
            if(multiKillScore > 0)
                scoreTypes.Add(ScoreTypes.MultiKill);
        }
        
        AddScore(killScore);

        OnScoreAdded?.Invoke(scoreTypes, killScore);
        
        Debug.Log("Registered Kill: " + killType + " | Score: " + killScore);
    }

    private void RegisterDamageSource(IDamageSource source)
    {
        if (activeDamageSources.TryGetValue(source, out var activeDamageSource))
        {
            activeDamageSource.count++;
            activeDamageSource.timer.Stop();
            activeDamageSource.timer = GetUnregisterTimer(source);
            return;
        }
        
        ActiveSourceData newSourceData = new()
        {
            count = 0,
            timer = GetUnregisterTimer(source)
        };
        activeDamageSources.Add(source, newSourceData);
    }

    private void UnregisterDamageSource(IDamageSource source)
    {
        activeDamageSources.Remove(source);
    }

    private Tween GetUnregisterTimer(IDamageSource source)
    {
        return Tween.Delay(multiKillTimeThreshold, () => UnregisterDamageSource(source));
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

    public class ActiveSourceData
    {
        public int count;
        public Tween timer;
    }

    private Dictionary<KillTypes, ScoreTypes> killScoreTypes = new()
    {
        { KillTypes.Generic, ScoreTypes.GenericKill },
        { KillTypes.Parachuting, ScoreTypes.ParachutingKill },
        { KillTypes.EnvironmentalKill, ScoreTypes.EnvironmentalKill },
        { KillTypes.BreakCam, ScoreTypes.BreakCamKill }
    };

    public enum ScoreTypes
    {
        MainObjective,
        BonusObjective1,
        BonusObjective2,
        BonusObjective3,
        GenericKill,
        ParachutingKill,
        EnvironmentalKill,
        BreakCamKill,
        MultiKill
    }

    public static string TypeToString(ScoreTypes type)
    {
        switch (type)
        {
            default:
                return "";
            case ScoreTypes.MainObjective:
                return "Main Objective";
            case ScoreTypes.BonusObjective1:
                return "Bonus Objective";
            case ScoreTypes.BonusObjective2:
                return "Bonus Objective";
            case ScoreTypes.BonusObjective3:
                return "Bonus Objective";
            case ScoreTypes.GenericKill:
                return "Enemy Killed";
            case ScoreTypes.ParachutingKill:
                return "Parachute Kill";
            case ScoreTypes.EnvironmentalKill:
                return "Environmental Kill";
            case ScoreTypes.BreakCamKill:
                return "Break Cam Kill";
            case ScoreTypes.MultiKill:
                return "Multi-Kill";
            
            
        }
    }

    public enum KillTypes
    {
        Generic,
        Parachuting,
        EnvironmentalKill,
        BreakCam
    }
}
