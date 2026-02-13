using System.Collections.Generic;
using UnityEngine;

public class AIInventory : MonoBehaviour
{
    [SerializeField] private AIDataSO aiData;
    
    [Header("Runtime Data")]
    private Weapon primaryWeapon;
    public Weapon GetPrimaryWeapon() => primaryWeapon;
    
    private int normalAmmoCount;
    public int GetNormalAmmoCount() => normalAmmoCount;
    
    private int specialAmmoCount;
    public int GetSpecialAmmoCount() => specialAmmoCount;
    
    private ThrowableDataSO throwableData;
    public ThrowableDataSO GetThrowableData() => throwableData;
    
    private int throwableCount;
    public int GetThrowableCount() => throwableCount;


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        List<WeaponDataSO> weapons = aiData.StartingPrimaryWeapons;
        int index = Random.Range(0, weapons.Count);
        primaryWeapon = new Weapon(weapons[index], null);
        normalAmmoCount = aiData.MaxNormalAmmo;
        specialAmmoCount = aiData.MaxSpecialAmmo;
        throwableData = aiData.StartingThrowable;
        throwableCount = aiData.MaxThrowableAmount;
    }
    
    public void SetNormalAmmoCount(int count)
    {
        normalAmmoCount = Mathf.Clamp(count, 0, aiData.MaxNormalAmmo);
    }
    
    public void AdjustNormalAmmoCount(int delta)
    {
        SetNormalAmmoCount(normalAmmoCount + delta);
    }
    
    public void SetSpecialAmmoCount(int count)
    {
        specialAmmoCount = Mathf.Clamp(count, 0, aiData.MaxSpecialAmmo);
    }
    
    public void AdjustSpecialAmmoCount(int delta)
    {
        SetSpecialAmmoCount(specialAmmoCount + delta);
    }
    
    public void SetThrowableCount(int count)
    {
        throwableCount = Mathf.Clamp(count, 0, aiData.MaxThrowableAmount);
    }

    public void AdjustThrowableCount(int delta)
    {
        SetThrowableCount(throwableCount + delta);
    }

    public void SetAIData(AIDataSO newAIData)
    {
        aiData = newAIData;
        Init();
    }
}
