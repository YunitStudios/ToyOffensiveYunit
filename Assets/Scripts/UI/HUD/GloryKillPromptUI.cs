using UnityEngine;

public class GloryKillPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject gloryKillPrompt;

    void Awake()
    {
        gloryKillPrompt.SetActive(false);
    }
    
    public void SetGloryKillPrompt(bool active)
    {
        gloryKillPrompt.SetActive(active);
    }
}
