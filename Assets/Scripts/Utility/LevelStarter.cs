using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO pauseEvent;
    // Starts the actual game after loading a game scene
    void Start()
    {
        if(!GameManager.Instance.Ingame)
        {
            GameManager.Instance.StartLevel();
            pauseEvent?.Invoke();
        }        
    }
    
}
