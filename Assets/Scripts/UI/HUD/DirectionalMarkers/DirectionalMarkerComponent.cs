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

    private void OnEnable()
    {
        if(onMissionCompleted != null)
            onMissionCompleted.OnEventRaised += ShowImage;
    }

    private void OnDisable()
    {
        if(onMissionCompleted != null)
            onMissionCompleted.OnEventRaised -= ShowImage;
    }

    void Start()
    {
        if (!showAlways)
            imageObject.SetActive(false);
        if(markerType == MarkerTypes.Target)
            RadialHUDDisplay.SpawnLiveMarker?.Invoke(transform, markerType);
        else
        {
            RadialHUDDisplay.SpawnMarker?.Invoke(transform.position, markerType);
        }
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
