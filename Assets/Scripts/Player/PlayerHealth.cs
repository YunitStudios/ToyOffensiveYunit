using System;
using UnityEngine;

public class PlayerHealth : Health, IDamageable
{

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
}
