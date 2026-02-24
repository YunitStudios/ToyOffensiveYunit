using System;
using EditorAttributes;
using PrimeTween;
using UnityEngine;

public class PlayerHealth : Health, IDamageable
{

    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private Animator playerAnimator;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private float deathDelay = 2f;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)] 
    [SerializeField] private VoidEventChannelSO onMissionFail;
    [SerializeField] private VoidEventChannelSO onStopLevel;

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
            GameManager.PlayerData.SetCurrentHealth(CurrentHealth);
    }

    public void TakeDamage(IDamageSource source, float damage)
    {
        DealDamage(damage);
    }

    protected override void Die()
    {
        if (!base.isDead)
        {
            playerMovement.enabled = false;
            base.Die();
                    
            playerAnimator.CrossFadeInFixedTime("Die", 0.2f);
    
            deathTween = Tween.Delay(deathDelay, DieFinish);
        }
    }

    private void DieFinish()
    {
        onMissionFail?.Invoke();
        onStopLevel?.Invoke();
    }
}
