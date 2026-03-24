using UnityEngine;
using System.Collections;
using SoundSystem;

public class AIWeaponSystem : MonoBehaviour
{
    [Tooltip("Layer mask to stop the gun from shooting the player torso")]
    [SerializeField] private LayerMask canShoot;
    [Tooltip("The point that the gun actually shoots from, will be obtained dynamically in the future")]
    [SerializeField] private Transform firePoint;
    
    [HideInInspector] public Weapon currentWeapon;
    
    // tracer
    public GameObject tracerPrefab;   // assign in Inspector
    public float tracerSpeed = 200f;  // only for moving tracers (optional)
    
    // physics projectile
    [SerializeField] private GameObject physicsProjectilePrefab;
    
    // timing values
    private float lastShotTime = 0;                 // time in seconds since the start of the application when the last shot happened
    private float accumulatedShootingTime = 0f;     // total time spent shooting, used for recovery speed
    private float lastReloadTime = -999f;
    
    [HideInInspector] public Transform target;
    private AIInventory aiInventory;

    private float damageMult = 0.5f;
    private float accuracyMult = 1f;
 
    [Tooltip("Delay before the enemy will start shooting")]
    [SerializeField] private Vector2 shootCooldownRange = new Vector2(1.0f, 3.0f);
    [Tooltip("How long the enemy will Shoot for")]
    [SerializeField] private Vector2 shootPeriodRange = new Vector2 (1.0f, 3.0f);
    [HideInInspector] public float currentShootCooldown;
    [HideInInspector] public float currentShootPeriod;
    private float shootCooldownTimer;
    private float shootPeriodTimer;
    private AIStateMachine aiStateMachine;
    
    [SerializeField] private WwisePlayer wwisePlayer;


    private void Start()
    {
        aiStateMachine = GetComponentInParent<AIStateMachine>();
        damageMult = aiStateMachine.DamageMultiplier;
        accuracyMult = aiStateMachine.AccuracyMultiplier;
        aiInventory = GetComponent<AIInventory>();
        currentWeapon = aiInventory.GetPrimaryWeapon();
        RandomiseShootTimes();
        currentWeapon.WeaponSpread.IsAiming = false;
        currentWeapon.WeaponSpread.ResetSpread();
        wwisePlayer = GetComponent<WwisePlayer>();
    }

    public bool CanFire()
    {
        shootCooldownTimer += Time.deltaTime;
        if (shootCooldownTimer < currentShootCooldown)
        {
            return false;
        }
        
        shootPeriodTimer += Time.deltaTime;
        if (shootPeriodTimer >= currentShootPeriod)
        {
            ResetFireTimers();
            RandomiseShootTimes();
            return false;
        }
        
        return true;
    }
    
    
    public void Fire()
    {
        // check if ready to shoot
        if (!CanFire())
        {
            return;
        }
        
        // check we aren't still reloading
        if(Time.time - lastReloadTime < currentWeapon.ModifiedWeaponData.ReloadTime)
        {
            // am still reloading
            return;
        }

        if(currentWeapon.CurrentAmmoInMag <= 0)
        {
            // play empty mag sound here
            Reload();
            return;
        }

        // limit it so you can only shoot up to the max fire rate
        float timeBetweenShots = 60f / currentWeapon.ModifiedWeaponData.FireRateRPM;
        if(Time.time - lastShotTime < timeBetweenShots)
        {
            return;
        }

        
        // do the actual shooting here
        if(currentWeapon.ModifiedWeaponData.IsPhysicsBased)
        {
            DoPhysicsShoot();
        }
        else
        {
            Vector3 endPos = DoRaycastShoot();
            SpawnTracer(firePoint.position, endPos);
        }

        // do the spread calculations
        lastShotTime = Time.time;

        
        currentWeapon.Fire();
        if(wwisePlayer is not null)
            wwisePlayer.PlaySound(currentWeapon.ModifiedWeaponData.soundPack.Gunshot);
    }
    
    public void Reload()
    {
        if(wwisePlayer is not null)
            wwisePlayer.PlaySound(currentWeapon.ModifiedWeaponData.soundPack.Reload_Empty);
        
        accumulatedShootingTime = 0f;
        lastReloadTime = Time.time;
        
        ResetFireTimers();
        RandomiseShootTimes();

        currentWeapon.EnemyReload(aiInventory);
    }
    
