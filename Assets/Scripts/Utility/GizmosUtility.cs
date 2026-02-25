using UnityEngine;

// https://discussions.unity.com/t/draw-2d-circle-with-gizmos/123578/5

public static class GizmosUtility
{
    private const float GIZMO_DISK_THICKNESS = 0.01f;

    public static void DrawGizmoDisk(this Transform t, float radius)
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, new Vector3(1, GIZMO_DISK_THICKNESS, 1));
        Gizmos.DrawSphere(Vector3.zero, radius);
        Gizmos.matrix = oldMatrix;
    }
}
