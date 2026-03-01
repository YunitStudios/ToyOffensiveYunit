using UnityEngine;
using UnityEngine.AI;

namespace Player.Inventory.PickupSystem
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] public PickupDatabaseSO database;
        [SerializeField] private float navSampleRadius = 2f;

        public void SpawnPickup(PickupDatabaseSO.PickupDatabaseEntry? type, Vector3 desiredPosition)
        {
            if (!type.HasValue)
                return;

            PickupDatabaseSO.PickupDatabaseEntry entry = type.Value;

            GameObject prefab = entry.prefab;
            if (prefab == null)
                return;

            if (NavMesh.SamplePosition(
                    desiredPosition,
                    out NavMeshHit hit,
                    navSampleRadius,
                    NavMesh.AllAreas))
            {
                PickupObject pickupObj = Instantiate(prefab, hit.position, Quaternion.identity).GetComponent<PickupObject>();
                pickupObj.type = entry.type;
                pickupObj.ammoType = entry.ammoType;
                pickupObj.value = entry.value;
            }
        }
    }
}