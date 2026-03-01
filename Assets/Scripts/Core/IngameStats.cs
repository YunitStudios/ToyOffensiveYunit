using System;
using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/IngameStats", fileName = "IngameStats")]
public class IngameStats : ScriptableObject
{
    [Title("Stats")]
    [field: SerializeField, DisableInPlayMode] public float ElapsedTime { get; private set; }
    [field: SerializeField, DisableInPlayMode] public int EnemyKills { get; private set; }
    [field: SerializeField, DisableInPlayMode] public int ShotsFired { get; private set; }
    [field: SerializeField, DisableInPlayMode] public int ShotsHit { get; private set; }
    [field: SerializeField, DisableInPlayMode] public float PlayerDamageTaken { get; private set; }
    [field: SerializeField, DisableInPlayMode] public float Accuracy { get; private set; }

    [Space,Space,Space]
    [Title("Event Binding")]
    [SerializeField] private FloatEventChannelSO onTimePassed;
    [SerializeField] private VoidEventChannelSO onEnemyKilled;
    [SerializeField] private VoidEventChannelSO onWeaponFired;
    [SerializeField] private VoidEventChannelSO onWeaponHit;
    [SerializeField] private FloatEventChannelSO onPlayerHealthChangedDifference;
    
    private void RevertToDefaultValues()
    {
        ElapsedTime = 0f;
        EnemyKills = 0;
        ShotsFired = 0;
        ShotsHit = 0;
        PlayerDamageTaken = 0f;
        Accuracy = 0f;
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
        onWeaponFired.OnEventRaised += WeaponFired;
        onWeaponHit.OnEventRaised += WeaponHit;
        onPlayerHealthChangedDifference.OnEventRaised += PlayerHealthChanged;
    }
    
    public void Stop()
    {
        onTimePassed.OnEventRaised -= TimePassed;
        onEnemyKilled.OnEventRaised -= EnemyKilled;
        onWeaponFired.OnEventRaised -= WeaponFired;
        onWeaponHit.OnEventRaised -= WeaponHit;
        onPlayerHealthChangedDifference.OnEventRaised -= PlayerHealthChanged;
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
    
    private void WeaponFired()
    {
        ShotsFired++;
        CalculateAccuracy();
    }
    
    private void WeaponHit()
    {
        // TODO Doesnt work with shotguns or piercing
        ShotsHit++;
        CalculateAccuracy();
    }

    private void CalculateAccuracy()
    {
        if (ShotsFired == 0)
        {
            Accuracy = 0f;
            return;
        }
        
        Accuracy = (float) ShotsHit / ShotsFired;
    }
    
    private void PlayerHealthChanged(float offset)
    {
        if (offset > 0)
            return;

        PlayerDamageTaken += Mathf.Abs(offset);
    }
    
}
