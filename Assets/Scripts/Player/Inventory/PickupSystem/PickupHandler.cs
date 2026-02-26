using System;
using Player.Inventory.PickupSystem;
using UnityEngine;

public class PickupHandler : MonoBehaviour
{
    [SerializeField] private string pickupTag = "Pickup";
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(pickupTag))
            return;

        Pickup(other);
    }

    private void Pickup(Collider pickupCollider)
    {
        PickupObject obj = pickupCollider.GetComponent<PickupObject>();
        // do the pickup
        switch (obj.type)
        {
            case PickupObjectType.Ammo:
                switch (obj.ammoType)
                {
                    case AmmoType.Primary:
                        GameManager.PlayerData.AdjustNormalAmmoCount(obj.value);
                        break;
                    case AmmoType.Secondary:
                        GameManager.PlayerData.AdjustSecondaryAmmoCount(obj.value);
                        break;
                }
                break;
            case PickupObjectType.Health:
                GameManager.PlayerData.SetCurrentHealth(GameManager.PlayerData.CurrentHealth + obj.value);
                break;
        }
        
        obj.ConsumeObject();
    }
}