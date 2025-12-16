using UnityEngine;

public interface IDamageable
{

    public void TakeDamage(IDamageSource source, float damage);
}
