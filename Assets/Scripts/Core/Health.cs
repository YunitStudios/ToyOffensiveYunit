using System;
using EditorAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Health : MonoBehaviour, IObjectiveTarget
{
    [Header("Attributes")]
    [SerializeField] private float maxHealth;
    public float MaxHealth => maxHealth;
    [SerializeField] private bool shouldRegen;
    [Tooltip("Health regenerated per second")]
    [SerializeField, ShowField(nameof(shouldRegen))] private float regenRate;
    [Tooltip("Delay before you can regenerate health after taking damage")]
    [SerializeField, ShowField(nameof(shouldRegen))] private float regenMaxWait;

    [FormerlySerializedAs("onTakeDamage")]
    [Header("Input Events")] 
    [SerializeField] private FloatEventChannelSO onHealthAdjust;
    
    [Header("Output Events")] 
    [Tooltip("Invoked if the damage dealing is successful")]
    [SerializeField] private VoidEventChannelSO onDamageTaken;
    [Tooltip("Invoked if the health changes, is successful and passes new health value")]
    [SerializeField] private FloatEventChannelSO onHealthChanged;
    [Tooltip("Invoked if the health changes, passing the change in health rather than the total value")]
    [SerializeField] private FloatEventChannelSO onHealthChangedDifference;
    [SerializeField] private VoidEventChannelSO onDie;
    public UnityEvent OnDieUnity;

    [Header("Debug")] 
    [SerializeField] private float debugDamage;

    private float currentHealth;
    public float CurrentHealth
    {
        get => currentHealth;
        protected set
        {
            onHealthChangedDifference?.Invoke(value - currentHealth);
            currentHealth = value;
            HealthChanged();
            onHealthChanged?.Invoke(CurrentHealth);
        }
    }
    public bool IsInvulnerable { get; set; }
    protected bool isDead = false;
    public event Action OnTargetComplete;

    private float regenWait;
    
    public bool IsAlive => CurrentHealth > 0;
    

    private void OnEnable()
    {
        if(onHealthAdjust)
            onHealthAdjust.OnEventRaised += AdjustHealth;
    }

    private void OnDisable()
    {
        if(onHealthAdjust)
            onHealthAdjust.OnEventRaised -= AdjustHealth;
    }

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        
    }

    private void Update()
    {
        if(shouldRegen)
        {
            regenWait -= Time.deltaTime;
            if (regenWait <= 0 && CurrentHealth < maxHealth)
                RegenerateHealth();
        }
    }

    public void DealDamage(float damage)
    {
        DealDamage(damage, out bool _);
    }
    public void DealDamage(float damage, out bool didDie)
    {
        AdjustHealth(damage);

        didDie = CurrentHealth <= 0;
    }

    private void AdjustHealth(float damage)
    {
        if (IsInvulnerable)
            return;
        
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, maxHealth);
        
        // If taken damage and not dealed
        if(damage > 0)
        {
            onDamageTaken?.Invoke();

            regenWait = regenMaxWait;

            if (CurrentHealth <= 0)
                Die();
        }
    }

    protected virtual void Die()
    {
        if (!isDead)
        {
            onDie?.Invoke();
            OnDieUnity?.Invoke();
            
            isDead = true;
            
            OnTargetComplete?.Invoke();
        }
    }

    private void RegenerateHealth()
    {
        float regen = Time.deltaTime * regenRate;
        CurrentHealth += regen;
    }

    protected virtual void HealthChanged()
    {
        
    }


    [Button]
    private void DebugDealDamage()
    {
        AdjustHealth(debugDamage);
    }

    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
    }

}
