using System.Collections.Generic;
using UnityEngine;

public enum WeaponStat
{
    FireRateRPM,
    Damage,
    MagSize,
    ReloadTime,
    BaseSpread,
    HalfSpread,
    MaxSpread,
    ShotQuantity,
    ShotSpread,
    InitialVelocityMS,
    MassKG,
    Noise
}

public enum StatOperation
{
    Add,
    Multiply
}

[System.Serializable]
public struct StatModifier
{
    [Tooltip("Weapon ClassName this applies to")]
    public string WeaponClassName;

    public int ResinCost;

    public WeaponStat Stat;
    public StatOperation Operation;
    public float Value;
}

[CreateAssetMenu(fileName = "NewAttachment", menuName = "ScriptableObjects/Attachment")]
public class AttachmentDataSO : ScriptableObject
{
    [Header("Name")]
    [Tooltip("How the attachment will be referred to in code")]
    public string ClassName;
    [Tooltip("How the attachment is shown in UI")]
    public string DisplayName;

    [Header("Stat modifiers")]
    public List<StatModifier> Modifiers;
    
    public void CopyFrom(AttachmentDataSO other)
    {
        ClassName = other.ClassName;
        DisplayName = other.DisplayName;
        
        Modifiers = other.Modifiers;
    }
}

// one of these attachment SO can have several modifiers attached. Each one of these is a reference to a weapon, the stat its modifying, how its modifying it, and the amnt it modifies it by
// each attachment has a prefab for the model etc added to it
// this gets instantiated at the relevant point on each weapon