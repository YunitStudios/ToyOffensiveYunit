using EditorAttributes;
using SoundSystem;
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
        
        [Header("Sound elements")]
        [SerializeField] private WwisePlayer audioPlayer;
        [SerializeField] private SoundDataSO healthPickupSound;
        [SerializeField] private SoundDataSO ammoPickupSound;

        public void ConsumeObject()
        {
            // do effects/sounds/etc
            switch (type)
            {
                case PickupObjectType.Health:
                    audioPlayer.PlaySound(healthPickupSound);
                    break;
                case PickupObjectType.Ammo:
                    audioPlayer.PlaySound(ammoPickupSound); 
                    break;
            }
            Destroy(this.gameObject);
        }
    }
}