using EditorAttributes;
using TMPro;
using UnityEngine;

public class MenuTab : MenuContainer
{
    [Title("\n<b><color=#fb80ff>Tab Attributes", 15, 5, false)] 
    [SerializeField] private TMP_Text tabTitle;

    public override void ToggleContent(bool value)
    {
        base.ToggleContent(value);

        if(tabTitle)
            tabTitle.text = button.GetText;
    }
}
