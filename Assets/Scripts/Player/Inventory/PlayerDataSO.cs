using EditorAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Player Stats")]
    [SerializeField] private float maxHealth;
    public float MaxHealth => maxHealth;

    [Header("Weapons data")]
    [SerializeField] private WeaponDataSO startingPrimaryWeapon;
    [SerializeField] private WeaponDataSO startingSecondaryWeapon;
    [SerializeField] private int maxNormalAmmo = 300;
    [SerializeField] private int maxSpecialAmmo = 100;
    [SerializeField] private List<AttachmentDataSO> primaryAttachments;
    [SerializeField] private List<AttachmentDataSO> secondaryAttachments;

    [Header("Throwables data")]
    [SerializeField] private ThrowableDataSO startingThrowable;
    [SerializeField] private int maxThrowableAmount;

    [Header("Runtime Data")]
    // Weapons
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public Weapon PrimaryWeapon { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public Weapon SecondaryWeapon { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public ThrowableDataSO StartingThrowable { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public List<AttachmentDataSO> PrimaryAttachments { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public List<AttachmentDataSO> SecondaryAttachments { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public int NormalAmmoCount { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public int SpecialAmmoCount { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public int ThrowableCount { get; private set; }
    [field: SerializeField, HideInEditMode, DisableInPlayMode]
    public float CurrentHealth { get; private set; }
    public void SetPrimaryWeapon(Weapon weapon)
    {
        PrimaryWeapon = weapon;
        OnWeaponChanged?.Invoke();
    }
    public void SetSecondaryWeapon(Weapon weapon)
    {
        SecondaryWeapon = weapon;
        OnWeaponChanged?.Invoke();
    }
    public void SetPrimaryAttachments(List<AttachmentDataSO> value)
    {
        PrimaryAttachments = value;
        OnAttachmentsChanged?.Invoke();
    }
    public void SetSecondaryAttachments(List<AttachmentDataSO> value)
    {
        SecondaryAttachments = value;
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
    public void SetSpecialAmmoCount(int value)
    {
        SpecialAmmoCount = Mathf.Clamp(value, 0, maxSpecialAmmo);
        OnAmmoCountChanged?.Invoke();
    }
    public void AdjustSpecialAmmoCount(int delta) => SetSpecialAmmoCount(SpecialAmmoCount + delta);
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
    public VoidEventChannelSO OnWeaponChanged;
    public VoidEventChannelSO OnAttachmentsChanged;
    public VoidEventChannelSO OnThrowableChanged;
    public VoidEventChannelSO OnAmmoCountChanged;


    public void Init()
    {
        SetPrimaryWeapon(new Weapon(startingPrimaryWeapon, primaryAttachments));
        SetSecondaryWeapon(new Weapon(startingSecondaryWeapon, secondaryAttachments));
        SetPrimaryAttachments(primaryAttachments);
        SetSecondaryAttachments(secondaryAttachments);
        SetThrowables(startingThrowable);
        SetNormalAmmoCount(maxNormalAmmo);
        SetSpecialAmmoCount(maxSpecialAmmo);
        SetThrowableCount(maxThrowableAmount);
        CameraType = PlayerCamera.CameraType.Main;
        CurrentHealth = MaxHealth;
    }

    public void Reset()
    {
        
    }
}
