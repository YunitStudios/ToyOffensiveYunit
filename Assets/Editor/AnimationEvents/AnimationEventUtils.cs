#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEditor;
using UnityEditor.Animations;

// credit to https://github.com/adammyhre/Improved-Unity-Animation-Events/tree/master for the original system

public static class AnimationEventUtils
{
    [Header("Preview Settings")]
    private static Motion previewClip;             // clip used for previewing
    private static float previewTime;              // current preview time
    private static bool isPreviewing;              // flag to track preview state

    [Header("Root Motion Tracking")]
    private static Vector3 originalPosition;       // original world position of target
    private static Quaternion originalRotation;    // original world rotation of target

    private static PlayableGraph playableGraph;    // graph used for BlendTree previews
    private static AnimationMixerPlayable mixer;   // mixer for BlendTree playback

    // track which behaviour is currently driving the preview
    private static BaseAnimationEventBehaviour activePreviewBehaviour;
    
    public static void DrawBaseInspector(BaseAnimationEventBehaviour stateBehaviour)
    {
        // draw default inspector fields
        EditorGUILayout.LabelField("Base Animation Event Inspector", EditorStyles.boldLabel);
    
        if (Validate(stateBehaviour, out string errorMessage))
        {
            GUILayout.Space(10);

            // preview buttons
            if (isPreviewing && activePreviewBehaviour == stateBehaviour)
            {
                if (GUILayout.Button("Stop Preview"))
                {
                    StopPreview();
                }
                else
                {
                    PreviewAnimationClip(stateBehaviour);
                }
            }
            else
            {
                if (GUILayout.Button("Preview"))
                {
                    StartPreview(stateBehaviour);
                    PreviewAnimationClip(stateBehaviour);
                }
            }

            GUILayout.Label($"Previewing at {previewTime:F2}s", EditorStyles.helpBox);
        }
        else
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Info);
        }
    }

    private static void StartPreview(BaseAnimationEventBehaviour stateBehaviour)
    {
        if (activePreviewBehaviour != null && activePreviewBehaviour != stateBehaviour)
        {
            StopPreview();
        }

        activePreviewBehaviour = stateBehaviour;
        SaveOriginalTransform();
        isPreviewing = true;
        AnimationMode.StartAnimationMode();
    }

    private static void StopPreview()
    {
        RestoreOriginalTransform();
        EnforceTPose();
        isPreviewing = false;

        if (playableGraph.IsValid())
            playableGraph.Destroy();

        activePreviewBehaviour = null;
        AnimationMode.StopAnimationMode();
    }

    // saves the current world transform
    private static void SaveOriginalTransform()
    {
        GameObject target = Selection.activeGameObject;
        if (!target) return;

        originalPosition = target.transform.position;
        originalRotation = target.transform.rotation;
    }

    // restores the previously saved world transform
    private static void RestoreOriginalTransform()
    {
        GameObject target = Selection.activeGameObject;
        if (!target) return;

        target.transform.position = originalPosition;
        target.transform.rotation = originalRotation;
        SceneView.RepaintAll();
    }

    // ---------------------------------------------------------------------------------------------------
    // PREVIEW LOGIC
    // ---------------------------------------------------------------------------------------------------
    private static void PreviewAnimationClip(BaseAnimationEventBehaviour stateBehaviour)
    {
        AnimatorController animatorController = GetValidAnimatorController(out _);
        if (!animatorController) return;

        GameObject targetGameObject = Selection.activeGameObject;
        if (!targetGameObject) return;

        // find the Animator state matching this behaviour
        var matchingState = animatorController.layers
            .Select(layer => FindMatchingState(layer.stateMachine, stateBehaviour))
            .FirstOrDefault(state => state.state);

        if (!matchingState.state) return;

        Motion motion = matchingState.state.motion;

        if (motion is BlendTree blendTree)
        {
            SampleBlendTreeAnimation(stateBehaviour, stateBehaviour.triggerTime);
            return;
        }

        if (motion is AnimationClip clip)
        {
            previewTime = stateBehaviour.triggerTime * clip.length;

            // sample clip and apply root motion
            AnimationMode.SampleAnimationClip(targetGameObject, clip, previewTime);
            ApplyRootMotion(targetGameObject, clip, previewTime);
        }
    }

    // applies root motion delta from clip
    private static void ApplyRootMotion(GameObject target, AnimationClip clip, float timeInSeconds)
    {
        string path = "";
        Type type = typeof(Transform);

        // initial local transform
        Vector3 rootPos0 = new Vector3(
            GetCurveValue(clip, path, type, "RootT.x", 0),
            GetCurveValue(clip, path, type, "RootT.y", 0),
            GetCurveValue(clip, path, type, "RootT.z", 0)
        );

        Quaternion rootRot0 = GetRootRotationAtTime(clip, path, type, 0);

        // local transform at preview time
        Vector3 rootPosN = new Vector3(
            GetCurveValue(clip, path, type, "RootT.x", timeInSeconds),
            GetCurveValue(clip, path, type, "RootT.y", timeInSeconds),
            GetCurveValue(clip, path, type, "RootT.z", timeInSeconds)
        );

        Quaternion rootRotN = GetRootRotationAtTime(clip, path, type, timeInSeconds);

        // apply delta in world space
        target.transform.position = originalPosition + (originalRotation * (rootPosN - rootPos0));
        target.transform.rotation = originalRotation * (rootRotN * Quaternion.Inverse(rootRot0));
    }

    // safely retrieves curve value
    private static float GetCurveValue(AnimationClip clip, string path, Type type, string propertyName, float time)
    {
        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, path, type, propertyName);
        return curve?.Evaluate(time) ?? 0f;
    }

    // safely gets root rotation
    private static Quaternion GetRootRotationAtTime(AnimationClip clip, string path, Type type, float time)
    {
        float qx = GetCurveValue(clip, path, type, "RootQ.x", time);
        float qy = GetCurveValue(clip, path, type, "RootQ.y", time);
        float qz = GetCurveValue(clip, path, type, "RootQ.z", time);
        float qw = GetCurveValue(clip, path, type, "RootQ.w", time);

        if (Mathf.Approximately(qx, 0f) && Mathf.Approximately(qy, 0f) &&
            Mathf.Approximately(qz, 0f) && Mathf.Approximately(qw, 0f))
            return Quaternion.identity;

        return new Quaternion(qx, qy, qz, qw);
    }

    // ---------------------------------------------------------------------------------------------------
    // BLEND TREE LOGIC
    // ---------------------------------------------------------------------------------------------------
    private static void SampleBlendTreeAnimation(BaseAnimationEventBehaviour stateBehaviour, float normalizedTime)
    {
        Animator animator = Selection.activeGameObject.GetComponent<Animator>();

        if (playableGraph.IsValid())
            playableGraph.Destroy();

        playableGraph = PlayableGraph.Create("BlendTreePreviewGraph");
        mixer = AnimationMixerPlayable.Create(playableGraph, 1, true);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        output.SetSourcePlayable(mixer);

        AnimatorController animatorController = GetValidAnimatorController(out _);
        if (!animatorController) return;

        var matchingState = animatorController.layers
            .Select(layer => FindMatchingState(layer.stateMachine, stateBehaviour))
            .FirstOrDefault(state => state.state);

        if (matchingState.state.motion is not BlendTree blendTree) return;

        // simplified 1D blending for editor preview
        AnimationClipPlayable[] clipPlayables = new AnimationClipPlayable[blendTree.children.Length];
        for (int i = 0; i < blendTree.children.Length; i++)
        {
            clipPlayables[i] = AnimationClipPlayable.Create(playableGraph, blendTree.children[i].motion as AnimationClip);
        }
    }

    // ---------------------------------------------------------------------------------------------------
    // VALIDATION / UTILITY
    // ---------------------------------------------------------------------------------------------------
    private static ChildAnimatorState FindMatchingState(AnimatorStateMachine stateMachine, BaseAnimationEventBehaviour stateBehaviour)
    {
        foreach (ChildAnimatorState state in stateMachine.states)
            if (state.state.behaviours.Contains(stateBehaviour))
                return state;

        foreach (ChildAnimatorStateMachine sub in stateMachine.stateMachines)
        {
            var match = FindMatchingState(sub.stateMachine, stateBehaviour);
            if (match.state)
                return match;
        }

        return default;
    }

    private static bool Validate(BaseAnimationEventBehaviour stateBehaviour, out string errorMessage)
    {
        AnimatorController animatorController = GetValidAnimatorController(out errorMessage);
        if (!animatorController) return false;

        var matchingState = animatorController.layers
            .Select(layer => FindMatchingState(layer.stateMachine, stateBehaviour))
            .FirstOrDefault(state => state.state);

        if (!matchingState.state)
        {
            errorMessage = "No matching state found.";
            return false;
        }

        previewClip = GetAnimationClipFromMotion(matchingState.state.motion);
        if (!previewClip)
        {
            errorMessage = "No valid AnimationClip found for the current state.";
            return false;
        }

        return true;
    }

    private static AnimationClip GetAnimationClipFromMotion(Motion motion)
    {
        if (motion is AnimationClip clip) return clip;
        if (motion is BlendTree blendTree)
            return blendTree.children.Select(c => GetAnimationClipFromMotion(c.motion)).FirstOrDefault(c => c != null);
        return null;
    }

    private static AnimatorController GetValidAnimatorController(out string errorMessage)
    {
        errorMessage = string.Empty;

        GameObject targetGameObject = Selection.activeGameObject;
        if (!targetGameObject)
        {
            errorMessage = "Select a GameObject with an Animator.";
            return null;
        }

        Animator animator = targetGameObject.GetComponent<Animator>();
        if (!animator)
        {
            errorMessage = "Selected GameObject does not have an Animator component.";
            return null;
        }

        AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
        if (!controller)
        {
            errorMessage = "Selected Animator does not have a valid AnimatorController.";
            return null;
        }

        return controller;
    }

    [MenuItem("GameObject/Enforce T-Pose", false, 0)]
    private static void EnforceTPose()
    {
        GameObject selected = Selection.activeGameObject;
        if (!selected || !selected.TryGetComponent(out Animator animator) || !animator.avatar) return;

        SkeletonBone[] skeletonBones = animator.avatar.humanDescription.skeleton;

        foreach (HumanBodyBones hbb in Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (hbb == HumanBodyBones.LastBone) continue;

            Transform bone = animator.GetBoneTransform(hbb);
            if (!bone) continue;

            SkeletonBone skeletonBone = skeletonBones.FirstOrDefault(sb => sb.name == bone.name);
            if (skeletonBone.name == null) continue;

            if (hbb == HumanBodyBones.Hips) bone.localPosition = skeletonBone.position;
            bone.localRotation = skeletonBone.rotation;
        }
    }
}
#endif
