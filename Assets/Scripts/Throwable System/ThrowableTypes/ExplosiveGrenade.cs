using UnityEngine;

namespace Throwable_System.ThrowableTypes
{
    public class ExplosiveGrenade : ThrowableTemplate
    {
        protected override void OnDetonate()
        {
            Debug.Log($"Detonating explosive grenade with damage of {Damage}");

            // play animations effects sounds etc (the type already does the damage for you (if its a flash it does 0)
            var particle = Instantiate(DetonateParticles, transform.position, Quaternion.identity);
            particle.Play();
            particle.transform.parent = null;
            
            wwisePlayer.PlaySound(detonationSound);
            
            // destroy the object
            Destroy(gameObject);
        }
    }
}