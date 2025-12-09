using UnityEngine;

public class ThrowableTemplate : MonoBehaviour
{
    public float FuseTime;
    public bool IsImpactFuse;
    
    public float Damage;
    public float Radius;
    
    private bool detonated;

    private void Update()
    {
        if (!IsImpactFuse && !detonated)
        {
            FuseTime -= Time.deltaTime;

            if (FuseTime <= 0)
            {
                detonated = true;
                if(Damage > 0)
                    DoDamage();
                OnDetonate();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsImpactFuse)
        {
            detonated = true;
            if(Damage > 0)
                DoDamage();
            OnDetonate();
        }
    }

    private void DoDamage()
    {
        // get all things colliding with us
        Collider[] hits = Physics.OverlapSphere(transform.position, Radius);
        
        foreach (Collider hit in hits)
        {
            // if tag is enemy or player do damage
            if (hit.CompareTag("Enemy"))
                hit.GetComponent<AIController>().TakeDamage(Damage);
            // if(hit.collider.CompareTag("Player"))
            //     hit.collider.GetComponent<PlayerHealth>().TakeDamage(Damage);
        }
    }
    
    protected virtual void OnDetonate()
    {
        
    }
}
