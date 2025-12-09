using System;
using UnityEngine.Events;

// credit to https://github.com/adammyhre/Improved-Unity-Animation-Events/tree/master for the original system

[Serializable]
public class AnimationEvent {
    public string eventName;
    public UnityEvent OnAnimationEvent;
}