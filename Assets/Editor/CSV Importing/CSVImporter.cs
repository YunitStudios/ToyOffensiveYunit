using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public enum CSVType
{
    Weapons,
    Throwables,
    Sounds
}

#if UNITY_EDITOR
public class CSVImporter : MonoBehaviour
{
    public static void ImportCSV(CSVType type)
    {
        // determine which ScriptableObject type to find
        string assetTypeName = "";
        switch (type)
        {
            case CSVType.Weapons:
                assetTypeName = "WeaponTypesSO";
                break;
            case CSVType.Throwables:
                assetTypeName = "ThrowableTypesSO";
                break;
            case CSVType.Sounds:
                assetTypeName = "SoundTypesSO";
                break;
            default:
                Debug.LogError("Unknown CSV type: " + type);
                return;
        }

        // find the asset in the project
        string[] guids = AssetDatabase.FindAssets($"t:{assetTypeName}");
        if (guids.Length == 0)
        {
            Debug.LogError($"{assetTypeName} asset not found! Please create one before importing.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        // load the ScriptableObject and update its data
        switch (type)
        {
            case CSVType.Weapons:
                WeaponTypesSO weaponSO = AssetDatabase.LoadAssetAtPath<WeaponTypesSO>(path);
                weaponSO.WeaponTypes = DataLoader.LoadWeaponsCSV().ToArray();
                EditorUtility.SetDirty(weaponSO);
                break;

            case CSVType.Throwables:
                ThrowableTypesSO throwableSO = AssetDatabase.LoadAssetAtPath<ThrowableTypesSO>(path);
                throwableSO.ThrowableTypes = DataLoader.LoadThrowablesCSV().ToArray();
                EditorUtility.SetDirty(throwableSO);
                break;

            case CSVType.Sounds:
                SoundTypesSO soundSO = AssetDatabase.LoadAssetAtPath<SoundTypesSO>(path);
                soundSO.SoundTypes = DataLoader.LoadSoundsCSV().ToArray();
                EditorUtility.SetDirty(soundSO);
                break;
        }

        // save changes and refresh the AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"{type} updated successfully.");
    }
}
#endif