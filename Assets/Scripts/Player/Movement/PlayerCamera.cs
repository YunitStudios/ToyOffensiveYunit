using System;
using System.Collections.Generic;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    public CinemachineBrain CinemachineBrain => cinemachineBrain;
    [SerializeField] private AYellowpaper.SerializedCollections.SerializedDictionary<CameraType, CameraData> cameras = new();
    [SerializeField] private GameSettings playerSettings;
    [SerializeField] private VolumeProfile volumeSettings;
    
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

    private void OnEnable()
    {
        SettingsManager.OnSettingsChanged += ApplySettings;
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsChanged -= ApplySettings;
    }

    private void ApplySettings()
    {
        SetFov();

        volumeSettings.TryGet<MotionBlur>(out var motBlur);
        if (motBlur)
        {
            motBlur.active = playerSettings.motionBlur;
        }
        volumeSettings.TryGet<LiftGammaGain>(out var liftGammaGain);
        if (liftGammaGain)
        {
            liftGammaGain.gamma.value = new Vector4(1,1,1,1-SettingsManager.Instance.GetBrightnessValue);
        }

    }

    private void SetFov()
    {
        
        currentBaseFov = playerSettings.fov + cameras[CurrentCameraType].fovOffset;
        if(currentCamera)
            currentCamera.Lens.FieldOfView = currentBaseFov;
    }

    private void Start()
    {
        foreach (var camData in cameras.Values)
            camData.camera.Priority = 0;
        
        ResetCamera();
    }

    public void ResetCamera()
    {
        ChangeCamera(defaultCamera);
    }

    public void ChangeCamera(CameraType cameraType)
    {
        // If this camera type isn't set
        if (!cameras.TryGetValue(cameraType, out var newCamData)) return;
        
        // Make sure not changing to current camera
        if (currentCamera && cameras.TryGetValue(cameraType, out var checkCam) && currentCamera == checkCam.camera)
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
        newCamData.camera.Priority = 1;

        if (currentCamera)
            ResetFov(currentCamera, currentBaseFov, fovResetDuration);
        
        currentCamera = newCamData.camera;
        CurrentCameraType = cameraType;
        SetFov();
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


    [Serializable]
    public struct CameraData
    {
        public CinemachineCamera camera;
        public int fovOffset;
    }
}
