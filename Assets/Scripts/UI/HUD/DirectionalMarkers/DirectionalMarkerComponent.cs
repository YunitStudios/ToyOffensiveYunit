using System;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalMarkerComponent : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private MarkerTypes markerType;
    [SerializeField] private bool hasWorldSprite = false;
    
    [Header("References")]
    [SerializeField] private GameObject imageObject;
    void Start()
    {
        if(markerType == MarkerTypes.Target)
            RadialHUDDisplay.SpawnLiveMarker?.Invoke(transform, markerType);
        else
        {
            RadialHUDDisplay.SpawnMarker?.Invoke(transform.position, markerType);
        }
    }

    private void Update()
    {
        if (hasWorldSprite)
        {
            imageObject.transform.forward = Camera.main.transform.forward;
        }
    }
}
