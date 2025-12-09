using System;
using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TabManager : MonoBehaviour
{

    private Tab currentTab;
    [SerializeField] private List<Tab> tabList;
    [SerializeField, Tooltip("Default tab to toggle when activated")] private Tab startingTab;
    [SerializeField] private UnityEvent onChangeTab;

    [SerializeField, Tooltip("Option to assign buttons to navigating back and forth through tabs")] private bool buttonNavigation = false;
    [SerializeField, ShowField(nameof(buttonNavigation))] private InputActionReference uiForwardInput;
    [SerializeField, ShowField(nameof(buttonNavigation))] private InputActionReference uiBackInput;

    private int tabIndex;

    private void Awake()
    {
        // Setup all tabs
        foreach (Tab tab in tabList)
            tab.OnSelectTabAction += ChangeTab;
           }

    private void Start()
    {
        SetStartingTab();
    }

    private void OnEnable()
    {
        if(uiForwardInput)
            uiForwardInput.action.performed += ForwardPressed;
        if (uiBackInput)
            uiBackInput.action.performed += BackwardPressed;
    }
    private void OnDisable()
    {
        if(uiForwardInput)
            uiForwardInput.action.performed -= ForwardPressed;
        if (uiBackInput)
            uiBackInput.action.performed -= BackwardPressed;
    }

    private void ForwardPressed(InputAction.CallbackContext context)
    {
        if (buttonNavigation)
            TabForward();
    }
    private void BackwardPressed(InputAction.CallbackContext context)
    {
        if (buttonNavigation)
            TabBackward();
    }

    public void SetStartingTab()
    {
        if (startingTab)
        {
            if (currentTab != null)
                currentTab.SetContent(false);

            startingTab.SetContent(true);
            currentTab = startingTab;
        }

        if (buttonNavigation)
            tabIndex = tabList.IndexOf(currentTab);
    }

    public void ChangeTab(Tab tab)
    {
        if (currentTab)
        {
            if (tab == currentTab) return;
            currentTab.SetContent(false);
        }

        currentTab = tab;

        tab.SetContent(true, true);

        onChangeTab?.Invoke();
    }

    public void DisableSelectedTab()
    {
        if (currentTab)
            currentTab.SetContent(false);

        currentTab = null;
    }

    public void TabForward()
    {
        tabIndex = (tabIndex + 1) % tabList.Count;

        // If select tab is inactive, skip it
        if (!tabList[tabIndex].gameObject.activeSelf)
        {
            TabForward();
            return;
        }

        ChangeTab(tabList[tabIndex]);
    }
    public void TabBackward()
    {
        tabIndex--;
        if (tabIndex < 0)
            tabIndex = tabList.Count - 1;

        // If select tab is inactive, skip it
        if (!tabList[tabIndex].gameObject.activeSelf)
        {
            TabBackward();
            return;
        }

        ChangeTab(tabList[tabIndex]);
    }

}