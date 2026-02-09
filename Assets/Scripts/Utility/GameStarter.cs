using UnityEngine;

public class GameStarter : MonoBehaviour
{
    // Starts the actual game after loading a game scene
    void Start()
    {
        if(!GameManager.Instance.Ingame)
            GameManager.Instance.StartGame();
        
    }
    
}
