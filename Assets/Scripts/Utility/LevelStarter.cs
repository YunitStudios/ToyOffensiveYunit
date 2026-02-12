using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    // Starts the actual game after loading a game scene
    void Start()
    {
        if(!GameManager.Instance.Ingame)
            GameManager.Instance.StartLevel();
        
    }
    
}
