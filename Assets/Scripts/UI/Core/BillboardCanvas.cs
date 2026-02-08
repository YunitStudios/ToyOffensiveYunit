using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    [SerializeField] private bool yaw = true;
    [SerializeField] private bool pitch = true;
    
    private Transform camTransform;


    void Start()
    {
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (camTransform != null)
        {
            transform.rotation = Quaternion.Euler(pitch ? camTransform.eulerAngles.x : transform.eulerAngles.x, yaw ? camTransform.eulerAngles.y : transform.eulerAngles.y, camTransform.eulerAngles.z);   
        }
    }

    public void Manual()
    {
        camTransform = Camera.main.transform;
        
        transform.rotation = Quaternion.Euler(pitch ? camTransform.eulerAngles.x : transform.eulerAngles.x, yaw ? camTransform.eulerAngles.y : transform.eulerAngles.y, camTransform.eulerAngles.z);   
    }
}