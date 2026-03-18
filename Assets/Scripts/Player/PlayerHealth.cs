using System;
using EditorAttributes;
using PrimeTween;
using UI.HUD;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerHealth : Health, IDamageable
{

    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private PlayerCamera playerCamera;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private float deathDelay = 2f;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)] 
    [SerializeField] private VoidEventChannelSO onMissionFail;

    private Tween deathTween;

    [Header("Movement Script")]
    [SerializeField] private PlayerMovement playerMovement;

    protected override void Start()
    {
        CurrentHealth = GameManager.PlayerData.MaxHealth;
    }

    protected override void HealthChanged()
    {
        if(GameManager.PlayerData)
            GameManager.PlayerData.StoreCurrentHealth(CurrentHealth);
    }

    public void TakeDamage(IDamageSource source, float damage)
    {
        if(source is PhysicsBulletMovement)
            RadialHUDDisplay.SpawnMarker?.Invoke(source.damageSourcePos, MarkerTypes.Damage);
        DealDamage(damage);
    }

    protected override void Die()
    {
        if (!base.isDead)
        {
            playerMovement.enabled = false;
            base.Die();
                    
            playerAnimator.CrossFadeInFixedTime("Die", 0.2f);
            
            playerAnimator.SetLayerWeight(1, 0);
            rigBuilder.enabled = false;
            
            playerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
            
            deathTween = Tween.Delay(deathDelay, DieFinish);
        }
    }

    private void DieFinish()
    {
        onMissionFail?.Invoke();
    }
}
