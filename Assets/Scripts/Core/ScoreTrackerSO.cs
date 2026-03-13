using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AYellowpaper.SerializedCollections;
using PrimeTween;
using Throwable_System.ThrowableTypes;
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
    public int GetScoreValue(ScoreTypes type) => scoreValues.GetValueOrDefault(type);
    public int GetScoreValue(ScoreTypes type, int count) => scoreValues.GetValueOrDefault(type) * count;

    [Header("Bonus Attributes")] 
    [SerializeField] private float timerStartBonus = 6000f;
    [SerializeField] private float timerMinusPerSecond = 20f;
    [SerializeField] private float healthBonusPerPoints = 20f;
    [SerializeField] private float accuracyBonusPerPercentage = 20f;
    [SerializeField] private float allBonusObjectivesMultiplier = 1.25f;
    [Tooltip("How far should the player and kill height be to trigger above/below events")]
    [SerializeField] private float heightDifferenceThreshold = 0.5f;
    [SerializeField] private float targetKillMultiplier = 1.2f;

    [Header("Input Events")] 
    [SerializeField] private VoidEventChannelSO OnMainObjectiveCompleted;
    [SerializeField] private VoidEventChannelSO OnBonusObjectiveCompleted;
    
    [Header("Output Events")] 
    [SerializeField] private VoidEventChannelSO OnGenericKillAbove;
    [SerializeField] private VoidEventChannelSO OnGenericKillBelow;
    [SerializeField] private VoidEventChannelSO OnTargetKillAbove;
    [SerializeField] private VoidEventChannelSO OnTargetKillBelow;
    [SerializeField] private VoidEventChannelSO OnMultiKillWithHazard;
    [SerializeField] private VoidEventChannelSO OnMultiKillWithGadget;
    [SerializeField] private VoidEventChannelSO On3KillsWithGrenade;
    

    public static Action<List<ScoreTypes>, float> OnScoreAdded;

    public int CurrentScore { get; private set; }

    private Dictionary<IDamageSource, ActiveSourceData> activeDamageSources = new();
    public Dictionary<ScoreTypes, RuntimeScoreData> RuntimeScoreCounts = new();

    public struct RuntimeScoreData
    {
        public int count;
        public int score;
            public RuntimeScoreData(int count, int score)
            {
                this.count = count;
                this.score = score;
            }
    }

    public void Init()
    {
        OnMainObjectiveCompleted.OnEventRaised += AddMainObjective;
        OnBonusObjectiveCompleted.OnEventRaised += AddBonusObjective;
    }
    public void Start()
    {
        CurrentScore = 0;
        RuntimeScoreCounts = new();
    }

    public void Reset()
    {
        OnMainObjectiveCompleted.OnEventRaised -= AddMainObjective;
        OnBonusObjectiveCompleted.OnEventRaised -= AddBonusObjective;
    }

    public void AddScore(ScoreTypes type)
    {
        int value = GetScoreValue(type);
        AddScore(value);

        List<ScoreTypes> scoreTypes = new() { type };
        OnScoreAdded?.Invoke(scoreTypes, value);
        UpdateScoreCount(type);
    }
    private void AddScore(int value)
    {
        CurrentScore += value;
    }

    private void UpdateScoreCounts(List<ScoreTypes> types)
    {
        foreach (ScoreTypes type in types)
            UpdateScoreCount(type);
    }

    private void UpdateScoreCount(ScoreTypes type)
    {
        int scoreValue = GetScoreValue(type);
        if (!RuntimeScoreCounts.TryAdd(type, new RuntimeScoreData(1, scoreValue)))
        {
            RuntimeScoreData data = RuntimeScoreCounts[type];
            data.count++;
            data.score += scoreValue;
            RuntimeScoreCounts[type] = data;
        }
    }
    private void UpdateScoreCount(ScoreTypes type, int count, int score)
    {
        if (!RuntimeScoreCounts.TryAdd(type, new RuntimeScoreData(count, score)))
        {
            RuntimeScoreData data = RuntimeScoreCounts[type];
            data.count += count;
            data.score += score;
            RuntimeScoreCounts[type] = data;
        }
    }

    private void AddMainObjective()
    {
        AddScore(ScoreTypes.MainObjective);
    }
    private void AddBonusObjective()
    {
        AddScore(ScoreTypes.BonusObjective);
    }

    public void RegisterKill(KillTypes killType, IDamageSource source, bool wasTarget = false)
    {
        List<ScoreTypes> scoreTypes = new() { ScoreTypes.Kill };
        
        if(wasTarget)
            scoreTypes.Add(ScoreTypes.TargetKill);

        int killScore = GetScoreValue(ScoreTypes.Kill);

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
            
            // Hardcoded trigger events
            if(multiKillCount >= 2 && source is ExplosiveGrenade)
                On3KillsWithGrenade?.Invoke();
                
        }

        if (wasTarget)
        {
            int oldScore = killScore;
            killScore = Mathf.RoundToInt(killScore * targetKillMultiplier);
            int targetKillDifference = killScore - oldScore;
            UpdateScoreCount(ScoreTypes.TargetBonus, 0, targetKillDifference);
        }
        
        AddScore(killScore);

        OnScoreAdded?.Invoke(scoreTypes, killScore);
        
        UniqueKillConditions(killType, source, scoreTypes);

        UpdateScoreCounts(scoreTypes);
        
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
    
    private void UniqueKillConditions(KillTypes killType, IDamageSource source, List<ScoreTypes> scoreTypes)
    {
        // If generic kill and player is above the damage source
        if (killType == KillTypes.Generic && source != null)
        {
            float yDifference = GameManager.PlayerData.PlayerPosition.y - source.transform.position.y;
            if (yDifference > heightDifferenceThreshold)
            {
                if(scoreTypes.Contains(ScoreTypes.TargetKill))
                {
                    OnTargetKillAbove?.Invoke();
                    Debug.Log("Target Kill Above Triggered");
                }
                OnGenericKillAbove?.Invoke();
            }
            if (yDifference < heightDifferenceThreshold)
            {
                if (scoreTypes.Contains(ScoreTypes.TargetKill))
                {
                    OnTargetKillBelow?.Invoke();
                    Debug.Log("Target Kill Below Triggered");
                }
                OnGenericKillBelow?.Invoke();
            }
        }
        
        // If multi kill with hazard or gadget
        if (source != null && activeDamageSources.TryGetValue(source, out var activeSourceData))
        {
            if (activeSourceData.count > 1)
            {
                //if (source is IHazard)
                //{
                //    OnMultiKillWithHazard?.Invoke();
                //}
                //else if (source is IGadget)
                //{
                //    OnMultiKillWithGadget?.Invoke();
                //}
            }
        }
    }


    
    // Store values from each part of the scoring system
    public float TotalTimerBonus { get; private set; }
    public float TotalHealthBonus { get; private set; }
    public float TotalAccuracyBonus { get; private set; }

    public void ApplyBonus()
    {
        // Time Bonus
        int totalSeconds = Mathf.FloorToInt(stats.ElapsedTime);
        float timerReduction = totalSeconds * timerMinusPerSecond;
        // Limit to 0
        TotalTimerBonus = Mathf.Max(timerStartBonus - timerReduction, 0);
        UpdateScoreCount(ScoreTypes.TimeBonus, 0, Mathf.RoundToInt(TotalTimerBonus));
        
        // Health Bonus
        float finalHealth = GameManager.PlayerData.CurrentHealth;
        int flooredHealth = Mathf.FloorToInt(finalHealth);
        TotalHealthBonus = flooredHealth * healthBonusPerPoints;
        UpdateScoreCount(ScoreTypes.HealthBonus, 0, Mathf.RoundToInt(TotalHealthBonus));
        
        // Accuracy Bonus
        float accuracy = stats.Accuracy;
        float accuracyPercentage = accuracy * 100f;
        TotalAccuracyBonus = accuracyPercentage * accuracyBonusPerPercentage;
        UpdateScoreCount(ScoreTypes.AccuracyBonus, 0, Mathf.RoundToInt(TotalAccuracyBonus));
        
        CurrentScore = Mathf.RoundToInt(CurrentScore + TotalTimerBonus + TotalHealthBonus + TotalAccuracyBonus);
        
        // All clear multiplier
        bool allBonuses = true;
        foreach(var bonus in MissionManager.CurrentMission.BonusObjectives)
            if (!bonus.Completed)
                allBonuses = false;

        int oldScore = CurrentScore;
        if (allBonuses)
        {
            CurrentScore = Mathf.RoundToInt(CurrentScore * allBonusObjectivesMultiplier);
            int scoreDifference = CurrentScore - oldScore;
            UpdateScoreCount(ScoreTypes.AllClearedBonus, 0, scoreDifference); 
        }
        



    }

    public class ActiveSourceData
    {
        public int count;
        public Tween timer;
    }

    private Dictionary<KillTypes, ScoreTypes> killScoreTypes = new()
    {
        { KillTypes.Generic, ScoreTypes.Kill },
        { KillTypes.Parachuting, ScoreTypes.ParachutingKill },
        { KillTypes.EnvironmentalKill, ScoreTypes.EnvironmentalKill },
        { KillTypes.BreakCam, ScoreTypes.BreakCamKill }
    };

    public enum ScoreTypes
    {
        MainObjective,
        BonusObjective,
        Kill,
        ParachutingKill,
        EnvironmentalKill,
        BreakCamKill,
        MultiKill,
        TargetKill,
        TargetBonus,
        TimeBonus,
        HealthBonus,
        AccuracyBonus,
        AllClearedBonus
    }

    public static string TypeToString(ScoreTypes type)
    {
        switch (type)
        {
            default:
                return "";
            case ScoreTypes.MainObjective:
                return "Main Objective";
            case ScoreTypes.BonusObjective:
                return "Bonus Objective";
            case ScoreTypes.Kill:
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
