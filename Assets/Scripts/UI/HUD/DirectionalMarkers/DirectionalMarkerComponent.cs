using System;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalMarkerComponent : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private MarkerTypes markerType;
    [SerializeField] private bool hasWorldSprite = false;
    [SerializeField] private bool showAlways = true;
    
    [Header("References")]
    [SerializeField] private GameObject imageObject;

    [SerializeField] private VoidEventChannelSO onMissionCompleted;
    [SerializeField] private VoidEventChannelSO onLoadRadialHUD;

    private void OnEnable()
    {
        if(onMissionCompleted != null)
            onMissionCompleted.OnEventRaised += ShowImage;
        if (onLoadRadialHUD != null)
            onLoadRadialHUD.OnEventRaised += SpawnMarker;
    }

    private void OnDisable()
    {
        if(onMissionCompleted != null)
            onMissionCompleted.OnEventRaised -= ShowImage;
        if (onLoadRadialHUD != null)
            onLoadRadialHUD.OnEventRaised -= SpawnMarker;
    }

    private void Start()
    {
        if (!showAlways)
            imageObject.SetActive(false);
        if (markerType == MarkerTypes.Target)
            RadialHUDDisplay.SpawnLiveMarker?.Invoke(transform, markerType);
    }

    void SpawnMarker()
    {
        if (markerType != MarkerTypes.Target)
            RadialHUDDisplay.SpawnMarker?.Invoke(transform.position, markerType);
    }

    private void ShowImage()
    {
        if(!showAlways)
            imageObject.SetActive(true);
    }

    private void Update()
    {
        if (hasWorldSprite)
        {
            imageObject.transform.forward = Camera.main.transform.forward;
        }
    }
}
