using System;
using EditorAttributes;
using UnityEngine;
using TMPro;

public class WeaponDisplay : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private TMP_Text currentAmmoObj;
    [SerializeField] private TMP_Text maxAmmoObj;
    [SerializeField] private TMP_Text weaponName;

    //[Title("\n<b><color=#ffd180>Attributes", 15, 5, false)]


    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    [SerializeField] private VoidEventChannelSO onAmmoChanged;
    [SerializeField] private VoidEventChannelSO onCurrentWeaponChanged;

    private void OnEnable()
    {
        onAmmoChanged.OnEventRaised += UpdateAmmo;
        onCurrentWeaponChanged.OnEventRaised += UpdateWeapon;
        onCurrentWeaponChanged.OnEventRaised += UpdateAmmo;
    }

    private void OnDisable()
    {
        onAmmoChanged.OnEventRaised -= UpdateAmmo;
        onCurrentWeaponChanged.OnEventRaised -= UpdateWeapon;
        onCurrentWeaponChanged.OnEventRaised += UpdateAmmo;

    }

    private void Start()
    {
        UpdateAmmo();
        UpdateWeapon();
    }

    private void UpdateAmmo()
    {
        Weapon currentWeapon = GameManager.PlayerData.CurrentWeapon;
        currentAmmoObj.text = ""+currentWeapon.CurrentAmmoInMag;
        maxAmmoObj.text = "" + (currentWeapon.WeaponData.SecondaryAmmo ? GameManager.PlayerData.SecondaryAmmoCount : GameManager.PlayerData.NormalAmmoCount);
    }

    private void UpdateWeapon()
    {
        weaponName.text = GameManager.PlayerData.CurrentWeapon.WeaponData.DisplayName;
    }


}
