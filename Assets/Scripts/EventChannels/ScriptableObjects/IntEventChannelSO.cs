using EditorAttributes;
using UnityEngine;
[CreateAssetMenu(menuName = "Events/Int Event Channel", fileName = "IntEventChannel")]
public class IntEventChannelSO : GenericEventChannelSO<int>
{

    [Button]
    private void DebugInvoke(int value)
    {
        Invoke(value);
    }
}
