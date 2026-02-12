using EditorAttributes;
using UnityEngine;
[CreateAssetMenu(menuName = "Events/Float Event Channel", fileName = "FloatEventChannel")]
public class FloatEventChannelSO : GenericEventChannelSO<float>
{

    [Button]
    private void DebugInvoke(float value)
    {
        Invoke(value);
    }
}
