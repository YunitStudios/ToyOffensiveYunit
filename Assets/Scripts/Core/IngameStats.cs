using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/IngameStats", fileName = "IngameStats")]
public class IngameStats : ScriptableObject
{
    [Title("Stats")]
    [field: SerializeField, DisableInPlayMode] public float ElapsedTime { get; private set; }
    [Space,Space,Space]
    [Title("Event Binding")]
    [SerializeField] private FloatEventChannelSO onTimePassed;

    public float Accuracy;
    
    private void RevertToDefaultValues()
    {
        ElapsedTime = 0f;
    }

    public void Start()
    {
        RevertToDefaultValues();
        onTimePassed.OnEventRaised += TimePassed;
    }

    public void Stop()
    {
        onTimePassed.OnEventRaised -= TimePassed;
    }
    
    private void TimePassed(float val)
    {
        ElapsedTime += val;
    }
    
}
