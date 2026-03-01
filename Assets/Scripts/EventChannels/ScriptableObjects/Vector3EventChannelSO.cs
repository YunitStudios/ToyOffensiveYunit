using EditorAttributes;
using UnityEngine;
[CreateAssetMenu(menuName = "Events/Vector3 Event Channel", fileName = "Vector3EventChannel")]
public class Vector3EventChannelSO : GenericEventChannelSO<Vector3>
{

    [Button]
    private void DebugInvoke(Vector3 value)
    {
        Invoke(value);
    }
}
