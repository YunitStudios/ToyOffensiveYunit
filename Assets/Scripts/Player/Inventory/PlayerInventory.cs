using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapons data")]
    [SerializeField] private WeaponDataSO startingPrimaryWeapon;
    [SerializeField] private WeaponDataSO startingSecondaryWeapon;
    [SerializeField] private int maxNormalAmmo = 300;
    [SerializeField] private int maxSpecialAmmo = 100;
    
    [Header("Throwables data")]
    [SerializeField] private ThrowableDataSO startingThrowable;
    [SerializeField] private int maxThrowableAmount;
    
    public static PlayerInventory Instance { get; private set; }
    
    [Header("Runtime Data")]
    private Weapon primaryWeapon;
    public Weapon GetPrimaryWeapon() => primaryWeapon;
    private Weapon secondaryWeapon;
    public Weapon GetSecondaryWeapon() => secondaryWeapon;
    private int normalAmmoCount;
    public ThrowableDataSO GetStartingThrowable() => startingThrowable;
    public int GetNormalAmmoCount() => normalAmmoCount;
    private int specialAmmoCount;
    public int GetSpecialAmmoCount() => specialAmmoCount;
    private int throwableCount;
    public int GetThrowableCount() => throwableCount;
    public PlayerCamera.CameraType cameraType;


    private void Awake()
    {
        Instance = this;
        
        Init();
    }

    public void Init()
    {
        primaryWeapon = new Weapon(startingPrimaryWeapon);
        secondaryWeapon = new Weapon(startingSecondaryWeapon);
        normalAmmoCount = maxNormalAmmo;
        specialAmmoCount = maxSpecialAmmo;
        throwableCount = maxThrowableAmount;
    }
    
    public void SetNormalAmmoCount(int count)
    {
        normalAmmoCount = Mathf.Clamp(count, 0, maxNormalAmmo);
    }
    public void AdjustNormalAmmoCount(int delta)
    {
        SetNormalAmmoCount(normalAmmoCount + delta);
    }
    public void SetSpecialAmmoCount(int count)
    {
        specialAmmoCount = Mathf.Clamp(count, 0, maxSpecialAmmo);
    }
    public void AdjustSpecialAmmoCount(int delta)
    {
        SetSpecialAmmoCount(specialAmmoCount + delta);
    }
    public void SetThrowableCount(int count)
    {
        throwableCount = Mathf.Clamp(count, 0, maxThrowableAmount);
    }
    public void AdjustThrowableCount(int delta)
    {
        SetThrowableCount(throwableCount + delta);
    }
}
