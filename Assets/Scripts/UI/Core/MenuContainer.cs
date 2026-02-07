using System;
using EditorAttributes;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class MenuContainer : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private ContainerContent[] contents;
    [SerializeField] private MenuButton button;
    
    [Title("\n<b><color=#80f0ff>UI Input Navigation", 15, 5, false, 2f, TextAnchor.LowerLeft)]
    [SerializeField, Tooltip("UI Object to select when opening container")] 
    private GameObject firstSelectedObject;
    [SerializeField, Tooltip("If ticked this will automatically find the first UI object from the firstSelected root"),ShowField(nameof(HasFirstSelected))] 
    private bool findFirstSelectedInChildren;
    
    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    [SerializeField] private UnityEvent OnContainerEnabled;
    [SerializeField] private UnityEvent OnContainerDisabled;

    private bool HasFirstSelected => firstSelectedObject;
    
    protected MenuGroup parentGroup;
    
    public virtual void Init(MenuGroup group)
    {
        // Get canvas if it exists
        foreach (var containerContent in contents)
        {
            if(containerContent.canvasGroup.TryGetComponent<Canvas>(out var canvas))
                containerContent.StoreCanvas(canvas);
        }

        if (firstSelectedObject&& findFirstSelectedInChildren)
            firstSelectedObject = firstSelectedObject.GetComponentInChildren<Selectable>().gameObject;

        ToggleContent(false);

        parentGroup = group;
        
        if(button)
            button.SetClickEvent(TrySelectContainer);
    }

    private void TrySelectContainer()
    {
        parentGroup.ChangeContainer(this);
    }

    public void ToggleContent(bool value)
    {

        //ToggleContent(value);
        ToggleCanvas(value);
        if (value)
            OnContainerEnabled?.Invoke();
        else
            OnContainerDisabled?.Invoke();
        
        if (value && firstSelectedObject)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
        if(button)
            button.SetState(value);
    }

    private void ToggleCanvas(bool value)
    {


        foreach (var content in contents)
        {
            if (content.alphaTween.isAlive)
                content.alphaTween.Complete();

            // Instant toggle
            if (content.fadeTime == 0)
            {
                content.canvasGroup.alpha = value ? 1.0f : 0.0f;
                if (content.Canvas)
                    content.Canvas.enabled = value;
            }
            // Fade in/out
            else
            {
                if (content.Canvas && value)
                    content.Canvas.enabled = true;

                content.alphaTween = Tween.Alpha(content.canvasGroup, value ? 1.0f : 0.0f, content.fadeTime, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true).OnComplete(
                    () =>
                    {
                        if (content.Canvas && !value)
                            content.Canvas.enabled = false;
                    });
            }

            content.canvasGroup.interactable = value;
            content.canvasGroup.blocksRaycasts = value;
        }
    }
    
    
    [Serializable]
    public class ContainerContent
    {
        public CanvasGroup canvasGroup;
        public float fadeTime;

        public void StoreCanvas(Canvas canvas) => this.Canvas = canvas;
        
        public Canvas Canvas { get; private set; }
        public Tween alphaTween;
    }

}