    private void DoMultiShoot(bool isPhysicsBased = false)
    {
        for (int i = 0; i < currentWeapon.ModifiedWeaponData.ShotQuantity; i++)
        {
            // calculate shot offset rotation
            float max = currentWeapon.ModifiedWeaponData.ShotSpread;
            
            if (isPhysicsBased)
            {
                DoPhysicsShoot(true, Random.Range(-max, max));
            }
            else
            {
                DoRaycastShoot(true, Random.Range(-max, max));
            }
        }
    }

    private Vector3 DoRaycastShoot(bool isMultiShot = false, float multiRotation = 0f)
    {
        // Direction to target center
        Vector3 direction = ((target.position + Vector3.up) - firePoint.position).normalized;

        if (!isMultiShot)
        {
            Quaternion spreadRot = Quaternion.Euler(
                GetSpreadRotation(),
                GetSpreadRotation(),
                GetSpreadRotation()
            );
            direction = spreadRot * direction;
        }

        // debug lines
        Debug.DrawRay(firePoint.position, direction * 100f, Color.red, 10f);

        // raycast from gun along shoot direction
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, Mathf.Infinity, canShoot))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Health>().DealDamage(currentWeapon.ModifiedWeaponData.Damage * damageMult);
            }
            
            return hit.point;
        }
        return firePoint.position + direction * 100f; // 100 units forward
    }

    
    private void DoPhysicsShoot(bool isMultiShot = false, float multiRotation = 0f)
    {
        // do the actual physics based shoot for rockets, arrows etc
        Vector3 direction = ((target.position + Vector3.up) - firePoint.position).normalized;
        Vector3 shootDir;

        // multi shot support
        if (!isMultiShot)
        {
            Quaternion spreadRot = Quaternion.Euler(
                GetSpreadRotation(),
                GetSpreadRotation(),
                GetSpreadRotation()
            );

            shootDir = spreadRot * direction;
        }
        else
        {
            Debug.Log("Multi shot rotation");
            shootDir = GetShotgunRotation(direction, multiRotation);
        }
        
        // instantiate and set up the physics projectile
        GameObject physicsProjectile = Instantiate(physicsProjectilePrefab, firePoint.position, firePoint.rotation);
        PhysicsBulletMovement movementScript = physicsProjectile.GetComponent<PhysicsBulletMovement>();
        movementScript.bulletFromEnemy = true;
        movementScript.InitialDirection = shootDir;
        movementScript.InitialVelocity = currentWeapon.ModifiedWeaponData.InitialVelocityMS;
        movementScript.Damage = currentWeapon.ModifiedWeaponData.Damage * damageMult;
        movementScript.MassKG = currentWeapon.ModifiedWeaponData.MassKG;
        movementScript.Shootable = canShoot;
    }
    
    private float GetSpreadRotation()
    {
        float max = currentWeapon.WeaponSpread.CurrentSpreadAmount * accuracyMult;
        return Random.Range(-max, max);
    }
    
    private Vector3 GetShotgunRotation(Vector3 forward, float angle)
    {
        // this makes a uniform cone based on the angle we give and then randomises a location in that as the offset
        float angleRad = angle * Mathf.Deg2Rad;

        Vector3 random = Random.onUnitSphere;
        Vector3 axis = Vector3.Cross(forward, random).normalized;

        float theta = Random.Range(0f, angleRad);

        return Quaternion.AngleAxis(theta * Mathf.Rad2Deg, axis) * forward;
    }
    
    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(tracerPrefab, start, Quaternion.identity);

        Vector3 direction = (end - start).normalized;

        // move the tracer with a coroutine
        StartCoroutine(MoveTracer(tracer, end));
    }
    
    private IEnumerator MoveTracer(GameObject tracer, Vector3 end)
    {
        while (tracer)
        {
            tracer.transform.position = Vector3.MoveTowards(
                tracer.transform.position,
                end,
                tracerSpeed * Time.deltaTime
            );

            if (Vector3.SqrMagnitude(tracer.transform.position - end) <= 0.01f)
                break;

            yield return null;
        }

        if (tracer)
            Destroy(tracer);
    }

    private void RandomiseShootTimes()
    {
        currentShootCooldown = Random.Range(shootCooldownRange.x, shootCooldownRange.y);
        currentShootPeriod = Random.Range(shootPeriodRange.x, shootPeriodRange.y);
    }

    public void ResetFireTimers()
    {
        shootCooldownTimer = 0f;
        shootPeriodTimer = 0f;
    }

    public bool IsReloading()
    {
        return Time.time - lastReloadTime < currentWeapon.ModifiedWeaponData.ReloadTime;
    }

    public bool IsInShootPeriod()
    {
        return shootCooldownTimer >= currentShootCooldown && shootPeriodTimer < currentShootPeriod;
    }
}
