using UnityEngine;


[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapon")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Name")]
    [Tooltip("How the weapon will be referred to in code")]
    public string ClassName;

    [Tooltip("How the weapon is shown in UI")]
    public string DisplayName;


    [Header("Core stats")]
    [Tooltip("The weapons maximum fire rate in rounds per minute. It also acts as a limit for semi auto firing speed")]
    public int FireRateRPM;     // rounds per minute (a delay between shots will come from this, it also acts as a limit for semi auto firing speed)
    public int Damage;

    [Header("Fire modes")]
    [Tooltip("The weapons fire modes in order of how they will be cycled through. The first one is the default mode")]
    public FireModes[] SupportedFireModes;
    public FireModes CurrentFireMode;

    [Header("Reloading")]
    public int MagSize;
    [Tooltip("True or false if it uses special ammo type")]
    public bool SpecialAmmo;
    [Tooltip("Time taken to reload the weapon in seconds, needs to be the same length as the animation")]
    public float ReloadTime;


    [Header("Spread")]
    [Tooltip("These are the start middle and end of a line that will be interpolated while continually firing to change the weapons spread")]
    public float BaseSpread;
    public float HalfSpread;
    public float MaxSpread;


    [Header("Misc projectile settings")]
    [Tooltip("Number of projectiles fired per shot. If its over 1 its a shotgun")]
    public int ShotQuantity;
    [Tooltip("spread per projectile, this is 0 unless its a shotgun then it might be higher as they dont use the regular spread like a full auto or semi auto weapon")]
    public float ShotSpread;
    [Tooltip("If true the projectile will use rigidbody like physics")]
    public bool IsPhysicsBased;
    [Tooltip("Initial velocity of physics projectiles in meters per second")]
    public float InitialVelocityMS;
    [Tooltip("Mass of physics projectiles in kilograms")]
    public float MassKG;
    
    [Header("Attachment settings")]
    [Tooltip("What attachments the gun can take")]
    public string[] Attachments;

    [Header("Weapon prefab")]
    [Tooltip("A reference to the weapon prefab with its fire point child")]
    public GameObject WeaponPrefab;
    
    [Header("Aim type")]
    [Tooltip("Type of aiming used (scope or aim or whatever)")]
    public PlayerCamera.CameraType AimCameraType;

    public enum FireModes
    {
        Full,
        Semi,
        Single
    }

    public void CopyFrom(WeaponDataSO other)
    {
        ClassName = other.ClassName;
        DisplayName = other.DisplayName;
        FireRateRPM = other.FireRateRPM;
        Damage = other.Damage;
        SupportedFireModes = other.SupportedFireModes;
        CurrentFireMode = other.CurrentFireMode;
        MagSize = other.MagSize;
        SpecialAmmo = other.SpecialAmmo;
        ReloadTime = other.ReloadTime;
        BaseSpread = other.BaseSpread;
        HalfSpread = other.HalfSpread;
        MaxSpread = other.MaxSpread;
        ShotQuantity = other.ShotQuantity;
        ShotSpread = other.ShotSpread;
        IsPhysicsBased = other.IsPhysicsBased;
        WeaponPrefab = other.WeaponPrefab;
        InitialVelocityMS = other.InitialVelocityMS;
        MassKG = other.MassKG;
        Attachments = other.Attachments;
        AimCameraType = other.AimCameraType;
    }
}
