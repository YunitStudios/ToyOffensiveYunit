using System;
using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using Google.Apis.Services;
using PrimeTween;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private TMP_Text primaryAmmoObj;
    [SerializeField] private TMP_Text primaryCurrentAmmoObj;
    [SerializeField] private Image primaryImageObj;
    [SerializeField] private CanvasGroup primaryCanvas;
    [SerializeField] private TMP_Text secondaryAmmoObj;
    [SerializeField] private TMP_Text secondaryCurrentAmmoObj;
    [SerializeField] private Image secondaryImageObj;
    [SerializeField] private CanvasGroup secondaryCanvas;
    [SerializeField] private FlexibleGridLayoutGroup ammoCountLayout;
    [SerializeField] private GameObject ammoImagePrefab;
    [SerializeField] private TMP_Text throwableCountObj;
    [SerializeField] private Image throwableImageObj;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField, Range(0,1)] private float ammo0Alpha = 0.5f;
    [SerializeField] private float weaponSwapInTime = 0.25f;
    [SerializeField] private float weaponSwapOutTime = 0.25f;
    [SerializeField, Range(0,1)] private float weaponInactiveScale = 0.8f;
    [SerializeField, Range(0,1)] private float weaponInactiveAlpha = 0.4f;



    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    [SerializeField] private VoidEventChannelSO onAmmoChanged;
    [SerializeField] private VoidEventChannelSO onCurrentWeaponChanged;
    [SerializeField] private VoidEventChannelSO onThrowableUsed;

    private PlayerDataSO PlayerData => GameManager.PlayerData;

    private Tween swapAlphaTween;
    private Tween swapScaleTween;

    private List<GameObject> ammoObjects = new();

    private void OnEnable()
    {
        onAmmoChanged.OnEventRaised += UpdateAmmo;
        onCurrentWeaponChanged.OnEventRaised += UpdateWeapon;
        onCurrentWeaponChanged.OnEventRaised += UpdateAmmo;
        onThrowableUsed.OnEventRaised += UpdateThrowable;
    }

    private void OnDisable()
    {
        onAmmoChanged.OnEventRaised -= UpdateAmmo;
        onCurrentWeaponChanged.OnEventRaised -= UpdateWeapon;
        onCurrentWeaponChanged.OnEventRaised -= UpdateAmmo;
        onThrowableUsed.OnEventRaised -= UpdateThrowable;

    }

    private IEnumerator Start()
    {
        yield return null;
        Initialize();
        UpdateAmmo();
        UpdateWeapon();
        UpdateThrowable();
    }
    
    private void Initialize()
    {
        Weapon primaryWeapon = PlayerData.PrimaryWeapon;
        primaryImageObj.sprite = primaryWeapon.WeaponData.WeaponSpriteWhite;
        Weapon secondaryWeapon = PlayerData.SecondaryWeapon;
        secondaryImageObj.sprite = secondaryWeapon.WeaponData.WeaponSpriteWhite;
    }

    private void UpdateAmmo()
    {
        primaryAmmoObj.text = GetAmmoString(PlayerData.TotalNormalAmmoCount.ToString("D3"));
        secondaryAmmoObj.text = GetAmmoString(PlayerData.TotalSecondaryAmmoCount.ToString("D3"));

        primaryCurrentAmmoObj.text = GetAmmoString(PlayerData.PrimaryWeapon.CurrentAmmoInMag.ToString("D2"));
        secondaryCurrentAmmoObj.text = GetAmmoString(PlayerData.SecondaryWeapon.CurrentAmmoInMag.ToString("D2"));
        UpdateVisualAmmo();
    }

    private string GetAmmoString(string targetString)
    {
        // Format ammo to be 3 digits always.
        // If only 1 or 2 digits, format the to add 0 to the start
        // Any 0 digit before the first non-zero digit also needs a lower alpha
        string result = "";
        foreach(var character in targetString)
        {
            if (character == '0')
            {
                result += $"<alpha={ammo0Alpha}>{character}</color>";
            }
            else
            {
                result += character;
            }
        }
        return result;
    }

    private void UpdateWeapon()
    {
        var currentWeapon = PlayerData.CurrentWeapon;
        ToggleWeaponState(primaryCanvas, currentWeapon == PlayerData.PrimaryWeapon);
        ToggleWeaponState(secondaryCanvas, currentWeapon == PlayerData.SecondaryWeapon);
        InitVisualAmmo();
        UpdateVisualAmmo();
    }

    private void ToggleWeaponState(CanvasGroup canvasGroup, bool value)
    {
        float targetAlpha = value ? 1 : weaponInactiveAlpha;
        Vector3 targetScale = value ? Vector3.one : Vector3.one * weaponInactiveScale;
        float targetTime = value ? weaponSwapInTime : weaponSwapOutTime;

        Tween.CompleteAll(canvasGroup);

        Tween.Scale(canvasGroup.transform, targetScale, targetTime);
        Tween.Alpha(canvasGroup, targetAlpha, targetTime);
    }

    private void InitVisualAmmo()
    {
        // Clear ammo objects on root
        foreach (Transform child in ammoCountLayout.transform)
        {
            Destroy(child.gameObject);
        }
        ammoObjects.Clear();
        
        // Create new objects based on mag size
        int magSize = PlayerData.CurrentWeapon.WeaponData.MagSize;
        for (int i = 0; i < magSize; i++)
        {
            GameObject ammoObj = Instantiate(ammoImagePrefab, ammoCountLayout.transform);
            ammoObjects.Add(ammoObj);
        }

        ammoCountLayout.constraintCount = magSize;
    }

    private void UpdateVisualAmmo()
    {
        // Set ammo game object state based on max ammo and ammo in mag
        int ammoInMag = PlayerData.CurrentWeapon.CurrentAmmoInMag;
        for (int i = 0; i < ammoObjects.Count; i++)
        {
            ammoObjects[i].SetActive(i < ammoInMag);
        }
    }
    
    private void UpdateThrowable()
    {
        throwableCountObj.text = PlayerData.ThrowableCount.ToString();
        throwableImageObj.sprite = PlayerData.StartingThrowable.iconWhite;
    }


}
