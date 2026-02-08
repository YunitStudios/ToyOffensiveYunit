using System;
using System.Collections.Generic;
using UnityEngine;

// Math provided by https://easings.net/
public abstract class Easing
{
    public delegate float EaseTypeDelegate(float t);

    public enum EaseType
    {
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InCirc,
        OutCirc,
        InOutCirc,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce
    }

    // Creates mapping of EaseType enum and easing function
    // This allows for easy customisation when choosing which easing type to use
    private static readonly Dictionary<EaseType, EaseTypeDelegate> EaseTypeDictionary = new()
    {
        {EaseType.InSine, easeInSine},
        {EaseType.OutSine, easeOutSine},
        {EaseType.InOutSine, easeInOutSine},
        {EaseType.InQuad, easeInQuad},
        {EaseType.OutQuad, easeOutQuad},
        {EaseType.InOutQuad, easeInOutQuad},
        {EaseType.InCubic, easeInCubic},
        {EaseType.OutCubic, easeOutCubic},
        {EaseType.InOutCubic, easeInOutCubic},
        {EaseType.InQuart, easeInQuart},
        {EaseType.OutQuart, easeOutQuart},
        {EaseType.InOutQuart, easeInOutQuart},
        {EaseType.InCirc, easeInCirc},
        {EaseType.OutCirc, easeOutCirc},
        {EaseType.InOutCirc, easeInOutCirc},
        {EaseType.InBack, easeInBack},
        {EaseType.OutBack, easeOutBack},
        {EaseType.InOutBack, easeInOutBack},
        {EaseType.InBounce, easeInBounce},
        {EaseType.OutBounce, easeOutBounce},
        {EaseType.InOutBounce, easeInOutBounce}
    };

    // Abstracts the dictionary by only needing user to call this function to find the function
    public static EaseTypeDelegate FindEaseType(EaseType key)
    {
        return EaseTypeDictionary[key];
    }
    
    public static float easeInSine(float t)
    {
        return 1 - MathF.Cos((t * MathF.PI) / 2);
    }

    public static float easeOutSine(float t)
    {
        return MathF.Sin((t * MathF.PI) / 2);
    }
    
    public static float easeInOutSine(float t)
    {
        return -(MathF.Cos(MathF.PI * t) - 1) / 2;
    }
    
    public static float easeInQuad(float t)
    {
        return t * t;
    }

    public static float easeOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }
    
    public static float easeInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : 1 - MathF.Pow(-2 * t + 2, 2) / 2;
    }
    
    public static float easeInCubic(float t)
    {
        return t * t * t;
    }

    public static float easeOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
    
    public static float easeInOutCubic(float t)
    {
        return t < 0.5f ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;
    }
    
    public static float easeInQuart(float t)
    {
        return t * t * t * t;
    }

    public static float easeOutQuart(float t)
    {
        return 1 - Mathf.Pow(1 - t, 4);
    }
    
    public static float easeInOutQuart(float t)
    {
        return t < 0.5f ? 8 * t * t * t * t : 1 - MathF.Pow(-2 * t + 2, 4) / 2;
    }
    
    public static float easeInCirc(float t)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
    }

    public static float easeOutCirc(float t)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
    }
    
    public static float easeInOutCirc(float t)
    {
        return t < 0.5f
            ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2
            : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;
    }

    public static float easeInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return c3 * t * t * t - c1 * t * t;
    }

    public static float easeOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(t - 1, 3) + c1 * MathF.Pow(t - 1, 2);
    }
    
    public static float easeInOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        return t < 0.5f
            ? (MathF.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
            : (MathF.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }
    
    public static float easeInBounce(float t)
    {
        return 1 - easeOutBounce(1 - t);
    }

    public static float easeOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        return t switch
        {
            < 1.0f / d1 => n1 * t * t,
            < 2.0f / d1 => n1 * (t -= 1.5f / d1) * t + 0.75f,
            < 2.5f / d1 => n1 * (t -= 2.25f / d1) * t + 0.9375f,
            _ => n1 * (t -= 2.625f / d1) * t + 0.984375f
        };
    }
    
    public static float easeInOutBounce(float t)
    {
        return t < 0.5f
            ? (1 - easeOutBounce(1 - 2 * t)) / 2
            : (1 + easeOutBounce(2 * t - 1)) / 2;
    }
}
