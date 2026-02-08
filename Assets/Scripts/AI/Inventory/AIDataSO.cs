using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIData", menuName = "ScriptableObjects/AIData")]
public class AIDataSO : ScriptableObject
{
    [Header("Weapons data")]
    [SerializeField] private List<WeaponDataSO> startingPrimaryWeapons;
    [SerializeField] private int maxNormalAmmo = 300;
    [SerializeField] private int maxSpecialAmmo = 100;
    
    [Header("Throwables data")]
    [SerializeField] private ThrowableDataSO startingThrowable;
    [SerializeField] private int maxThrowableAmount;

    public List<WeaponDataSO> StartingPrimaryWeapons => startingPrimaryWeapons;
    public int MaxNormalAmmo => maxNormalAmmo;
    public int MaxSpecialAmmo => maxSpecialAmmo;
    
    public ThrowableDataSO StartingThrowable => startingThrowable;
    public int MaxThrowableAmount => maxThrowableAmount;
}
