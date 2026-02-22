using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WeaponsSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player camera used for the obstruction check")]
    [SerializeField] private Camera playerCamera;   // player camera used for the obstruction check

    [SerializeField] private PlayerCamera playerCameraSystem;
    [FormerlySerializedAs("playerLayer")]
    [Tooltip("Layer mask to stop the gun from shooting the player torso")]
    [SerializeField] private LayerMask canShoot;
    [Tooltip("The point that the gun actually shoots from, will be obtained dynamically in the future")]
    [SerializeField] private Transform firePoint;
    
    private PlayerDataSO PlayerData => GameManager.PlayerData;
    private Weapon currentWeapon => PlayerData.CurrentWeapon;
    
    // tracer
    [SerializeField] private GameObject tracerPrefab;   // assign in Inspector
    [SerializeField] private float tracerSpeed = 200f;  // only for moving tracers (optional)
    
    // physics projectile
    [SerializeField] private GameObject physicsProjectilePrefab;

    [Header("Output Events")]
    [SerializeField] private VoidEventChannelSO onShowHitmarker;
    [SerializeField] private FloatEventChannelSO onUpdateSpread;
    [SerializeField] private FloatEventChannelSO onUpdateReload;

    // timing values
    private float lastShotTime = 0;                 // time in seconds since the start of the application when the last shot happened
    private float accumulatedShootingTime = 0f;     // total time spent shooting, used for recovery speed
    private float lastReloadTime = -999f;

    private bool aiming = false;
    private Tween weaponSwapTimer;

    private float ReloadProgress => (Time.time - lastReloadTime) / currentWeapon.WeaponData.ReloadTime;

    private void Start()
    {
        InputManager.Instance.OnReloadAction += Reload;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnReloadAction -= Reload;
    }

    private void Update()
    {
        // Check for weapon switch
        WeaponSwitching();
        
        // TODO: use events this is temp due to it not working for unknown reason
        if (InputManager.Instance.IsShooting)
            Fire();

        if (InputManager.Instance.IsAiming && !aiming)
            Aim();

        if (!InputManager.Instance.IsAiming && aiming)
        {
            playerCameraSystem.ResetCamera(); 
            aiming = false;
        }
        
        currentWeapon.WeaponSpread.UpdateSpreadOverTime();
        
        //UI Crosshair update
        onUpdateSpread?.Invoke(currentWeapon.WeaponSpread.CurrentSpreadAmount);
        
        // Reload update
        if(ReloadProgress > 0)
            onUpdateReload?.Invoke(ReloadProgress);
        
    }

    private void WeaponSwitching()
    {
        if (weaponSwapTimer.isAlive)
            return;
        
        PlayerDataSO.WeaponSlot currentSlot = GameManager.PlayerData.CurrentWeaponSlot;
        PlayerDataSO.WeaponSlot newSlot;
        
        float scrollValue = InputManager.Instance.FrameScroll;
        if(scrollValue != 0)
        {
            // Go to next/previous value in enum
            int scrollDelta = scrollValue > 0 ? 1 : -1;
            // Add delta to enum value, then wrap around to enum size
            int newValue = ((int)currentSlot + scrollDelta) % Enum.GetNames(typeof(PlayerDataSO.WeaponSlot)).Length;
            newSlot = (PlayerDataSO.WeaponSlot)newValue;
        }
        else if (currentSlot != PlayerDataSO.WeaponSlot.Primary && InputManager.Instance.PrimaryWeapon)
        {
            newSlot = PlayerDataSO.WeaponSlot.Primary;
        }
        else if (currentSlot != PlayerDataSO.WeaponSlot.Secondary && InputManager.Instance.SecondaryWeapon)
        {
            newSlot = PlayerDataSO.WeaponSlot.Secondary;
        }
        else
        {
            return; // no change
        }
        
        
        GameManager.PlayerData.SetWeaponSlot(newSlot);
        weaponSwapTimer = Tween.Delay(GameManager.PlayerData.WeaponSwapTime);
    }

    // called when for example the player clicks, or called every frame if holding down for full auto
    private void Fire()
    {
        // check we arent still reloading
        if(ReloadProgress < 1)
        {
            // am still reloading
            // Debug.Log("Still reloading!");
            return;
        }

        if(currentWeapon.CurrentAmmoInMag <= 0)
        {
            // play empty mag sound here
            // Debug.Log("No ammo in mag!");
            return;
        }

        // limit it so you can only shoot up to the max fire rate
        float timeBetweenShots = 60f / currentWeapon.WeaponData.FireRateRPM;
        if(Time.time - lastShotTime < timeBetweenShots)
        {
            // Debug.Log("Shooting too fast!");
            return;
        }

        // Debug.Log("Firing weapon: " + currentWeapon.WeaponData.DisplayName);
        
        // shoot it
        if (currentWeapon.WeaponData.ShotQuantity > 1)
        {
            DoMultiShoot(currentWeapon.WeaponData.IsPhysicsBased);
        }
        else
        {
            if(currentWeapon.WeaponData.IsPhysicsBased)
            {
                DoPhysicsShoot();
            }
            else
            {
                DoRaycastShoot();
            }
        }
        
        // do the spread calculations
        lastShotTime = Time.time;
        
        currentWeapon.Fire();
    }

    private void Aim()
    {
        if (playerCameraSystem.CurrentCameraType != currentWeapon.WeaponData.AimCameraType)
        {
            playerCameraSystem.ChangeCamera(currentWeapon.WeaponData.AimCameraType);
            aiming = true;
        }
    }

    private void Reload()
    {
        accumulatedShootingTime = 0f;
        lastReloadTime = Time.time;

        currentWeapon.Reload(PlayerData);
    }

    private void DoMultiShoot(bool isPhysicsBased = false)
    {
        for (int i = 0; i < currentWeapon.WeaponData.ShotQuantity; i++)
        {
            // calculate shot offset rotation
            float max = currentWeapon.WeaponData.ShotSpread;
            
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

    private void DoRaycastShoot(bool isMultiShot = false, float multiRotation = 0f)
    {
        // get camera forward aim direction
        Vector3 camForward = playerCamera.transform.forward;
        
        Vector3 shootDir;

        // apply random spread to the shot direction
        if (!isMultiShot)
        {
            Quaternion spreadRot = Quaternion.Euler(
                GetSpreadRotation(),
                GetSpreadRotation(),
                GetSpreadRotation()
            );

            shootDir = spreadRot * camForward;
        }
        else
        {
            Debug.Log("Multi shot rotation");
            shootDir = GetShotgunRotation(camForward, multiRotation);
        }

        // visualize the shot direction from the fire point
        Debug.DrawRay(firePoint.position, shootDir * 100f, Color.red, 10f);

        // raycast from gun along the spread direction
        RaycastHit hit;
        Bullet bullet = new();
        if (Physics.Raycast(firePoint.position, shootDir, out hit, Mathf.Infinity, canShoot))
        {
            // apply damage if we hit an enemy
            if (hit.transform.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(bullet, currentWeapon.WeaponData.Damage);
                onShowHitmarker?.Invoke();
            }
        }

        // spawn tracer
        SpawnTracer(firePoint.position, firePoint.position + shootDir * 100f);
    }

    private void DoPhysicsShoot(bool isMultiShot = false, float multiRotation = 0f)
    {
        // do the actual physics based shoot for rockets, arrows etc
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 shootDir;

        // multi shot support
        if (!isMultiShot)
        {
            Quaternion spreadRot = Quaternion.Euler(
                GetSpreadRotation(),
                GetSpreadRotation(),
                GetSpreadRotation()
            );

            shootDir = spreadRot * camForward;
        }
        else
        {
            Debug.Log("Multi shot rotation");
            shootDir = GetShotgunRotation(camForward, multiRotation);
        }
        
        // instantiate and setup the physics projectile
        GameObject physicsProjectile = Instantiate(physicsProjectilePrefab, firePoint.position, firePoint.rotation);
        PhysicsBulletMovement movementScript = physicsProjectile.GetComponent<PhysicsBulletMovement>();
        
        movementScript.InitialDirection = shootDir;
        movementScript.InitialVelocity = currentWeapon.WeaponData.InitialVelocityMS;
        movementScript.Damage = currentWeapon.WeaponData.Damage;
        movementScript.MassKG = currentWeapon.WeaponData.MassKG;
        movementScript.Shootable = canShoot;
    }

    private float GetSpreadRotation()
    {
        float max = currentWeapon.WeaponSpread.CurrentSpreadAmount;
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
        StartCoroutine(MoveTracer(tracer, direction, end));
    }
    
    private IEnumerator MoveTracer(GameObject tracer, Vector3 dir, Vector3 end)
    {
        while (tracer && Vector3.Distance(tracer.transform.position, end) > 0.1f)
        {
            tracer.transform.position += dir * (tracerSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(tracer);
    }

    public class Bullet : IDamageSource
    {
        public Transform transform => null;
    }
}
