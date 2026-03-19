using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class Weapon
{
    public WeaponDataSO WeaponData;

    [Header("Runtime Values")] 
    [SerializeField] private int currentAmmoInMag; 
    public int CurrentAmmoInMag
    {
        get => currentAmmoInMag;
        set
        {
            currentAmmoInMag = value;
            OnAmmoChanged?.Invoke();
        }
    }
    public WeaponSpread WeaponSpread;
    public Transform FirePoint;

    public AttachmentDataSO AttachmentSO;


    public Action OnAmmoChanged;
    
    // Constructor
    public Weapon(WeaponDataSO weaponData)
    {
        Initialize(weaponData);
    }
    
    public void Initialize(WeaponDataSO weaponData)
    {
        WeaponData = weaponData;
        FirePoint = LocateFirePoint(weaponData.WeaponPrefab);
        CurrentAmmoInMag = weaponData.MagSize;
        
        WeaponSpread = new WeaponSpread(
            weaponData.BaseSpread,
            weaponData.HalfSpread,
            weaponData.MaxSpread,
            weaponData.MagSize,
            weaponData.FireRateRPM
        );
    }

    public void LoadAttachments(AttachmentDataSO attachment)
    {
        if (WeaponData == null || attachment == null) return;
        
        Debug.Log("Loading Attachments");

        foreach (StatModifier mod in attachment.Modifiers)
        {
            // skip if modifier is for a different weapon
            if (!string.IsNullOrEmpty(mod.WeaponClassName) &&
                mod.WeaponClassName != WeaponData.ClassName)
                continue;

            ApplyModifier(WeaponData, mod);
        }

        if (!WeaponData.AttachmentSOs.Contains(attachment))
            WeaponData.AttachmentSOs.Add(attachment);
        
        AttachmentSO = attachment;
        
        CurrentAmmoInMag = WeaponData.MagSize;
        WeaponSpread = new WeaponSpread(
            WeaponData.BaseSpread,
            WeaponData.HalfSpread,
            WeaponData.MaxSpread,
            WeaponData.MagSize,
            WeaponData.FireRateRPM
        );
    }

    private void ApplyModifier(WeaponDataSO weaponData, StatModifier mod)
    {
        switch (mod.Stat)
        {
            case WeaponStat.FireRateRPM:
                weaponData.FireRateRPM = ApplyInt(weaponData.FireRateRPM, mod);
                break;
            case WeaponStat.Damage:
                weaponData.Damage = ApplyInt(weaponData.Damage, mod);
                break;
            case WeaponStat.MagSize:
                weaponData.MagSize = ApplyInt(weaponData.MagSize, mod);
                break;
            case WeaponStat.ReloadTime:
                weaponData.ReloadTime = ApplyFloat(weaponData.ReloadTime, mod);
                break;
            case WeaponStat.BaseSpread:
                weaponData.BaseSpread = ApplyFloat(weaponData.BaseSpread, mod);
                break;
            case WeaponStat.HalfSpread:
                weaponData.HalfSpread = ApplyFloat(weaponData.HalfSpread, mod);
                break;
            case WeaponStat.MaxSpread:
                weaponData.MaxSpread = ApplyFloat(weaponData.MaxSpread, mod);
                break;
            case WeaponStat.ShotQuantity:
                weaponData.ShotQuantity = ApplyInt(weaponData.ShotQuantity, mod);
                break;
            case WeaponStat.ShotSpread:
                weaponData.ShotSpread = ApplyFloat(weaponData.ShotSpread, mod);
                break;
            case WeaponStat.InitialVelocityMS:
                weaponData.InitialVelocityMS = ApplyFloat(weaponData.InitialVelocityMS, mod);
                break;
            case WeaponStat.MassKG:
                weaponData.MassKG = ApplyFloat(weaponData.MassKG, mod);
                break;
        }
    }

    private int ApplyInt(int baseValue, StatModifier mod)
    {
        return mod.Operation switch
        {
            StatOperation.Add => baseValue + Mathf.RoundToInt(mod.Value),
            StatOperation.Multiply => Mathf.RoundToInt(baseValue * mod.Value),
            _ => baseValue
        };
    }

    private float ApplyFloat(float baseValue, StatModifier mod)
    {
        return mod.Operation switch
        {
            StatOperation.Add => baseValue + mod.Value,
            StatOperation.Multiply => baseValue * mod.Value,
            _ => baseValue
        };
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
        int ammoNeeded = WeaponData.MagSize - CurrentAmmoInMag;

        if (ammoNeeded == 0)
        {
            return;
        }
        
        // reset spread on reload
        WeaponSpread.ResetSpread();

        // determine which ammo to use
        int availableAmmo;

        if (WeaponData.SecondaryAmmo)
        {
            availableAmmo = playerData.SecondaryAmmoCount;
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
            
            if (WeaponData.SecondaryAmmo)
            {
                playerData.AdjustSecondaryAmmoCount(-ammoNeeded);;
            }
            else
            {
                playerData.AdjustNormalAmmoCount(-ammoNeeded);
            }
            Debug.Log("Reloaded full mag");
        }
        else
        {
            CurrentAmmoInMag = availableAmmo;

            if (WeaponData.SecondaryAmmo)
            {
                playerData.SetSecondaryAmmoCount(0);
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

        if (WeaponData.SecondaryAmmo)
        {
            availableAmmo = aiInventory.GetSecondaryAmmoCount();
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
