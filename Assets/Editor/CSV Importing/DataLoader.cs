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
            Debug.LogError("No throwable data found in CSV");
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

    public static List<AttachmentDataSO> LoadAttachmentsCSV()
    {
        List<AttachmentDataSO> attachments = new List<AttachmentDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("AttachmentsCSV");

        if(rows != null && rows.Count <= 1)
        {
            Debug.LogError("No attachment data found in CSV");
            return attachments;
        }

        AttachmentDataSO currentAttachmentData = null;

        // starts on row 1 so skips the global header row
        for (int i = 1; i < rows.Count; i++)
        {
            var columns = rows[i];
            
            // check if the row is empty or just spacers to avoid index out of bounds
            if (columns == null || columns.Length < 3) 
            {
                continue;
            }

            // see if its a new attachment by seeing what the display name is
            if (!string.IsNullOrEmpty(columns[0]) && columns[0] != "Attachment display name" && columns[0] != "Special weapon")
            {
                string displayName = columns[0];
                string className = columns[1];
                var currentAssetPath = $"Assets/ScriptableObjects/Attachments/{className}.asset";

                // check if we already have this asset
                currentAttachmentData = AssetDatabase.LoadAssetAtPath<AttachmentDataSO>(currentAssetPath);
                bool isNew = false;

                if (currentAttachmentData == null)
                {
                    currentAttachmentData = ScriptableObject.CreateInstance<AttachmentDataSO>();
                    isNew = true;
                }

                currentAttachmentData.DisplayName = displayName;
                currentAttachmentData.ClassName = className;
                
                currentAttachmentData.Modifiers = new List<StatModifier>();

                if (isNew)
                {
                    AssetDatabase.CreateAsset(currentAttachmentData, currentAssetPath);
                }
                
                attachments.Add(currentAttachmentData);
                continue; // move to next row which should be the first weapon (pistol)
            }

            // if we are inside an attachment block process the weapon row
            if (currentAttachmentData != null && !string.IsNullOrEmpty(columns[1]))
            {
                string weaponClassName = columns[1];
                int.TryParse(columns[2], out int resinCost);
                
                // confirm this weapon class actually exists, if it doesent skip it
                string weaponPath = $"Assets/ScriptableObjects/Weapons/{weaponClassName}.asset";
                WeaponDataSO weaponData = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(weaponPath);
                if (weaponData == null)
                {
                    Debug.Log($"The weapon: {weaponClassName} does not exist in the weapon scriptable objects folder"); 
                    continue;
                }
                
                // loop through the stat columns starting at index 3 because 2 is resin cost which doesent matter here
                for (int ci = 3; ci < columns.Length; ci++)
                {
                    // check if there is actually a value here
                    if (string.IsNullOrEmpty(columns[ci])) continue;

                    if (float.TryParse(columns[ci], out float statValue))
                    {
                        // This is like all really fragile so make sure everything is in the correct order with correct values
                        // if the enum and column headings dont match itll break
                        
                        StatModifier modifier = new StatModifier();
                        modifier.WeaponClassName = weaponClassName;
                        modifier.ResinCost = resinCost; // assign the cost from col 2
                        
                        // index 3 in CSV is FireRateRPM (index 0 in WeaponStat enum).
                        modifier.Stat = (WeaponStat)(ci - 3);

                        // logic for determining if we add or multiply
                        // columns 3, 6, 7, 8, 9, 11, 14 are multipliers
                        if (ci == 3 || ci == 6 || ci == 7 || ci == 8 || ci == 9 || ci == 11 || ci == 14)
                        {
                            modifier.Operation = StatOperation.Multiply;
                        }
                        else
                        {
                            modifier.Operation = StatOperation.Add;
                        }

                        modifier.Value = statValue;
                        currentAttachmentData.Modifiers.Add(modifier);
                    }
                }

                // apply the list back to the array
                EditorUtility.SetDirty(currentAttachmentData);
            }
        }

        AssetDatabase.SaveAssets();
        return attachments;
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
            
            // MaxHearingRadius
            if (string.IsNullOrEmpty(columns[3]))
            {
                soundData.MaxHearingRadius = 0f; // or a default value
            }
            else if (!float.TryParse(columns[3], out soundData.MaxHearingRadius))
            {
                throw new FormatException($"Invalid float for MaxHearingRadius: '{columns[3]}'");
            }

            // BaseLoudness
            if (string.IsNullOrEmpty(columns[4]))
            {
                soundData.BaseLoudness = 0f; // or a default value
            }
            else if (!float.TryParse(columns[4], out soundData.BaseLoudness))
            {
                throw new FormatException($"Invalid float for BaseLoudness: '{columns[4]}'");
            }

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