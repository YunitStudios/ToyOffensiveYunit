using UnityEngine;
using System.Collections.Generic;


public class PhysicsBulletMovement : MonoBehaviour, IDamageSource
{
    // value set by weapons system
    [HideInInspector] public Vector3 InitialDirection; // Forward direction
    [HideInInspector] public float InitialVelocity;
    [HideInInspector] public float Damage = 1f;
    [HideInInspector] public float MassKG;
    [HideInInspector] public LayerMask Shootable;

    [Header("Output Events")] 
    [SerializeField] private VoidEventChannelSO onBulletHitEnemy;
    [SerializeField] private VoidEventChannelSO onShowHitmarker;
    
    private Vector3 bulletSpawnPoint; // This will hold the "Snapshot"
    
    // constants
    private const float gravity = 9.81f; // m/s²
    
    // internal values
    private Vector3 velocity;
    void Start()
    {
        velocity = InitialDirection.normalized * InitialVelocity;
        damageSourcePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // check for all objects hit along the projectile path
        RaycastHit hit;
        float distance = (velocity.magnitude * Time.deltaTime);

        if (Physics.Raycast(transform.position, velocity.normalized, out hit, distance, Shootable))
        {
            transform.position = hit.point;
            Collider collider = hit.collider;

            // Debug.Log(hit.collider.name);

            // if hit something
            if (IsInLayerMask(collider.gameObject, Shootable))
            {
                // TODO: This is a bit of a bodge? Really if the AI needed its own damage system it should be using events from the generic health system, not intercepting it and dealing its own damage
                if (collider.gameObject.CompareTag("Player"))
                {
                    if (hit.transform.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                    {
                        playerHealth.TakeDamage(this, Damage);
                        Debug.Log("Damage Dealt: " + Damage);
                        // onShowHitmarker.Invoke();
                    }
                }
                else
                {
                    if (hit.transform.TryGetComponent<IDamageable>(out IDamageable target))
                    {
                        target.TakeDamage(this, Damage);
                        // TODO: THIS CAUSES ISSUES MMAKING IT REGISTER THE PLAYERS HITMARKER WHEN THE ENEMIES SHOOT

                        if (collider.gameObject.CompareTag("Enemy"))
                        {
                            onShowHitmarker.Invoke();
                        }
                        // onShowHitmarker.Invoke();
                        
                        onBulletHitEnemy?.Invoke();
                    }
                }

                // destroy self after hit
                Destroy(gameObject);
                return;
            }

            // destroy self after hit
            Destroy(gameObject);
            return;
        }

        // calculate simple mass-based drag
        float damping = 0.01f; // adjust as needed
        Vector3 dragAcceleration = -velocity * (damping / MassKG);

        // update velocity (gravity + drag)
        velocity += (dragAcceleration + Vector3.down * gravity) * Time.deltaTime;

        // update position
        transform.position += velocity * Time.deltaTime;
    }
    
    bool IsInLayerMask(GameObject obj, LayerMask mask) 
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }
    public Vector3 damageSourcePos { get; set; }
}
