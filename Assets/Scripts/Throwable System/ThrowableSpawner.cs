using System;
using System.Collections;
using UnityEngine;

// this will likely be used by a throwable system that handles things like the inventory. This just spawns and throws them, should be on an empty at where theyre thrown from
// on the player this will probably be on the player hand? For use with animation. Im unsure currently
public class ThrowableSpawner : MonoBehaviour
{
    [SerializeField] private Transform baseRotation;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpForce = 0.2f;
    public void ThrowObject(ThrowableDataSO throwableDataSO)
    {
        if (throwableDataSO.ThrowablePrefab == null)
        {
            Debug.LogError($"No prefab set for {throwableDataSO.ClassName}");
            return;
        }

        // instantiate the prefab from the SO
        GameObject instance = Instantiate(throwableDataSO.ThrowablePrefab, transform.position, baseRotation.rotation);

        // make sure it has a Rigidbody for physics
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = instance.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // fixes grenade throw being affected by the player
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Throwable"));

        // throw the prefab
        Vector3 throwDirection = baseRotation.forward + Vector3.up * 0.2f; // slight arc
        rb.AddForce(throwDirection.normalized * throwForce, ForceMode.VelocityChange);
        
    }
}
