using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System;
using Throwable_System.ThrowableTypes;

public class DataLoader
{
    public static List<WeaponDataSO> LoadWeaponsCSV()
    {
        List<WeaponDataSO> weapons = new List<WeaponDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("GunsCSV");

        if(rows != null && rows.Count <= 1)
        {
            Debug.LogError("No weapon data found in CSV");
            return weapons;
        }

        for (int i = 1; i < rows.Count; i++)
        {
            var columns = rows[i];
            string className = columns[0];
            string assetPath = $"Assets/ScriptableObjects/Weapons/{className}.asset";

            // try to load existing asset first to prevent unassigning in editor
            WeaponDataSO weaponData = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(assetPath);
            bool isNew = false;

            if (weaponData == null)
            {
                weaponData = ScriptableObject.CreateInstance<WeaponDataSO>();
                isNew = true;
            }

            // assign values from CSV
            weaponData.ClassName = className;
            weaponData.DisplayName = columns[1];
            weaponData.FireRateRPM = int.Parse(columns[2]);
            weaponData.Damage = int.Parse(columns[3]);
            
            weaponData.SupportedFireModes = columns[4]
                .Split(' ')
                .Where(x => Enum.TryParse<WeaponDataSO.FireModes>(x, true, out _))
                .Select(x => Enum.Parse<WeaponDataSO.FireModes>(x, true))
                .ToArray();
            
            weaponData.MagSize = int.Parse(columns[5]);
            weaponData.SpecialAmmo = bool.Parse(columns[6]);
            weaponData.ReloadTime = float.Parse(columns[7]);
            weaponData.BaseSpread = float.Parse(columns[8]);
            weaponData.HalfSpread = float.Parse(columns[9]);
            weaponData.MaxSpread = float.Parse(columns[10]);
            weaponData.ShotQuantity = int.Parse(columns[11]);
            weaponData.ShotSpread = float.Parse(columns[12]);
            weaponData.IsPhysicsBased = bool.Parse(columns[13]);
            weaponData.InitialVelocityMS = float.Parse(columns[14]);
            weaponData.MassKG = float.Parse(columns[15]);
            weaponData.Attachments = columns[16].Split(',');
            
            weaponData.AimCameraType = GetCameraType(columns[17]);

            // only create new asset if it didn't exist, otherwise just mark dirty
            if (isNew)
            {
                AssetDatabase.CreateAsset(weaponData, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(weaponData);
            }

            weapons.Add(weaponData);
        }

        AssetDatabase.SaveAssets();
        return weapons;
    }
    
    public static List<ThrowableDataSO> LoadThrowablesCSV()
    {
        List<ThrowableDataSO> throwables = new List<ThrowableDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("ThrowablesCSV");

        if(rows != null && rows.Count <= 1)
        {
            Debug.LogError("No weapon data found in CSV");
            return throwables;
        }

        for (int i = 1; i < rows.Count; i++)
        {
            var columns = rows[i];
            string className = columns[0];
            string assetPath = $"Assets/ScriptableObjects/Throwables/{className}.asset";

            // check if we already have this asset
            ThrowableDataSO throwableData = AssetDatabase.LoadAssetAtPath<ThrowableDataSO>(assetPath);
            bool isNew = false;

            if (throwableData == null)
            {
                throwableData = ScriptableObject.CreateInstance<ThrowableDataSO>();
                isNew = true;
            }

            // assign values from CSV
            throwableData.ClassName = className;
            throwableData.DisplayName = columns[1];
            throwableData.FuseTime = float.Parse(columns[2]);
            throwableData.IsImpactFuse = bool.Parse(columns[3]);
            throwableData.Damage = float.Parse(columns[4]);
            throwableData.Radius = float.Parse(columns[5]);
            throwableData.EffectType = Enum.Parse<ThrowableDataSO.EffectTypes>(columns[6], true);

            SetupPrefab(throwableData);

            if (isNew)
            {
                AssetDatabase.CreateAsset(throwableData, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(throwableData);
            }

            throwables.Add(throwableData);
        }

        AssetDatabase.SaveAssets();
        return throwables;
    }

    // SetupPrefab remains mostly the same as it handles prefab instances correctly
    private static void SetupPrefab(ThrowableDataSO throwableData)
    {
        string prefabPath = $"Assets/Prefabs/Throwables/{throwableData.ClassName}/{throwableData.ClassName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.Log("No prefab found, making a new one as a clone of generic");
            string genericPath = "Assets/Prefabs/Throwables/generic/generic.prefab";
            GameObject genericPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(genericPath);

            if (genericPrefab != null)
            {
                string folderPath = $"Assets/Prefabs/Throwables/{throwableData.ClassName}";
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets/Prefabs/Throwables", throwableData.ClassName);
                }

                prefab = PrefabUtility.SaveAsPrefabAsset(genericPrefab, prefabPath);
            }
            else
            {
                Debug.LogError("No generic prefab found! Make sure you have one before importing");
            }
        }

        throwableData.ThrowablePrefab = prefab;

        if (prefab != null)
        {
            GameObject tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            ThrowableTemplate existingTemplate = tempInstance.GetComponent<ThrowableTemplate>();

            if (existingTemplate != null)
            {
                if (throwableData.EffectType != ThrowableDataSO.EffectTypes.Custom)
                {
                    GameObject.DestroyImmediate(existingTemplate);
                }
            }
            
            switch (throwableData.EffectType)
            {
                case ThrowableDataSO.EffectTypes.Explosion:
                    if (tempInstance.GetComponent<ExplosiveGrenade>() == null)
                        tempInstance.AddComponent<ExplosiveGrenade>();
                    break;

                case ThrowableDataSO.EffectTypes.Flash:
                    if (tempInstance.GetComponent<FlashGrenade>() == null)
                        tempInstance.AddComponent<FlashGrenade>();
                    break;
            }
            
            ThrowableTemplate throwableClass = tempInstance.GetComponent<ThrowableTemplate>();
            if (throwableClass != null)
            {
                throwableClass.FuseTime = throwableData.FuseTime;
                throwableClass.IsImpactFuse = throwableData.IsImpactFuse;
                throwableClass.Damage = throwableData.Damage;
                throwableClass.Radius = throwableData.Radius;
            }

            PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
            GameObject.DestroyImmediate(tempInstance);
        }
    }
    
