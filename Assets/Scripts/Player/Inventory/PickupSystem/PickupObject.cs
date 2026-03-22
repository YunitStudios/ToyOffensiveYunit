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
        public AmmoType ammoType;   // only relevant if type == Ammo
        public int value;

        public void ConsumeObject()
        {
            // do effects/sounds/etc
            Destroy(this.gameObject);
        }
    }
}