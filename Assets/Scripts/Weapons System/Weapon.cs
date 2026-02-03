using UnityEngine;

public class Weapon
{
    public WeaponDataSO WeaponData;
    
    [Header("Runtime Values")]
    public int CurrentAmmoInMag;
    public WeaponSpread WeaponSpread;
    public WeaponDataSO.FireModes CurrentFireMode;
    public Transform FirePoint;
    
    // Constructor
    public Weapon(WeaponDataSO weaponData)
    {
        Initialize(weaponData);
    }
    
    public void Initialize(WeaponDataSO weaponData)
    {
        WeaponData = weaponData;
        CurrentAmmoInMag = weaponData.MagSize;
        CurrentFireMode = weaponData.SupportedFireModes[0];
        FirePoint = LocateFirePoint(weaponData.WeaponPrefab);
        
        WeaponSpread = new WeaponSpread(
            weaponData.BaseSpread,
            weaponData.HalfSpread,
            weaponData.MaxSpread,
            weaponData.MagSize,
            weaponData.FireRateRPM
        );
    }
    
    
    private Transform LocateFirePoint(GameObject weaponPrefab)
    {
        // TODO implement fire point locating logic
        return null;
    }
    
    public void Fire()
    {
        // update spread
        WeaponSpread.OnShotFired(WeaponData.MagSize);
        
        CurrentAmmoInMag--;
    }
    
    public void Reload(PlayerDataSO playerData)
    {
        // reset spread on reload
        WeaponSpread.ResetSpread();

        // determine which ammo to use
        int availableAmmo;

        if (WeaponData.SpecialAmmo)
        {
            availableAmmo = playerData.SpecialAmmoCount;
        }
        else
        {
            availableAmmo = playerData.NormalAmmoCount;
        }

        if (availableAmmo <= 0)
        {
            // no ammo left at all! play a sound or something
            Debug.Log("No ammo left to reload!");
            return;
        }

        // actually give/take the ammo
        if (availableAmmo >= WeaponData.MagSize)
        {
            CurrentAmmoInMag = WeaponData.MagSize;

            if (WeaponData.SpecialAmmo)
            {
                playerData.AdjustSpecialAmmoCount(-WeaponData.MagSize);;
            }
            else
            {
                playerData.AdjustNormalAmmoCount(-WeaponData.MagSize);
            }
            Debug.Log("Reloaded full mag");
        }
        else
        {
            CurrentAmmoInMag = availableAmmo;

            if (WeaponData.SpecialAmmo)
            {
                playerData.SetSpecialAmmoCount(0);
            }
            else
            {
                playerData.SetNormalAmmoCount(0);
            }

            Debug.Log("Reloaded partial mag");
        }
    }
    
    public void EnemyReload(AIInventory aiInventory)
    {
        // reset spread on reload
        WeaponSpread.ResetSpread();

        // determine which ammo to use
        int availableAmmo;

        if (WeaponData.SpecialAmmo)
        {
            availableAmmo = aiInventory.GetSpecialAmmoCount();
        }
        else
        {
            availableAmmo = aiInventory.GetNormalAmmoCount();
        }

        if (availableAmmo <= 0)
        {
            // no ammo left at all! play a sound or something
            return;
        }

        // actually give/take the ammo
        if (availableAmmo >= WeaponData.MagSize)
        {
            CurrentAmmoInMag = WeaponData.MagSize;
        }
        else
        {
            CurrentAmmoInMag = availableAmmo;
        }
    }
    

}
