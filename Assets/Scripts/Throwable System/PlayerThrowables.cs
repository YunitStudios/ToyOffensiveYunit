using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerThrowables : MonoBehaviour
{
    [SerializeField] private ThrowableSpawner spawner;
    private ThrowableDataSO currentThrowable;
    private InputManager playerInputManager => InputManager.Instance;
    private PlayerDataSO PlayerData => GameManager.PlayerData;


    void OnEnable()
    {
        playerInputManager.OnThrowAction += ThrowThing;
    }
    
    void OnDisable()
    {
        playerInputManager.OnThrowAction -= ThrowThing;
    }

    private void Start()
    {
        currentThrowable = PlayerData.StartingThrowable;
    }

    void ThrowThing()
    {
        if (PlayerData.ThrowableCount > 0)
        {
            spawner.ThrowObject(currentThrowable);
            PlayerData.AdjustThrowableCount(-1);
        }
    }
}
