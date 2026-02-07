using UnityEngine;

public class PlayerHealth : Health
{
    protected override void HealthChanged()
    {
        GameManager.PlayerData.SetCurrentHealth(CurrentHealth);
    }
}
