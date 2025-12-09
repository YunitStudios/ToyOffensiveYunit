using UnityEngine;

public class AIInventory : MonoBehaviour
{
    [Header("Config Data (will likely need different system for later)")]
    [SerializeField] private WeaponDataSO startingPrimaryWeapon;
    [SerializeField] private WeaponDataSO startingSecondaryWeapon;
    [SerializeField] private int maxNormalAmmo = 300;
    [SerializeField] private int maxSpecialAmmo = 100;
    
    [Header("Runtime Data")]
    private Weapon primaryWeapon;
    public Weapon GetPrimaryWeapon() => primaryWeapon;
    private Weapon secondaryWeapon;
    public Weapon GetSecondaryWeapon() => secondaryWeapon;
    private int normalAmmoCount;
    public int GetNormalAmmoCount() => normalAmmoCount;
    private int specialAmmoCount;
    public int GetSpecialAmmoCount() => specialAmmoCount;


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        primaryWeapon = new Weapon(startingPrimaryWeapon);
        secondaryWeapon = new Weapon(startingSecondaryWeapon);
        normalAmmoCount = maxNormalAmmo;
        specialAmmoCount = maxSpecialAmmo;
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
}
