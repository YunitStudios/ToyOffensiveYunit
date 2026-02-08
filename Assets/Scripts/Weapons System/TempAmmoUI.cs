using System;
using UnityEngine;
using TMPro;

public class TempAmmoUI : MonoBehaviour
{
    public WeaponsSystem weaponsSystem;
    public TMP_Text ammoText;
    
    private void Update()
    {
        if (ammoText != null && weaponsSystem != null && weaponsSystem.currentWeapon != null && weaponsSystem.currentWeapon.WeaponData != null)
        {
            ammoText.text = $"{weaponsSystem.currentWeapon.CurrentAmmoInMag} / {weaponsSystem.currentWeapon.WeaponData.MagSize}";
        }
        else
        {
            ammoText.text = "-- / --"; // fallback if anything is null
        }
    }
}


