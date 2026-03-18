using EditorAttributes;
using UnityEngine;

namespace Player.Inventory.PickupSystem
{
    public enum PickupObjectType
    {
        Health,
        Ammo
    }
    public enum AmmoType
    {
        Primary,
        Secondary
    }

    public class PickupObject : MonoBehaviour
    {
        public PickupObjectType type; 
        [ShowField(nameof(IsAmmo))] public AmmoType ammoType;
        public int value;
        
        private bool IsAmmo => type == PickupObjectType.Ammo;

        public void ConsumeObject()
        {
            // do effects/sounds/etc
            Destroy(this.gameObject);
        }
    }
}