using System.Collections;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State CurrentState => currentState;
    protected State currentState;
    
    protected virtual void Update()
    {
        currentState?.CheckTransitions(); 
        currentState?.Tick();
    }

    protected virtual void FixedUpdate()
    {
        currentState?.FixedTick();
    }
    
    protected virtual void LateUpdate()
    {
        currentState?.LateTick();
    }

    public bool SwitchState(State newState, State oldState)
    {
        // Debug.Log("Switch to " + newState.GetType().Name);
        
        if (!newState.CanEnter())
            return false;
        
        
        // Avoid switching states multiple times in one frame
        if (oldState != null)
        {
            
            if (oldState.HasChangedState)
                return false;
            oldState.HasChangedState = true;
            StartCoroutine(ResetChangedStateFlagCoroutine(oldState));
        }

        
        currentState?.OnExit();

        currentState = newState;
        currentState?.OnEnter();
        
        OnStateSwitched();
        
        return true;
    }

    protected virtual void OnStateSwitched()
    {
        
    }
    
    private IEnumerator ResetChangedStateFlagCoroutine(State oldState)
    {
        yield return new WaitForEndOfFrame();
        oldState.HasChangedState = false;
    }
}
