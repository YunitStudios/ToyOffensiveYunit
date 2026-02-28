using System;
using EditorAttributes;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class EndScreenPopup : Popup
{
    [Title("\n<b><color=#fb80ff>Extra", 15, 5, false)] 
    [SerializeField] private GameObject failContent;
    [SerializeField] private GameObject winContent;

    protected override void Awake()
    {
        base.Awake();
        failContent.SetActive(false);
        winContent.SetActive(false);
    }

    // Wait a frame to make sure other logic completes first
    
    [Button]
    public void Fail()
    {
        failContent.SetActive(true);
        winContent.SetActive(false);
        Tween.Delay(0.0001f, Display);
    }
    [Button]
    public void Win()
    {
        failContent.SetActive(false);
        winContent.SetActive(true);
        Tween.Delay(0.0001f, Display); 
    }

}
