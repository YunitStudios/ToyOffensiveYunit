using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerThrowables : MonoBehaviour
{
    [FormerlySerializedAs("playerInputController")] [SerializeField] private InputManager playerInputManager;
    [SerializeField] private ThrowableSpawner spawner;
    private ThrowableDataSO currentThrowable;
    
    private PlayerInventory playerInventory => PlayerInventory.Instance;


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
        currentThrowable = playerInventory.GetStartingThrowable();
    }

    void ThrowThing()
    {
        if (playerInventory.GetThrowableCount() > 0)
        {
            spawner.ThrowObject(currentThrowable);
            playerInventory.AdjustThrowableCount(-1);
        }
    }
}
