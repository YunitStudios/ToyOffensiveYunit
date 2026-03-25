using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using PrimeTween;
using SoundSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WeaponsSystem : MonoBehaviour
{
    private static readonly int IsAiming = Animator.StringToHash("IsAiming");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");

    [Header("References")]
    [Tooltip("Player camera used for the obstruction check")]
    [SerializeField] private Camera playerCamera;   // player camera used for the obstruction check

    [SerializeField] private PlayerCamera playerCameraSystem;
    [FormerlySerializedAs("playerLayer")]
    [Tooltip("Layer mask to stop the gun from shooting the player torso")]
    [SerializeField] private LayerMask canShoot;
    [Tooltip("Layer mask of what the enemies are on")]
    [SerializeField] private LayerMask enemyLayerMask;
    [Tooltip("The point that the gun actually shoots from, will be obtained dynamically in the future")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private WwisePlayer wwisePlayer;
    
    private PlayerDataSO PlayerData => GameManager.PlayerData;
    private Weapon currentWeapon => PlayerData.CurrentWeapon;
    
    // tracer
    [SerializeField] private GameObject tracerPrefab;   // assign in Inspector
    [SerializeField] private float tracerSpeed = 200f;  // only for moving tracers (optional)
    
    // physics projectile
    [SerializeField] private GameObject physicsProjectilePrefab;

    [SerializeField] private SerializedDictionary<WeaponDataSO, GameObject> weaponModels;

    [Header("Output Events")]
    [SerializeField] private VoidEventChannelSO onShowHitmarker;
    [SerializeField] private FloatEventChannelSO onUpdateSpread;
    [SerializeField] private FloatEventChannelSO onUpdateReload;
    [SerializeField] private VoidEventChannelSO onWeaponFired;

    // timing values
    private float lastShotTime = 0;                 // time in seconds since the start of the application when the last shot happened
    private float accumulatedShootingTime = 0f;     // total time spent shooting, used for recovery speed
    private float reloadTime = 0;

    private bool aiming = false;
    private Tween weaponSwapTimer;

    private float ReloadProgress => Mathf.Clamp01(1-(reloadTime / currentWeapon.ModifiedWeaponData.ReloadTime));

    [SerializeField] private PlayerMovement playerMovement;
    
    [SerializeField] private Crosshair crosshair;
    [SerializeField] private ReloadPromptUI reloadPromptUI;

    private bool weaponFrozen;

    private bool IsReloading => reloadTime > 0;
    private bool CanSwitchWeapon =>  !aiming;

    private IEnumerator Start()
    {
        InputManager.Instance.OnReloadAction += Reload;
        
        crosshair = FindObjectOfType<Crosshair>();
        reloadPromptUI =  FindObjectOfType<ReloadPromptUI>();
        wwisePlayer = GetComponent<WwisePlayer>();

        yield return null;
        
        SetWeaponVisual();
    }

    private void OnDisable()
    {
        InputManager.Instance.OnReloadAction -= Reload;
    }

    private void Update()
    {
        if (GameManager.PlayerData.IsAlive && !weaponFrozen)
        {
            // Check for weapon switch
            WeaponSwitching();
            
            // TODO: use events this is temp due to it not working for unknown reason
            playerMovement.PlayerAnimator.SetBool(IsShooting, false);
            if (InputManager.Instance.IsShooting)
            {
                Fire();
            }
    
            if (InputManager.Instance.AimHeld && !aiming)
                AimStart();
            
            // Exit state if not holding input OR if they can no longer aun in this state
            if ((!InputManager.Instance.AimHeld || !playerMovement.CanAim) && aiming)
            {
                AimStop();
            }
        }
        
        currentWeapon.WeaponSpread.UpdateSpreadOverTime();
        
        //UI Crosshair update
        onUpdateSpread?.Invoke(currentWeapon.WeaponSpread.CurrentSpreadAmount);
        
        // Show reload prompt if out of ammo and not reloading
        if(IsReloading)
        {
            // Reload update
            reloadTime -= Time.deltaTime;

            onUpdateReload?.Invoke(ReloadProgress);

            if (!IsReloading)
            {
                reloadPromptUI.Hide();
                currentWeapon.Reload(PlayerData);
            }
            
        }
        if (currentWeapon.CurrentAmmoInMag <= 0 && !IsReloading)
        {
            reloadPromptUI.ShowReloadPrompt();
        }
        else if (IsReloading)
        {
            reloadPromptUI.ShowReloading();

        }
        else
        {
            reloadPromptUI.Hide();
        }
        

    }

    private void WeaponSwitching()
    {
        if (!CanSwitchWeapon || weaponSwapTimer.isAlive)
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
        SetWeaponVisual();
        ReloadCancel();
    }

    private Tween swapControllerTween;
    private void SetWeaponVisual()
    {
        SwapController();
        
        // Disable all gun models and disable current
        foreach (var model in weaponModels)
        {
            model.Value.SetActive(false);
        }
        if(weaponModels.TryGetValue(PlayerData.CurrentWeapon.BaseWeaponData, out var newModel))
            newModel.SetActive(true);
    }

    // ITS SO JANK BUT IT LOWKEY WORKS
    // https://www.reddit.com/r/Unity3D/comments/1clna15/tutorial_workaround_for_swapping/
    private void SwapController()
    {
        if(swapControllerTween.isAlive)
            swapControllerTween.Stop();

        playerMovement.PlayerAnimator.CrossFadeInFixedTime("Transition", 0.1f);
        swapControllerTween = Tween.Delay(0.1f, () =>
        {
            playerMovement.PlayerAnimator.runtimeAnimatorController = PlayerData.CurrentWeapon.BaseWeaponData.animationController;
            playerMovement.PlayerAnimator.PlayInFixedTime("Transition", 0, 0.1f);
        });
    }

    // called when for example the player clicks, or called every frame if holding down for full auto
    private void Fire()
    {
        
        // Cant shoot in some states
        if (!playerMovement.CanShoot)
            return;
        
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
            reloadPromptUI.ShowReloadPrompt();
            return;
        }
        reloadPromptUI.Hide();

        playerMovement.PlayerAnimator.SetBool(IsShooting, true);
        
        // limit it so you can only shoot up to the max fire rate
        float timeBetweenShots = 60f / currentWeapon.ModifiedWeaponData.FireRateRPM;
        if(Time.time - lastShotTime < timeBetweenShots)
        {
            // Debug.Log("Shooting too fast!");
            return;
        }

        // Debug.Log("Firing weapon: " + currentWeapon.WeaponData.DisplayName);
        
        // shoot it
        if (currentWeapon.ModifiedWeaponData.ShotQuantity > 1)
        {
            DoMultiShoot(currentWeapon.ModifiedWeaponData.IsPhysicsBased);
        }
        else
        {
            if(currentWeapon.ModifiedWeaponData.IsPhysicsBased)
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
        
        if (wwisePlayer is not null)
        {
            if (currentWeapon.ModifiedWeaponData.SoundLoudnessMultiplier != 1f)
            {
                // if loudness is multiplied meaning a supressor or something
                if(wwisePlayer is not null)
                    wwisePlayer.PlaySupressedSound(currentWeapon.ModifiedWeaponData.soundPack.Gunshot, currentWeapon.ModifiedWeaponData.SoundLoudnessMultiplier);
            }
            else
            {
                wwisePlayer.PlaySound(currentWeapon.ModifiedWeaponData.soundPack.Gunshot);
            }
        }

        onWeaponFired?.Invoke();
    }

    private void AimStart()
    {
        if (playerMovement.CanAim)
        {
            if (playerCameraSystem.CurrentCameraType != currentWeapon.ModifiedWeaponData.AimCameraType)
            {
                playerCameraSystem.ChangeCamera(currentWeapon.ModifiedWeaponData.AimCameraType);
                aiming = true;
                currentWeapon.WeaponSpread.IsAiming = aiming;
                PlayerData.ToggleAiming(aiming);
                playerMovement.PlayerAnimator.SetBool(IsAiming, true);
                
                if(wwisePlayer is not null)
                    wwisePlayer.PlaySound(currentWeapon.ModifiedWeaponData.soundPack.ADS);

            }
        }
    }

    private void AimStop()
    {
        aiming = false;
        currentWeapon.WeaponSpread.IsAiming = aiming;
        playerMovement.PlayerAnimator.SetBool(IsAiming, false);
        PlayerData.ToggleAiming(aiming);
        
        // Only reset the camera if current camera is ADS
        if (playerCameraSystem.CurrentCameraType == currentWeapon.ModifiedWeaponData.AimCameraType)
        {
            // Cba to make a proper system
            if(playerMovement.CurrentState is ParachuteState)
                playerCameraSystem.ChangeCamera(PlayerCamera.CameraType.Parachute);
            else
                playerCameraSystem.ResetCamera();
        }
        
        if(wwisePlayer is not null)
            wwisePlayer.PlaySound(currentWeapon.ModifiedWeaponData.soundPack.ADS);
    }

    private void Reload()
    {
        if (weaponFrozen)
            return;
        
        if(IsReloading)
            return;
        
        // Dont reload if mag full
        if (currentWeapon.CurrentAmmoInMag >= currentWeapon.ModifiedWeaponData.MagSize)
            return;
        
        if(wwisePlayer is not null)
            wwisePlayer.PlaySound(currentWeapon.ModifiedWeaponData.soundPack.Reload_Empty);
        
        if (ReloadProgress >= 1)
        {
            accumulatedShootingTime = 0f;

            reloadTime = currentWeapon.ModifiedWeaponData.ReloadTime;
        }
    }

    private void ReloadCancel()
    {
        reloadTime = 0;
        onUpdateReload.Invoke(-1);
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
                target.TakeDamage(bullet, currentWeapon.ModifiedWeaponData.Damage);
                onShowHitmarker?.Invoke();
            }
        }

        // spawn tracer
        SpawnTracer(firePoint.position, firePoint.position + shootDir * 100f);
    }

    private void DoPhysicsShoot(bool isMultiShot = false, float multiRotation = 0f)
    {
        // do the actual physics based shoot
        Vector3 camForward = playerCamera.transform.forward;
        float fallbackDistance = 200f;
        float sphereRadius = 3f;
        float debugDuration = 1f; // how long the lines stay visible

        Vector3 shotDestination;
        Ray ray = new Ray(playerCamera.transform.position, camForward);
        RaycastHit hit;

        // try direct ray from the camera first to see if we hit an enemy
        if (Physics.Raycast(ray, out hit, fallbackDistance, enemyLayerMask))
        {
            shotDestination = hit.point;
            // debug for direct hit
            Debug.DrawLine(ray.origin, hit.point, Color.green, debugDuration);
            DrawDebugSphere(hit.point, 0.2f, Color.green, debugDuration);
        }
        else
        {
            // if no direct hit, fire a SphereCast (thick ray pretty much) to find a nearby enemy where that enemy is located
            // find multiple enemies in a that cast
            RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, fallbackDistance, enemyLayerMask);

            // debug the spherecast path as a tube
            DrawDebugSphereCast(ray.origin, sphereRadius, camForward, fallbackDistance, Color.red, debugDuration);

            if (hits.Length > 0)
            {
                // find the one closest to the actual center ray
                RaycastHit bestHit = hits[0];
                float closestDist = float.MaxValue;

                foreach (var h in hits)
                {
                    // project the hit point onto the ray to see how far it is from the center line
                    Vector3 pointOnRay = Vector3.Project(h.point - playerCamera.transform.position, camForward) + playerCamera.transform.position;
                    float distToRay = Vector3.Distance(h.point, pointOnRay);

                    // debug each potential target found in the sphere
                    Debug.DrawLine(h.point, pointOnRay, Color.yellow, debugDuration);

                    if (distToRay < closestDist)
                    {
                        closestDist = distToRay;
                        bestHit = h;
                    }
                }

                // calculate destination based on the depth of the best enemy found
                float depth = Vector3.Distance(playerCamera.transform.position, bestHit.point);
                shotDestination = playerCamera.transform.position + camForward * depth;

                // debug best target
                DrawDebugSphere(bestHit.point, sphereRadius, Color.green, debugDuration);
                Debug.DrawLine(playerCamera.transform.position, shotDestination, Color.cyan, debugDuration);
            }
            else
            {
                // fallback if absolutely nothing is nearby
                shotDestination = playerCamera.transform.position + camForward * fallbackDistance;
                Debug.Log("Using fallback");
            }
        }

        Vector3 shootDir = shotDestination - firePoint.position;

        // debug the final calculated shoot direction from the gun
        Debug.DrawRay(firePoint.position, shootDir.normalized * 5f, Color.magenta, debugDuration);

        // multi shot support
        if (!isMultiShot)
        {
            Quaternion spreadRot = Quaternion.Euler(
                GetSpreadRotation(),
                GetSpreadRotation(),
                GetSpreadRotation()
            );

            shootDir = spreadRot * shootDir;
        }
        else
        {
            Debug.Log("Multi shot rotation");
            shootDir = GetShotgunRotation(camForward, multiRotation);
        }

        // instantiate and setup the physics projectile
        GameObject physicsProjectile = Instantiate(physicsProjectilePrefab, firePoint.position, firePoint.rotation);
        PhysicsBulletMovement movementScript = physicsProjectile.GetComponent<PhysicsBulletMovement>();

        movementScript.bulletFromEnemy = false;
        movementScript.InitialDirection = shootDir;
        movementScript.InitialVelocity = currentWeapon.ModifiedWeaponData.InitialVelocityMS;
        movementScript.Damage = currentWeapon.ModifiedWeaponData.Damage;
        movementScript.MassKG = currentWeapon.ModifiedWeaponData.MassKG;
        movementScript.Shootable = canShoot;
    }

    // helper to visualize the spherecast volume
    private void DrawDebugSphereCast(Vector3 origin, float radius, Vector3 direction, float distance, Color color, float duration)
    {
        Vector3 endPoint = origin + direction * distance;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(direction, up).normalized;
        if (right == Vector3.zero)
        {
            up = Vector3.forward;
            right = Vector3.Cross(direction, up).normalized;
        }
        up = Vector3.Cross(right, direction).normalized;

        // draw connecting lines for the tube shell
        Debug.DrawLine(origin + up * radius, endPoint + up * radius, color, duration);
        Debug.DrawLine(origin - up * radius, endPoint - up * radius, color, duration);
        Debug.DrawLine(origin + right * radius, endPoint + right * radius, color, duration);
        Debug.DrawLine(origin - right * radius, endPoint - right * radius, color, duration);

        // draw rings at start, middle, and end to show thickness
        DrawDebugRing(origin, up, right, radius, color, duration);
        DrawDebugRing(origin + direction * (distance * 0.5f), up, right, radius, color, duration);
        DrawDebugRing(endPoint, up, right, radius, color, duration);
    }

    // helper to draw the circular rings of the tube
    private void DrawDebugRing(Vector3 center, Vector3 up, Vector3 right, float radius, Color color, float duration)
    {
        int segments = 10;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            float nextAngle = (i + 1) * Mathf.PI * 2 / segments;

            Vector3 p1 = center + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
            Vector3 p2 = center + (right * Mathf.Cos(nextAngle) + up * Mathf.Sin(nextAngle)) * radius;

            Debug.DrawLine(p1, p2, color, duration);
        }
    }

    // helper to visualize hits since Debug.DrawSphere doesnt exist
    private void DrawDebugSphere(Vector3 point, float radius, Color color, float duration)
    {
        Debug.DrawLine(point + Vector3.up * radius, point + Vector3.down * radius, color, duration);
        Debug.DrawLine(point + Vector3.left * radius, point + Vector3.right * radius, color, duration);
        Debug.DrawLine(point + Vector3.forward * radius, point + Vector3.back * radius, color, duration);
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
        public Vector3 damageSourcePos { get; set; }
    }

    public void SetWeaponFrozen(bool isFrozen)
    {
        weaponFrozen =  isFrozen;
        if (isFrozen)
        {
            aiming = false;
            currentWeapon.WeaponSpread.IsAiming = aiming;
            playerCameraSystem.ResetCamera();
        }
    }
}
