using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(AnimationEventStateBehaviour))]
public class AnimationEventStateBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // draw all normal fields first
        AnimationEventUtils.DrawBaseInspector((BaseAnimationEventBehaviour)target);
    }
}
#endif