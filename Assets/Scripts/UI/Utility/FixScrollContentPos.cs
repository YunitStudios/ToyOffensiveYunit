using System;
using UnityEngine;

public class FixScrollContentPos : MonoBehaviour
{
    // Unity moment
    // https://discussions.unity.com/t/vertical-scrollbar-wrong-initial-position-value-if-canvas-was-disabled-before-play/893734/3
    private void Awake()
    {
        // Set rect transform Y pos to 0
        if(transform is RectTransform rect)
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
    }
}
