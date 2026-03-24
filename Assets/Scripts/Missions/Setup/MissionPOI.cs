using System;
using UnityEngine;
using EditorAttributes;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MissionPOI : MonoBehaviour
{
    private static readonly int ShaderProgress = Shader.PropertyToID("_Progress");

    [Header("POI Data")]
    [Tooltip("List of missions this POI is available in")]
    [SerializeField] private MissionSO[] missions;
    [SerializeField] private POIType poiType;
    public POIType GetPOIType => poiType;
    [Tooltip("All start points will be manually ordered ascending based on this value")]
    [SerializeField, ShowField(nameof(IsStartPoint))] private int startPointOrder;
    [Tooltip("How long it takes for the player to be extracted after entering the area")]
    [SerializeField, ShowField(nameof(IsExtractPoint))] private float extractDuration = 3;
    [SerializeField] private POIShape poiShape;
    public POIShape GetPOIShape => poiShape;
    [SerializeField, ShowField(nameof(IsCircle))] private float radius = 1;
    private bool IsStartPoint => poiType is POIType.StartPoint;
    private bool IsExtractPoint => poiType is POIType.ExtractPoint;
    private bool IsPoint => poiShape is POIShape.Point;
    private bool IsCircle => poiShape is POIShape.Circle;
    public enum POIType
    {
        StartPoint,
        ExtractPoint
    }
    public enum POIShape
    {
        Point,
        Circle
    }
    [SerializeField] private Renderer visual;
    private bool HasVisual => visual;
    [SerializeField, ShowField(nameof(HasVisual))] private Material visualMaterial;

    [Header("Input Callbacks")] 
    [SerializeField] private VoidEventChannelSO onMissionCompleted;

    [Header("Output Callbacks")] 
    [SerializeField] private VoidEventChannelSO onLevelWin;
    
    private float ExtractProgress => currentExtractTime / extractDuration;

    public int GetStartPointOrder => startPointOrder;

    private bool active;
    private bool extracted;
    private float currentExtractTime = 0.0f;

    private static bool DisplayedNotif;

    public Vector3 GetPosition()
    {
        if (IsPoint)
            return transform.position;
        if (IsCircle)
        {
            Vector2 randomPoint = Random.insideUnitCircle * radius;
            return transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);
        }
        
        return transform.position;
    }

    public bool IsInRange()
    {
        if (IsPoint)
            return false;
        if (IsCircle)
            return Vector3.Distance(GameManager.PlayerData.PlayerPosition, transform.position) <= radius;
        
        return false;
    }

    private void OnEnable()
    {
        foreach (var mission in missions)
        {
            if(IsStartPoint)
                mission.startPoints.Add(this);
            if(IsExtractPoint)
                mission.extractPoints.Add(this);
        }

        onMissionCompleted.OnEventRaised += ExtractEnable;
    }
    private void OnDisable()
    {
        foreach (var mission in missions)
        {
            if(IsStartPoint)
                mission.startPoints.Remove(this);
            if(IsExtractPoint)
                mission.extractPoints.Remove(this);
        }
        
        onMissionCompleted.OnEventRaised -= ExtractEnable;
    }
    
    private void Update()
    {
        if (!active)
            return;
        
        if (IsExtractPoint)
            ExtractTick();
    }
    
    public void SetupPOI()
    {
        active = true;
        extracted = false;
        
        if(IsExtractPoint)
            TogglePOI(false);
        
        if(visualMaterial)
            visualMaterial.SetFloat(ShaderProgress, 0);

        DisplayedNotif = false;
    }
    
    private void TogglePOI(bool value)
    {
        active = value;
        if(visual)
            visual.enabled = value;
    }

    private void ExtractTick()
    {
        if(extracted)
            return;
        
        if(IsPoint)
        {
            Debug.LogError("Extract POI is set to Point, making extraction impossible");
            return;
        }

        if (IsInRange())
        {
            currentExtractTime += Time.deltaTime;
            if(visualMaterial)
                visualMaterial.SetFloat(ShaderProgress, ExtractProgress);
        }
        else
            currentExtractTime = 0.0f;
        
        if(ExtractProgress >= 1)
            Extract();
    }

    private void Extract()
    {
        onLevelWin?.Invoke();
        extracted = true;
    }

    private void ExtractEnable()
    {
        TogglePOI(true);
        // Jank solution to stop duplicate notifs
        if(!DisplayedNotif)
            NotificationUI.DisplayNotification?.Invoke(new NotificationUI.NotificationData("New Goal", "Reach the extract point"));

        DisplayedNotif = true;
    }

    private void OnDrawGizmos()
    {
        switch (poiType)
        {
            case POIType.StartPoint:
                Gizmos.color = Color.green;
                break;
            case POIType.ExtractPoint:
                Gizmos.color = Color.red;
                break;
        }
        if (IsPoint)
        {
            Gizmos.DrawSphere(transform.position, 0.25f);
        }
        else if (IsCircle)
        {
            transform.DrawGizmoDisk(radius);
        }
    }
}
