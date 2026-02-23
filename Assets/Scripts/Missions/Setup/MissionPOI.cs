using UnityEngine;
using EditorAttributes;
using System.Collections.Generic;

public class MissionPOI : MonoBehaviour
{
    public static List<MissionPOI> activePOIs;

    [SerializeField] private POIType poiType;
    public POIType GetPOIType => poiType;
    [SerializeField] private POIShape poiShape;
    public POIShape GetPOIShape => poiShape;
    [SerializeField, ShowField(nameof(IsCircle))] private float radius;


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

    private void OnEnable()
    {
        activePOIs.Add(this);
    }
    private void OnDisable()
    {
        activePOIs.Remove(this);
    }

    private void OnDrawGizmos()
    {
        
    }
}
