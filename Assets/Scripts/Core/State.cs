using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public abstract class State
{
    [HideInInspector] protected StateMachine stateMachine;
    
    private List<Func<bool>> extraEnterConditions = new();

    protected State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Initialize()
    {
        SetEnterConditions();
    }
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Tick();
    public abstract void FixedTick();
    public abstract void CheckTransitions();
    
    
    // Check all enter conditions are true
    public bool CanEnter()
    {
        foreach (var c in extraEnterConditions)
        {
            if (!c())
                return false;
        }

        return true;
    }
    
    protected virtual void SetEnterConditions()
    {
        extraEnterConditions.Add(() => !delayTween.isAlive);
    }
    
    protected void AddCanEnterCondition(Func<bool> condition)
    {
        extraEnterConditions.Add(condition);
    }
    
    public bool HasChangedState = false;
    protected bool SwitchState(State newState)
    {
        //Debug.Log($"Switching to state: {newState.GetType().Name}");
        return stateMachine.SwitchState(newState, this);
    }
    
    private Tween delayTween;
    protected void SetDelay(float delay)
    {
        // Set delay to current or new delay, whichever is longer
        if (delayTween.isAlive)
        {
            float remaining = delayTween.duration - delayTween.elapsedTime;
            float newDelay = remaining > delay ? remaining : delay;
            Tween.Delay(newDelay);
        }
        else
        {
            delayTween = Tween.Delay(delay);
        }
    }

}

public abstract class StateSettings { }