    private static PlayerCamera.CameraType GetCameraType(string cameraType)
    {
        if (Enum.TryParse<PlayerCamera.CameraType>(cameraType, out var result))
            return result;

        return PlayerCamera.CameraType.Aim;
    }

    public static List<SoundDataSO> LoadSoundsCSV()
    {
        List<SoundDataSO> sounds = new List<SoundDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("SoundsCSV");

        if (rows == null || rows.Count <= 1)
        {
            Debug.LogError("No sound data found in CSV");
            return sounds;
        }

        for (int i = 1; i < rows.Count; i++)
        {
            string[] columns = rows[i];
            string wwiseName = columns[0];
            string assetPath = $"Assets/ScriptableObjects/Sounds/{wwiseName}.asset";

            // load existing or create new
            SoundDataSO soundData = AssetDatabase.LoadAssetAtPath<SoundDataSO>(assetPath);
            bool isNew = false;

            if (soundData == null)
            {
                soundData = ScriptableObject.CreateInstance<SoundDataSO>();
                isNew = true;
            }

            soundData.WwiseName = wwiseName;
            soundData.Description = columns[2];

            if (Enum.TryParse<SoundType>(columns[1], true, out SoundType parsedType))
                soundData.Type = parsedType;
            else
                soundData.Type = SoundType.WwiseEvent;

            soundData.Is2D = false;

            if (isNew)
            {
                AssetDatabase.CreateAsset(soundData, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(soundData);
            }

            sounds.Add(soundData);
        }

        AssetDatabase.SaveAssets();
        return sounds;
    }
}