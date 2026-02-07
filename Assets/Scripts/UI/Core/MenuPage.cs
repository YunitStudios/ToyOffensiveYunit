using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;

public class MenuPage : MenuContainer
{
    [Title("\n<b><color=#fb80ff>Page Data", 15, 5, false)]
    [SerializeField] private MenuButton backButton;

    public override void Init(MenuGroup group)
    {
        base.Init(group);
        
        if(backButton)
            backButton.SetClickEvent(parentGroup.ToggleStartingContainer);
    }
}