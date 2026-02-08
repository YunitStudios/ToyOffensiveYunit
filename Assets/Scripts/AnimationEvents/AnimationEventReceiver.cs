using UnityEngine;
using System.Collections.Generic;

// credit to https://github.com/adammyhre/Improved-Unity-Animation-Events/tree/master for the original system

public class AnimationEventReceiver : MonoBehaviour {
    [SerializeField] List<AnimationEvent> animationEvents = new();

    public void OnAnimationEventTriggered(string eventName) {
        AnimationEvent matchingEvent = animationEvents.Find(se => se.eventName == eventName);
        matchingEvent?.OnAnimationEvent?.Invoke();
    }
}