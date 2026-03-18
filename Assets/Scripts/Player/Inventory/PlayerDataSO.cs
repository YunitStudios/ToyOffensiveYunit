using EditorAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Player Stats")] 
    [field: SerializeField] public string PlayerName;
    [SerializeField] private float maxHealth;
    public float MaxHealth => maxHealth;

    [Header("Weapons data")]
    [field: SerializeField] public WeaponDataSO StartingPrimaryWeapon { get; set; }
    [field: SerializeField] public WeaponDataSO StartingSecondaryWeapon { get; set; }
    [field: SerializeField] public AttachmentDataSO StartingPrimaryAttachment;
    [field: SerializeField] public AttachmentDataSO StartingSecondaryAttachment;
    [SerializeField] private int maxNormalAmmo = 300;
    [SerializeField] private int maxSecondaryAmmo = 100;
    [SerializeField] private float weaponSwapTime = 0.5f;

    [Header("Throwables data")]
    [SerializeField] private ThrowableDataSO startingThrowable;
    [SerializeField] private int maxThrowableAmount;

    [Header("Runtime Data")]
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public WeaponSlot CurrentWeaponSlot { get; private set; }
    // Weapons
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public Weapon PrimaryWeapon { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public Weapon SecondaryWeapon { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public ThrowableDataSO StartingThrowable { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public AttachmentDataSO PrimaryAttachment { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public AttachmentDataSO SecondaryAttachment { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public int NormalAmmoCount { get; private set; }
    public int TotalNormalAmmoCount => NormalAmmoCount + PrimaryWeapon.CurrentAmmoInMag;
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public int SecondaryAmmoCount { get; private set; }
    public int TotalSecondaryAmmoCount => SecondaryAmmoCount + SecondaryWeapon.CurrentAmmoInMag;
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public int ThrowableCount { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public float CurrentHealth { get; private set; }
    
    public Vector3 PlayerPosition { get; private set;}
    public Transform RotationRootTransform { get; private set;}
    public bool IsAiming { get; private set; }
    public float HealthPercentage => CurrentHealth / MaxHealth;
    
    public void SetStartingPrimaryWeapon(WeaponDataSO weaponData)
    {
        StartingPrimaryWeapon = weaponData;
    }
    public void SetStartingPrimaryAttachment(AttachmentDataSO attachmentData)
    {
        StartingPrimaryAttachment = attachmentData;
    }
    public void SetStartingSecondaryWeapon(WeaponDataSO weaponData)
    {
        StartingSecondaryWeapon = weaponData;
    }
    public void SetStartingSecondaryAttachment(AttachmentDataSO attachmentData)
    {
        StartingSecondaryAttachment = attachmentData;
    }
    
    public void SetWeaponSlot(WeaponSlot newSlot)
    {
        if (CurrentWeaponSlot == newSlot) return;
        
        CurrentWeaponSlot = newSlot;
        OnCurrentWeaponChanged?.Invoke();
    }
    public void SetPrimaryWeapon(Weapon weapon)
    {
        PrimaryWeapon = weapon;
        OnWeaponChanged?.Invoke();
        weapon.OnAmmoChanged += OnAmmoCountChanged.Invoke;
    }
    public void SetSecondaryWeapon(Weapon weapon)
    {
        SecondaryWeapon = weapon;
        OnWeaponChanged?.Invoke();
        weapon.OnAmmoChanged += OnAmmoCountChanged.Invoke;
    }
    public void SetPrimaryAttachment(Weapon weapon, AttachmentDataSO value)
    {
        PrimaryAttachment = value;
        weapon.LoadAttachments(value);
        OnAttachmentsChanged?.Invoke();
    }
    public void SetSecondaryAttachment(Weapon weapon, AttachmentDataSO value)
    {
        SecondaryAttachment = value;
        weapon.LoadAttachments(value);
        OnAttachmentsChanged?.Invoke();
    }
    public void SetThrowables(ThrowableDataSO value)
    {
        StartingThrowable = value;
        OnThrowableChanged?.Invoke();
    }
    public void SetNormalAmmoCount(int value)
    {
        NormalAmmoCount = Mathf.Clamp(value, 0, maxNormalAmmo);
        OnAmmoCountChanged?.Invoke();
    }
    public void AdjustNormalAmmoCount(int delta) => SetNormalAmmoCount(NormalAmmoCount + delta);
    public void SetSecondaryAmmoCount(int value)
    {
        SecondaryAmmoCount = Mathf.Clamp(value, 0, maxSecondaryAmmo);
        OnAmmoCountChanged?.Invoke();
    }
    public void AdjustSecondaryAmmoCount(int delta) => SetSecondaryAmmoCount(SecondaryAmmoCount + delta);
    public void SetThrowableCount(int value) 
    {
        ThrowableCount = Mathf.Clamp(value, 0, maxThrowableAmount);
        OnAmmoCountChanged?.Invoke();
    }
    public void AdjustThrowableCount(int delta) => SetThrowableCount(ThrowableCount + delta);
    // Player Movement
    public PlayerCamera.CameraType CameraType { get; private set; }
    // Player General
    public void SetCurrentHealth(float value) => CurrentHealth = value;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    public VoidEventChannelSO OnCurrentWeaponChanged;
    public VoidEventChannelSO OnWeaponChanged;
    public VoidEventChannelSO OnAttachmentsChanged;
    public VoidEventChannelSO OnThrowableChanged;
    public VoidEventChannelSO OnAmmoCountChanged;


    public bool IsAlive => CurrentHealth > 0;
    public Weapon CurrentWeapon => CurrentWeaponSlot == WeaponSlot.Primary ? PrimaryWeapon : SecondaryWeapon;
    public float WeaponSwapTime => weaponSwapTime;

    public void Init()
    {
    }

    public void Start()
    {
        SetWeaponSlot(WeaponSlot.Primary);
        SetPrimaryWeapon(new Weapon(StartingPrimaryWeapon));
        SetSecondaryWeapon(new Weapon(StartingSecondaryWeapon));
        SetThrowables(startingThrowable);
        SetNormalAmmoCount(maxNormalAmmo);
        SetSecondaryAmmoCount(maxSecondaryAmmo);
        SetThrowableCount(maxThrowableAmount);
        SetPrimaryAttachment(PrimaryWeapon, StartingPrimaryAttachment);
        SetSecondaryAttachment(SecondaryWeapon, StartingSecondaryAttachment);
        
        CameraType = PlayerCamera.CameraType.Main;
        CurrentHealth = MaxHealth;

        IsAiming = false; }

    public void Reset()
    {
        
    }
    
    // Runtime value setting
    public void StorePosition(Vector3 newPosition)
    {
        PlayerPosition = newPosition;
    }
    
    public void StoreRotationRootTransform(Transform newRotation)
    {
        RotationRootTransform = newRotation;
    }
    public void ToggleAiming(bool value)
    {
        IsAiming = value;
    }

    public enum WeaponSlot
    {
        Primary,
        Secondary
    }
}
