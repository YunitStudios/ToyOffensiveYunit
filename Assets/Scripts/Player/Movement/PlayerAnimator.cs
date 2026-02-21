using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            Vector3 animDeltaPosition = animator.deltaPosition;
            cc.Move(animDeltaPosition);
        }
        
    }
}
