using System.Collections.Generic;
using UnityEngine;

namespace Player.Inventory.PickupSystem
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Pickup Database")]
    public class PickupDatabaseSO : ScriptableObject
    {
        [System.Serializable]
        public struct PickupDatabaseEntry
        {
            public PickupObjectType type;
            public AmmoType ammoType;   // only relevant if type == Ammo
            public GameObject prefab;
            public int value;
        }

        [SerializeField] private List<PickupDatabaseEntry> entries;

        private Dictionary<PickupObjectType, PickupObject> lookup;

        public PickupDatabaseEntry? GetRandomPickup()
        {
            if (entries == null || entries.Count == 0)
                return null;

            int index = Random.Range(0, entries.Count);
            return entries[index];
        }
    }
}