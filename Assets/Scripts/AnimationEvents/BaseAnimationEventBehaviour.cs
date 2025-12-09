using UnityEngine;

// credit to https://github.com/adammyhre/Improved-Unity-Animation-Events/tree/master for the original system

public abstract class BaseAnimationEventBehaviour : StateMachineBehaviour
{
    [Range(0f, 1f)]
    public float triggerTime;

    protected bool hasTriggered;
    protected AnimationEventReceiver receiver;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;
        receiver = animator.GetComponent<AnimationEventReceiver>();
        OnEnter(animator);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1f;
        if (!hasTriggered && currentTime >= triggerTime)
        {
            Trigger(animator);
            hasTriggered = true;
        }
    }

    protected abstract void OnEnter(Animator animator);
    protected abstract void Trigger(Animator animator);
}