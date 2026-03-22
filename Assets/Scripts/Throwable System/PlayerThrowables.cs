using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerThrowables : MonoBehaviour
{
    [SerializeField] private ThrowableSpawner spawner;
    private ThrowableDataSO currentThrowable;
    private InputManager playerInputManager => InputManager.Instance;
    private PlayerDataSO PlayerData => GameManager.PlayerData;
    [SerializeField] private VoidEventChannelSO onThrowableUsed;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float throwDelayLength = 0.5f;
    [SerializeField] private float throwLength = 1f;
    [SerializeField] private float throwAnimTransition = 0.2f;

    private Tween throwDelay;
    private Tween throwFull;

    void OnDisable()
    {
        playerInputManager.OnThrowAction -= StartThrow;
    }

    private void Start()
    {
        currentThrowable = PlayerData.StartingThrowable;
        playerInputManager.OnThrowAction += StartThrow;
    }

    private void StartThrow()
    {
        if (!playerMovement.CanShoot)
            return;

        if (throwDelay.isAlive)
            return;

        if (PlayerData.ThrowableCount > 0)
        {
            throwDelay = Tween.Delay(throwDelayLength, CompleteThrow);
            throwFull = Tween.Delay(throwLength, EndThrow);

            playerMovement.ShouldDisplayGun = false;

            // Anim
            playerMovement.PlayerAnimator.CrossFadeInFixedTime("Throw", throwAnimTransition);
        }
    }
    private void CompleteThrow()
    {
        spawner.ThrowObject(currentThrowable);
        PlayerData.AdjustThrowableCount(-1);
        onThrowableUsed?.Invoke();

    }

    private void EndThrow()
    {
        playerMovement.ShouldDisplayGun = true;
    }
}