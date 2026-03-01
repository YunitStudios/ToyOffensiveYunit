using TMPro;
using UnityEngine;

public class ReloadPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI reloadText;
    private bool visible = false;

    public void ShowReloadPrompt()
    {
        reloadText.text = "Reload";
        visible = true;
    }

    public void ShowReloading()
    {
        reloadText.text = "Reloading...";
        visible = true;
    }

    public void Hide()
    {
        visible = false;
    }

    void Update()
    {
        if (visible)
        {
            reloadText.gameObject.SetActive(true);
        }
        else
        {
            reloadText.gameObject.SetActive(false);
        }
    }
}
