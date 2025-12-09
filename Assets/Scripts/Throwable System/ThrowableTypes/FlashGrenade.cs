using UnityEngine;

namespace Throwable_System.ThrowableTypes
{
    public class FlashGrenade : ThrowableTemplate
    {
        protected override void OnDetonate()
        {
            Debug.Log($"Detonating flash grenade with damage of {Damage}");
            
            // play animations effects sounds etc (the type already does the damage for you (if its a flash it does 0)
            
            // destroy the object
            Destroy(gameObject);
        }
    }
}