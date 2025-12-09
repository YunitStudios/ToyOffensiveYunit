using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    public CinemachineBrain CinemachineBrain => cinemachineBrain;
    [SerializeField] private SerializedDictionary<CameraType, CinemachineCamera> cameras = new();
    
    [Header("Attributes")]
    [SerializeField] private CameraType defaultCamera = CameraType.Main;
    [SerializeField] private float fovResetDuration = 1f;
    [SerializeField] private float fovSetDuration = 0.5f;

    private CinemachineCamera currentCamera;
    [HideInInspector] public CameraType CurrentCameraType;
    
    public Transform CameraTransform => mainCamera.transform;

    private float currentBaseFov;
    private Tween resetFovTween;
    private Tween setFovTween;
    
    public float CurrentFovMultiplier
    {
        set => SetFov(currentCamera, currentBaseFov * value, fovSetDuration);
    }

    private void Start()
    {
        foreach (CinemachineCamera cam in cameras.Values)
            cam.Priority = 0;
        
        print("start");

        ResetCamera();
    }

    public void ResetCamera()
    {
        ChangeCamera(defaultCamera);
    }

    public void ChangeCamera(CameraType cameraType)
    {
        // If this camera type isn't set
        if (!cameras.TryGetValue(cameraType, out CinemachineCamera newCamera)) return;
        
        // Make sure not changing to current camera
        if (currentCamera && cameras.TryGetValue(cameraType, out CinemachineCamera cam) && currentCamera == cam)
            return;
        
        // If the current camera has orbital follow
        if (currentCamera)
        {
            var orbitalFollow = currentCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow)
                orbitalFollow.HorizontalAxis.TriggerRecentering();
        }
        
        // Switch priorities
        if (currentCamera)
            currentCamera.Priority = 0;
        newCamera.Priority = 1;

        if (currentCamera)
            ResetFov(currentCamera, currentBaseFov, fovResetDuration);
        
        currentCamera = newCamera;
        currentBaseFov = currentCamera.Lens.FieldOfView;
        CurrentCameraType = cameraType;
    }

    private void ResetFov(CinemachineCamera cam, float targetFov, float duration)
    {
        resetFovTween.Complete();
        TweenFov(resetFovTween, cam, targetFov, duration);
    }
    private void SetFov(CinemachineCamera cam, float targetFov, float duration)
    {
        TweenFov(setFovTween, cam, targetFov, duration);
    }
    
    private void TweenFov(Tween targetTween, CinemachineCamera cam, float targetFov, float duration)
    {
        targetTween = Tween.Custom(cam.Lens.FieldOfView, targetFov, duration, 
            v =>
            {
                var lens = cam.Lens;
                lens.FieldOfView = v;
                cam.Lens = lens;
            });
        
    }
    
    public enum CameraType
    {
        Main,
        Aim,
        AimScope,
        Climbing,
        Parachute
    }
}
