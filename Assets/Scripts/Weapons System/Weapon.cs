using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public WeaponDataSO WeaponData;
    
    [Header("Runtime Values")]
    public int CurrentAmmoInMag;
    public WeaponSpread WeaponSpread;
    public WeaponDataSO.FireModes CurrentFireMode;
    public Transform FirePoint;

    public List<AttachmentDataSO> AttachmentSOs;
    
    // Constructor
    public Weapon(WeaponDataSO weaponData, List<AttachmentDataSO> attachments)
    {
        Initialize(weaponData, attachments);
    }
    
    public void Initialize(WeaponDataSO weaponData, List<AttachmentDataSO> attachments)
    {
        WeaponDataSO weaponDataSoCopy = ScriptableObject.CreateInstance<WeaponDataSO>();
        weaponDataSoCopy.CopyFrom(weaponData);
        
        // load attachments
        LoadAttachments(weaponDataSoCopy, attachments);
        
        WeaponData = weaponDataSoCopy;
        CurrentAmmoInMag = weaponDataSoCopy.MagSize;
        CurrentFireMode = weaponDataSoCopy.SupportedFireModes[0];
        FirePoint = LocateFirePoint(weaponDataSoCopy.WeaponPrefab);
        
        WeaponSpread = new WeaponSpread(
            weaponDataSoCopy.BaseSpread,
            weaponDataSoCopy.HalfSpread,
            weaponDataSoCopy.MaxSpread,
            weaponDataSoCopy.MagSize,
            weaponDataSoCopy.FireRateRPM
        );
    }

    private void LoadAttachments(WeaponDataSO weaponData, List<AttachmentDataSO> attachments)
    {
        if (weaponData == null || attachments == null) return;

        foreach (AttachmentDataSO attachment in attachments)
        {
            if (attachment == null) continue;

            foreach (StatModifier mod in attachment.Modifiers)
            {
                // skip if modifier is for a different weapon
                if (!string.IsNullOrEmpty(mod.WeaponClassName) &&
                    mod.WeaponClassName != weaponData.ClassName)
                    continue;

                ApplyModifier(weaponData, mod);
            }

            if (!weaponData.AttachmentSOs.Contains(attachment))
                weaponData.AttachmentSOs.Add(attachment);
        }
        
        AttachmentSOs = new List<AttachmentDataSO>(attachments);
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
