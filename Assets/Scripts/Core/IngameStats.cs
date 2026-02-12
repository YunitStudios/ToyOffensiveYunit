using System;
using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/IngameStats", fileName = "IngameStats")]
public class IngameStats : ScriptableObject
{
    [Title("Stats")]
    [field: SerializeField, DisableInPlayMode] public float ElapsedTime { get; private set; }
    [field: SerializeField, DisableInPlayMode] public int EnemyKills { get; private set; }
    [Space,Space,Space]
    [Title("Event Binding")]
    [SerializeField] private FloatEventChannelSO onTimePassed;
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    public float Accuracy;
    
    private void RevertToDefaultValues()
    {
        ElapsedTime = 0f;
        EnemyKills = 0;
    }

    public void Init()
    {
        RevertToDefaultValues();
    }

    public void Start()
    {
        RevertToDefaultValues();
        onTimePassed.OnEventRaised += TimePassed;
        onEnemyKilled.OnEventRaised += EnemyKilled;
    }

    public void Stop()
    {
        onTimePassed.OnEventRaised -= TimePassed;
        onEnemyKilled.OnEventRaised -= EnemyKilled;
    }

    public void Reset()
    {
        onTimePassed.OnEventRaised -= TimePassed;
        onEnemyKilled.OnEventRaised -= EnemyKilled;
    }

    private void TimePassed(float val)
    {
        ElapsedTime += val;
    }

    private void EnemyKilled()
    {
        EnemyKills++;
    }
    
}
