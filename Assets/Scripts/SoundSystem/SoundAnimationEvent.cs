using SoundSystem;
using UnityEditor;
using UnityEngine;

public class SoundAnimationEvent : BaseAnimationEventBehaviour
{
    public SoundDataSO soundData;
    private WwisePlayer soundPlayer;
    private AkGameObj akGameObj;
    private GameObject selectedObject;

    protected override void OnEnter(Animator animator)
    {
        soundPlayer = animator.gameObject.GetComponent<WwisePlayer>();
        if (soundPlayer == null)
            soundPlayer = animator.gameObject.AddComponent<WwisePlayer>();
    }

    protected override void Trigger(Animator animator)
    {
        if (soundPlayer != null && soundData != null)
        {
            soundPlayer.PlaySound(soundData);
        }

        if (receiver != null)
        {
            receiver.OnAnimationEventTriggered(soundData != null ? soundData.name : "UnnamedSound");
        }
    }

#if UNITY_EDITOR
    public void DeleteComponents()
    {
        if (soundPlayer != null)
        {
            Object.DestroyImmediate(soundPlayer);
            soundPlayer = null;
        }

        if (akGameObj != null)
        {
            Object.DestroyImmediate(akGameObj);
            akGameObj = null;
        }

        receiver = null;
    }

    public void ApplySound()
    {
        selectedObject = Selection.activeGameObject;
        if (selectedObject == null) return;

        // WwisePlayer
        soundPlayer = selectedObject.GetComponent<WwisePlayer>();
        if (soundPlayer == null)
            soundPlayer = selectedObject.AddComponent<WwisePlayer>();

        // AkGameObj
        akGameObj = selectedObject.GetComponent<AkGameObj>();
        if (akGameObj == null)
            akGameObj = selectedObject.AddComponent<AkGameObj>();

        // disable environment aware
        akGameObj.isEnvironmentAware = false;
    }
#endif
}