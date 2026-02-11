using System;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

public class MenuGroup : MonoBehaviour
{

    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField, DataTable, Validate(nameof(CheckStartingContainer))] private ContainerGroupData[] containers;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    [SerializeField] private UnityEvent OnChangeContainer;

    private MenuContainer currentContainer;

    private int tabIndex;

    private void Awake()
    {
        // Setup all tabs
        foreach (var container in containers)
            container.container.Init(this); 
        
        ToggleStartingContainer();
    }
    
    
    private ValidationCheck CheckStartingContainer()
    {
        int startCount = 0;
        foreach(var containerData in containers)
        {
            if (containerData.isStartingContainer)
                startCount++;
                
            if(startCount > 1)
                return ValidationCheck.Fail("You cannot have multiple starting containers. Will default to the first in the list", MessageMode.Error, true);
        }

        return ValidationCheck.Pass();
    }

    public void ToggleStartingContainer()
    {
        foreach(var container in containers)
        {
            if (!container.isStartingContainer)
                continue;
            
            // If this is the starting container, turn any current containers off
            if(currentContainer)
                currentContainer.ToggleContent(false);

            container.container.ToggleContent(true);
            currentContainer = container.container;
        }

    }

    public void ChangeContainer(MenuContainer container)
    {
        // Disable current container
        if (currentContainer)
        {
            if (container == currentContainer) return;
            currentContainer.ToggleContent(false);
        }

        currentContainer = container;

        container.ToggleContent(true);

        OnChangeContainer?.Invoke();
    }

    public void DisableSelectedTab()
    {
        if (currentContainer)
            currentContainer.ToggleContent(false);

        currentContainer = null;
    }
    

    [Serializable]
    public struct ContainerGroupData
    {
        public MenuContainer container;
        [Tooltip("Signifies the main container in the group. This could be the front page or the first tab in a menu")] public bool isStartingContainer;
    }

}