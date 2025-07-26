using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour
{
    public enum Mode
    {
        LockOnPosition, LockOnPositionAndRotation, LookAtRocket
    }

    [SerializeField] private Mode mode;
    [SerializeField] private bool zoomedIn;
    public bool ZoomedIn
    {
        get => zoomedIn;
        set
        {
            zoomedIn = value;
            OnZoomInChanged(zoomedIn);
        }
    }
    [SerializeField] private Transform targetTrans;


    Vector3 positionOffset;
    Quaternion rotationOffset;

    Camera cam;
    float initalFieldOfView;


    void Start()
    {
        cam = GetComponent<Camera>();
        initalFieldOfView = cam.fieldOfView;
        positionOffset = transform.position - RocketData.RocketTransform.position;
        rotationOffset = transform.rotation * Quaternion.Inverse(RocketData.RocketTransform.rotation);
        UpdateTransform();
    }

    void LateUpdate()
    {
        UpdateTransform();
    }

    void UpdateTransform()
    {
        switch (mode)
        {
            case Mode.LockOnPosition:
                transform.position = RocketData.RocketTransform.position + positionOffset;
                transform.rotation = rotationOffset;
                break;

            case Mode.LockOnPositionAndRotation:
                transform.position = RocketData.RocketTransform.position + RocketData.RocketTransform.rotation * positionOffset;
                transform.rotation = RocketData.RocketTransform.rotation * rotationOffset;
                break;

            case Mode.LookAtRocket:
                transform.position = targetTrans ? targetTrans.position : positionOffset;
                transform.LookAt(RocketData.RocketTransform);
                break;

            default:
                transform.position = positionOffset;
                transform.rotation = rotationOffset;
                break;
        }
    }

    void OnZoomInChanged(bool newValue)
    {
        if (!cam)
            cam = GetComponent<Camera>();
        if (newValue)
            cam.fieldOfView = initalFieldOfView * 0.5f;
        else
            cam.fieldOfView = initalFieldOfView;
    }

    public void SetActive(bool truth)
    {
        if (!cam)
            cam = GetComponent<Camera>();
        cam.enabled = truth;
    }

    // For Inspector
    void OnValidate()
    {
        initalFieldOfView = GetComponent<Camera>().fieldOfView;
        ZoomedIn = zoomedIn;

        if (mode == Mode.LookAtRocket) transform.position = targetTrans ? targetTrans.position : transform.position;
    }
}
