using UnityEngine;
public class AnimationEventStateBehaviour : BaseAnimationEventBehaviour
{
    public string eventName;

    protected override void OnEnter(Animator animator) { }

    protected override void Trigger(Animator animator)
    {
        if (receiver != null)
        {
            receiver.OnAnimationEventTriggered(eventName);
        }
    }
}
