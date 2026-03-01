using System;
using System.Collections.Generic;
using EditorAttributes;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private CanvasFader canvasFader;
    [SerializeField]private TMP_Text notificationTitle;
    [SerializeField]private TMP_Text notificationContents;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private float defaultFadeInTime;
    [SerializeField] private float defaultFadeHoldTime;
    [SerializeField] private float defaultFadeOutTime;


    public static Action<NotificationData> DisplayNotification;

    private Queue<NotificationData> currentQueue = new();

    public struct NotificationData
    {
        public string title;
        public string contents;
        public float fadeInTime;
        public float fadeOutTime;
        public float fadeHoldTime;
        
        public NotificationData(string title, string contents, float fadeInTime = 0, float fadeHoldTime = 0, float fadeOutTime = 0)
        {
            this.title = title;
            this.contents = contents;
            this.fadeInTime = fadeInTime;
            this.fadeHoldTime = fadeHoldTime;
            this.fadeOutTime = fadeOutTime;
        }
    }

    private void OnEnable()
    {
        DisplayNotification += QueueNotification;
    }

    private void OnDisable()
    {
        DisplayNotification -= QueueNotification;
    }

    private void QueueNotification(NotificationData newNotification)
    {
        currentQueue.Enqueue(newNotification);
        
        TryDisplayNotification();
    }

    private void TryDisplayNotification()
    {
        if (canvasFader.IsFading)
            return;
        
        NotificationData currentData = currentQueue.Dequeue();
        
        notificationTitle.text = currentData.title;
        notificationContents.text = currentData.contents;
        
        if(currentData.fadeInTime == 0)
            currentData.fadeInTime = defaultFadeInTime;
        if(currentData.fadeOutTime == 0)
            currentData.fadeOutTime = defaultFadeOutTime;
        if(currentData.fadeHoldTime == 0)
            currentData.fadeHoldTime = defaultFadeHoldTime;
        canvasFader.OverwriteFadeTimes(currentData.fadeInTime, currentData.fadeOutTime, currentData.fadeHoldTime);
        
        canvasFader.Play(CanvasFader.FadeType.Full);
        canvasFader.OnFadeOutEnd += TryDisplayNotification;

    }
}
