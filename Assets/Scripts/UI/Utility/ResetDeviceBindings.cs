using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Sources:
// Voidloop
// https://www.youtube.com/watch?v=qXbjyzBlduY&t

public class ResetDeviceBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string targetControlScheme;

    private RebindActionUI[] rebindUIs;

    private void Start()
    {
        rebindUIs = transform.parent.GetComponentsInChildren<RebindActionUI>(); 
    }

    public void ResetControlSchemeBindings()
    {
        foreach (InputActionMap map in inputActions.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(targetControlScheme));
            }
        }
        foreach(var rebindUI in rebindUIs)
            rebindUI.UpdateBindingDisplay();
    }
}