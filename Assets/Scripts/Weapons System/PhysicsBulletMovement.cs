using UnityEngine;
using System.Collections.Generic;


public class PhysicsBulletMovement : MonoBehaviour
{
    // value set by weapons system
    [HideInInspector] public Vector3 InitialDirection; // Forward direction
    [HideInInspector] public float InitialVelocity;
    [HideInInspector] public float Damage = 1f;
    [HideInInspector] public float MassKG;
    [HideInInspector] public LayerMask Shootable;
    
    private Crosshair crosshair;
    
    // constants
    private const float gravity = 9.81f; // m/s²
    
    // internal values
    private Vector3 velocity;
    void Start()
    {
        crosshair = FindFirstObjectByType<Crosshair>();
        velocity = InitialDirection.normalized * InitialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        // check for all objects hit along the projectile path
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit,Mathf.Min(velocity.magnitude * Time.fixedDeltaTime, 20f), Shootable))
        {
            transform.position = hit.point;
            Collider collider = hit.collider;

            // if hit something
            if (IsInLayerMask(collider.gameObject, Shootable))
            {
                // apply damage if we hit an enemy
                if (hit.collider.CompareTag("Enemy"))
                {
                    hit.collider.GetComponent<AIController>().TakeDamage(Damage);
                    crosshair.Hitmarker();
                }
                
                // destroy self after hit
                Destroy(gameObject);
                return;
            }
        }
        
        // calculate simple mass-based drag
        float damping = 0.01f; // adjust as needed
        Vector3 dragAcceleration = -velocity * (damping / MassKG);

        // update velocity (gravity + drag)
        velocity += (dragAcceleration + Vector3.down * gravity) * Time.fixedDeltaTime;

        // update position
        transform.position += velocity * Time.fixedDeltaTime;
    }
    
    bool IsInLayerMask(GameObject obj, LayerMask mask) 
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }
}
