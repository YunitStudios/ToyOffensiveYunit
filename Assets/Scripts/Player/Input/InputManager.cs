using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputSettings settings;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private GameSettings playerSettings;




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
    
    private bool cursorDisabled;
    private bool cursorState = true;
    public void ForceDisableCursor(bool value)
    {
        cursorDisabled = value;
        if(value)
            ForceSetCursor(false);
        else
            SetCursor();
            
    }

    public void ToggleCursor(bool value)
    {
        cursorState = value;
        
        if(!cursorDisabled)
            SetCursor();
    }

    private void SetCursor()
    {
        Cursor.visible = cursorState;
        Cursor.lockState = cursorState ? CursorLockMode.None : CursorLockMode.Confined;
    }
    
    private void ForceSetCursor(bool value)
    {
        Cursor.visible = value;
        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Confined;
    }
    
    public void SetDeadzone(float playerSettingsDeadzone)
    {
        settings.defaultDeadzoneMin = playerSettingsDeadzone;
    }
    
    public void SetSensitivity(float playerSettingsSensitivity)
    {
        Vector2 sensRange = SettingsManager.Instance.SensitivityRange;
        float value = Mathf.Lerp(sensRange.x, sensRange.y, playerSettingsSensitivity/100);
        lookAction.action.ApplyParameterOverride((ScaleVector2Processor p) => p.x, value);
        lookAction.action.ApplyParameterOverride((ScaleVector2Processor p) => p.y, value);
    }

    public void ToggleInverted(bool playerSettingsInverseLook)
    {
        lookAction.action.ApplyParameterOverride((InvertVector2Processor p) => p.invertX, playerSettingsInverseLook);
        lookAction.action.ApplyParameterOverride((InvertVector2Processor p) => p.invertY, playerSettingsInverseLook);
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
    public bool AimHeld { get; private set; }
    private bool previousAimHeld;
    private bool currentAimValue;
    public bool IsShooting { get; private set; }
    public Action OnShootAction;
    public bool IsReloading { get; private set; }
    public Action OnReloadAction;
    
    public bool IsDebug { get; private set; }
    public Action OnDebug;
    
    public bool IsThrowing { get; private set; }
    public Action OnThrowAction;
    public float FrameScroll { get; private set; }
    public bool PrimaryWeapon { get; private set; }
    public Action OnPrimaryWeaponAction;
    public bool SecondaryWeapon { get; private set; }
    public Action OnSecondaryWeaponAction;

    public Action OnGloryKillAction;

    private void PlayerInputsEndFrame()
    {
        previousJumpHeld = JumpHeld;
        previousCrouchHeld = CrouchHeld;
        previousAimHeld = currentAimValue;

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
        currentAimValue = inputValue.isPressed;
        
        if(!playerSettings.toggleADS)
            AimHeld = currentAimValue;
        else
        {
            if(currentAimValue && !previousAimHeld)
                AimHeld = !AimHeld;
        }
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
    
    private void OnMouseScrollY(InputValue inputValue)
    {
        FrameScroll = inputValue.Get<float>();
    }
    private void OnPrimaryWeapon(InputValue inputValue)
    {
        PrimaryWeapon = inputValue.isPressed;
        OnPrimaryWeaponAction?.Invoke();
    }
    private void OnSecondaryWeapon(InputValue inputValue)
    {
        SecondaryWeapon = inputValue.isPressed;
        OnSecondaryWeaponAction?.Invoke();
    }
    private void OnToggleDebug(InputValue inputValue)
    {
        IsDebug = inputValue.isPressed;
        
        if(IsDebug && OnDebug != null)
            OnDebug.Invoke();
    }

    private void OnGloryKill(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            OnGloryKillAction?.Invoke();
        }
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
