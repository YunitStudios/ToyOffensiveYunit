using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

        LoadInputActions();
    }
    
    private void LoadInputActions()
    {
        // Enable all action maps
        foreach (var map in inputActions.actionMaps)
        {
            map.Enable();
        }
    }

    private void LateUpdate()
    {
        PlayerInputsEndFrame();
    }

    #region PlayerInput
    public Vector2 FrameMove { get; private set; }
    public Vector2 FrameLook { get; private set; }
    public bool JumpDown { get; private set; }
    public bool JumpHeld { get; private set; }
    private bool previousJumpHeld;

    public bool IsSprinting { get; private set; }
    public bool CrouchDown { get; private set; }
    public bool CrouchHeld { get; private set; }
    private bool previousCrouchHeld;
    public bool IsAiming { get; private set; }
    public bool IsShooting { get; private set; }
    public Action OnShootAction;
    public bool IsReloading { get; private set; }
    public Action OnReloadAction;
    
    public bool IsThrowing { get; private set; }
    public Action OnThrowAction;

    private void PlayerInputsEndFrame()
    {
        previousJumpHeld = JumpHeld;
        previousCrouchHeld = CrouchHeld;

        JumpDown = false;
        CrouchDown = false;
    }
    private void OnMove(InputValue inputValue)
    {
        FrameMove = inputValue.Get<Vector2>();
    }
    public bool IsMovingUp => FrameMove.y > 0;
    public bool IsMovingDown => FrameMove.y < 0;
    public bool IsMovingLeft => FrameMove.x < 0;
    public bool IsMovingRight => FrameMove.x > 0;
    
    
    private void OnJump(InputValue inputValue)
    {
        bool isPressed = inputValue.isPressed;

        JumpDown = isPressed && !previousJumpHeld;
        JumpHeld = isPressed;
    }
    
    private void OnSprint(InputValue inputValue)
    {
        IsSprinting = inputValue.isPressed;
    }
    
    private void OnCrouch(InputValue inputValue)
    {
        bool isPressed = inputValue.isPressed;

        CrouchDown = isPressed && !previousCrouchHeld;
        CrouchHeld = isPressed;
    }
    
    private void OnLook(InputValue inputValue)
    {
        FrameLook = inputValue.Get<Vector2>();
    }
    
    private void OnAim(InputValue inputValue)
    {
        IsAiming = inputValue.isPressed;
    }
    
    private void OnShoot(InputValue inputValue)
    {
        IsShooting = inputValue.isPressed;
        
        if(IsShooting && OnShootAction != null)
            OnShootAction.Invoke();
    }
    
    private void OnReload(InputValue inputValue)
    {
        IsReloading = inputValue.isPressed;
        
        if(IsReloading && OnReloadAction != null)
            OnReloadAction.Invoke();
    }
    
    private void OnThrow(InputValue inputValue)
    {
        IsThrowing = inputValue.isPressed;
        
        if(IsThrowing && OnThrowAction != null)
            OnThrowAction.Invoke();
    }
    #endregion

    #region UIInput
    public bool PauseDown { get; private set; }

    private void OnPause(InputValue inputValue)
    {
        PauseDown = inputValue.isPressed;
    }
    #endregion
}
