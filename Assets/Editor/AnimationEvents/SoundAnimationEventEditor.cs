#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using SoundSystem;

[CustomEditor(typeof(SoundAnimationEvent))]
public class SoundAnimationEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // draw all normal fields first
        AnimationEventUtils.DrawBaseInspector((BaseAnimationEventBehaviour)target);

        SoundAnimationEvent script = (SoundAnimationEvent)target;
        GameObject selectedObject = Selection.activeGameObject;

        GUILayout.Space(5);

        // show status of applied sound
        string status = "No sound applied.";
        if (selectedObject != null)
        {
            WwisePlayer player = selectedObject.GetComponent<WwisePlayer>();
            if (player != null)
            {
                status = $"Sound applied to object: {selectedObject.name}";
            }
        }
        EditorGUILayout.HelpBox(status, MessageType.Info);

        // Apply Sound Button
        if (GUILayout.Button("Apply Sound to Selected Object"))
        {
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog(
                    "No Object Selected",
                    "Select a GameObject with an Animator in the hierarchy before applying the sound",
                    "OK"
                );
            }
            else
            {
                script.ApplySound();
                // force the inspector to repaint so the HelpBox updates
                EditorUtility.SetDirty(script);
                Repaint(); // repaint the inspector
            }
        }

        // delete sound components button
        if (GUILayout.Button("Delete added Components"))
        {
            if (selectedObject != null)
            {
                script.DeleteComponents();
            }
        }
    }
}
#endif