using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;

public class EntryPointUI : MonoBehaviour
{
    private MissionSetupUI mainScript;
    public void SetMainScript(MissionSetupUI mainScript)
    {
        this.mainScript = mainScript;
    }

    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasFader canvasFader;

    public void OnSelectEntry()
    {
        mainScript.SelectEntryPoint(this);
    }

    public void ToggleBaseAlpha(bool value)
    {
        if(value)
            canvasFader.PlayInstant(CanvasFader.FadeType.In);
        else
            canvasFader.PlayInstant(CanvasFader.FadeType.Out);
    }
}
