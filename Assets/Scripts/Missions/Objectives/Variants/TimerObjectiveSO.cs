using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/TimerObjectives", fileName = "TimerObjectives")]
public class TimerObjectiveSO : CoreObjectiveSO
{
    [Header("Timer")] 
    [SerializeField] private int maxTime;
    [SerializeField, Tooltip("Begins timer or sets it back to 0")] private VoidEventChannelSO startTimerEvent;
    [SerializeField, Tooltip("End the timer and check if they were quick enough")] private VoidEventChannelSO endTimerEvent;

    private float currentTime;
    private Sequence timerSequence;

    public override string ObjectiveText => string.Format(objectiveDesc, maxTime % 60, maxTime / 60);
    public override string ProgressText => $"{(int)currentTime}/{maxTime} sec";

    public override void SetupObjective()
    {
        base.SetupObjective();
        currentTime = 0;
        startTimerEvent.OnEventRaised += StartTime;
        endTimerEvent.OnEventRaised += StopTimer;
    }

    public override void ResetObjective()
    {
        startTimerEvent.OnEventRaised -= StartTime;
        endTimerEvent.OnEventRaised -= StopTimer;
    }

    private void StartTime()
    {
        if(timerSequence.isAlive)
            timerSequence.Stop();

        if (Failed || Completed)
            return;

        currentTime = 0;

        timerSequence = Sequence.Create(-1).Group(Tween.Delay(1, TimerTick));
    }

    private void StopTimer()
    {
        DisableTimer();
        
        if(currentTime <= maxTime)
            CompleteObjective();
    }

    private void DisableTimer()
    {
        timerSequence.Stop();
    }

    private void TimerTick()
    {
        currentTime++;

        if (currentTime > maxTime)
            FailObjective();
        
        OnObjectiveUpdated?.Invoke();
        
    }
    
}
