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
    public Weapon PrimaryWeapon { get; private set; }
    public Weapon SecondaryWeapon { get; private set; }
    public ThrowableDataSO StartingThrowable { get; private set; }
    public List<AttachmentDataSO> PrimaryAttachments { get; private set; }
    public List<AttachmentDataSO> SecondaryAttachments { get; private set; }
    public void SetPrimaryAttachments(List<AttachmentDataSO> value) => PrimaryAttachments = value;
    public void SetSecondaryAttachments(List<AttachmentDataSO> value) => SecondaryAttachments = value;
    public int NormalAmmoCount { get; private set; }
    public void SetNormalAmmoCount(int value) => NormalAmmoCount = Mathf.Clamp(value, 0, maxNormalAmmo);
    public void AdjustNormalAmmoCount(int delta) => SetNormalAmmoCount(NormalAmmoCount + delta);
    public int SpecialAmmoCount { get; private set; }
    public void SetSpecialAmmoCount(int value) => SpecialAmmoCount = Mathf.Clamp(value, 0, maxSpecialAmmo);
    public void AdjustSpecialAmmoCount(int delta) => SetSpecialAmmoCount(SpecialAmmoCount + delta);
    public int ThrowableCount { get; private set; }
    public void SetThrowableCount(int value) => ThrowableCount = Mathf.Clamp(value, 0, maxThrowableAmount);
    public void AdjustThrowableCount(int delta) => SetThrowableCount(ThrowableCount + delta);
    // Player Movement
    public PlayerCamera.CameraType CameraType { get; private set; }
    // Player General
    public float CurrentHealth { get; private set; }
    public void SetCurrentHealth(float value) => CurrentHealth = value;


    public void Init()
    {
        PrimaryWeapon = new Weapon(startingPrimaryWeapon, primaryAttachments);
        SecondaryWeapon = new Weapon(startingSecondaryWeapon, secondaryAttachments);
        StartingThrowable = startingThrowable;
        NormalAmmoCount = maxNormalAmmo;
        SpecialAmmoCount = maxSpecialAmmo;
        ThrowableCount = maxThrowableAmount;
        CameraType = PlayerCamera.CameraType.Main;
    }
}
