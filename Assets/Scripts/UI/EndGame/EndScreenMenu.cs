using System;
using EditorAttributes;
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

    [Button]
    public void Fail()
    {
        Display();
        failContent.SetActive(true);
        winContent.SetActive(false);
    }
    [Button]
    public void Win()
    {
        Display();
        failContent.SetActive(false);
        winContent.SetActive(true);
    }

}
