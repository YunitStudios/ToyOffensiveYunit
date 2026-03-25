using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponSoundPack", menuName = "ScriptableObjects/WeaponSoundPack")]
public class WeaponSoundPackSO : ScriptableObject
{
    public SoundDataSO ADS;
    public SoundDataSO Equip;
    public SoundDataSO Gunshot;
    public SoundDataSO Reload_Empty;
    public SoundDataSO Reload_Half_Empty;
}
