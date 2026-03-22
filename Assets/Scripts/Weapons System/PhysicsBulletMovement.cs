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
    [SerializeField] private float HeadshotMultiplier = 1.5f;

    [Header("Output Events")] 
    [SerializeField] private VoidEventChannelSO onBulletHitEnemy;
    [SerializeField] private VoidEventChannelSO onBulletHeadshotEnemy;
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
                if (hit.transform.TryGetComponentInParent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    Debug.Log("Player health hit");

                    float dmgMultiplier = 1f;
                    
                    // TODO: I dont know if designers want the headshot multiplier on the player so ill just ignore it here
                    // if(collider.gameObject.CompareTag("Head"))
                    //     dmgMultiplier = HeadshotMultiplier;
                    
                    playerHealth.TakeDamage(this, Damage * dmgMultiplier);
                    // onShowHitmarker.Invoke();
                }
                else
                {
                    if (hit.transform.TryGetComponentInParent<IDamageable>(out IDamageable target))
                    {
                        bool hitHead = false;
                        Debug.Log("Enemy health hit");
                        
                        float dmgMultiplier = 1f;
                        if (collider.gameObject.CompareTag("Head"))
                        {
                            hitHead = true;
                            dmgMultiplier = HeadshotMultiplier;
                        }
                        
                        target.TakeDamage(this, Damage * dmgMultiplier);

                        onShowHitmarker.Invoke();
                        
                        if(hitHead)
                            onBulletHeadshotEnemy?.Invoke();
                        
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

// unity doesent have this so i just made one
public static class TransformExtensions
{
    public static bool TryGetComponentInParent<T>(this Transform transform, out T component) where T : class
    {
        component = transform.GetComponentInParent<T>();
        return component != null;
    }
}    