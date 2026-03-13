using UnityEngine;

public class WeaponSpread
{
    private AnimationCurve spreadCurve;
    private float baseSpread;
    private float halfSpread;
    private float maxSpread;
    private float fireRateRPM;

    public float CurrentSpreadPosition { get; private set; } = 0f;
    public float CurrentSpreadAmount { get; private set; } = 0f;
    public bool IsAiming { get; set; } = false;
    private float lastShotTime = 0f;

    public WeaponSpread(float baseSpread, float halfSpread, float maxSpread, float magSize, float fireRateRPM)
    {
        this.baseSpread = baseSpread;
        this.halfSpread = halfSpread;
        this.maxSpread = maxSpread;
        this.fireRateRPM = fireRateRPM;

        CreateSpreadCurve(baseSpread, halfSpread, maxSpread);
    }

    private void CreateSpreadCurve(float baseSpread, float halfSpread, float maxSpread)
    {
        spreadCurve = new AnimationCurve();

        // key 0: BaseSpread at start
        spreadCurve.AddKey(0f, baseSpread);

        // key 1: HalfSpread at middle
        spreadCurve.AddKey(0.5f, halfSpread);

        // key 2: MaxSpread at end
        spreadCurve.AddKey(1f, maxSpread);
    }

    public void OnShotFired(float magSize)
    {
        // increase spread based on shot
        CurrentSpreadPosition += 1f / magSize; // x axis is shots fired
        CurrentSpreadPosition = Mathf.Clamp01(CurrentSpreadPosition);

        lastShotTime = Time.time;

        CalculateSpread();
    }

    public void UpdateSpreadOverTime()
    {
        float minSpread = IsAiming ? baseSpread : 0.5f;
    
        float timeSinceLastShot = Time.time - lastShotTime;
        float maxFireInterval = 60f / fireRateRPM;

        if (CurrentSpreadPosition > minSpread)
        {
            // scale recovery based on how much slower than max fire rate
            float recoveryScale = Mathf.Clamp01((timeSinceLastShot - maxFireInterval) / maxFireInterval);
            float recoverySpeed = (maxSpread - baseSpread) * recoveryScale;

            CurrentSpreadPosition -= recoverySpeed * Time.deltaTime;

            if (CurrentSpreadPosition < minSpread)
            {
                CurrentSpreadPosition = minSpread;
            }
        }
        else if (CurrentSpreadPosition < minSpread)
        {
            CurrentSpreadPosition = minSpread;
        }
        CalculateSpread();
    }

    public void ResetSpread()
    {
        if (IsAiming)
        {
            CurrentSpreadPosition = baseSpread;
        }
        else
        {
            CurrentSpreadPosition = halfSpread;
        }
        CalculateSpread();
    }

    private void CalculateSpread()
    {
        CurrentSpreadAmount = spreadCurve.Evaluate(CurrentSpreadPosition);
    }
}
