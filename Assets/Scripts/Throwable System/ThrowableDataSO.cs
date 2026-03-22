using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowable", menuName = "ScriptableObjects/Throwable")]
public class ThrowableDataSO : ScriptableObject
{
    [Header("Name")]
    [Tooltip("How the weapon will be referred to in code")]
    public string ClassName;
    [Tooltip("How the weapon is shown in UI")]
    public string DisplayName;

    [Header("Fuse")] 
    [Tooltip("How long the throwable takes to go off")]
    public float FuseTime;
    [Tooltip("Does the throwable go off on impact")]
    public bool IsImpactFuse;

    [Header("Stats")]
    [Tooltip("How much damage the throwable will do at the center of its radius when it goes off")]
    public float Damage;
    [Tooltip("The area of effect of the throwable")]
    public float Radius;

    [Header("Effects")]
    [Tooltip("Is it one of the generic effects or a custom one?")]
    public EffectTypes EffectType;
    
    [Header("Throwable prefab")]
    [Tooltip("A reference to the throwable prefab")]
    public GameObject ThrowablePrefab;
    
    public enum EffectTypes
    {
        Explosion,
        Flash,
        Custom
    }
    
    public void CopyFrom(ThrowableDataSO other)
    {
        ClassName = other.ClassName;
        DisplayName = other.DisplayName;
        
        FuseTime = other.FuseTime;
        IsImpactFuse = other.IsImpactFuse;
        
        Damage = other.Damage;
        Radius = other.Radius;
        
        ThrowablePrefab = other.ThrowablePrefab;
    }
}
