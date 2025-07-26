using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObsoleteScripts;

/// <summary>
/// An alternative version of RocketController (OBSOLETE)
/// </summary>
public class RocketControllerV2 : MonoBehaviour
{
    [Header("Rocket")]
    [SerializeField] private GameObject rocket;
    [SerializeField] private ForceAdder forceAdder;

    [Header("PID Controllers")]
    public PIDController yAxisControl;
    public PIDController yAxisGravControl;
    public PIDController
        xRotControl = new() { valueType = PIDController.ValueType.Angular },
        yRotControl = new() { valueType = PIDController.ValueType.Angular },
        zRotControl = new() { valueType = PIDController.ValueType.Angular };

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Target")]
    [SerializeField] private float targetPosY;
    public float TargetPosY
    {
        get => targetPosY;
        set
        {
            yAxisControl.Reset();
            targetPosY = value;
        }
    }
    [SerializeField] private float targetRotY;
    public float TargetRotY
    {
        get => targetRotY;
        set
        {
            xRotControl.Reset();
            yRotControl.Reset();
            zRotControl.Reset();
            targetRotY = value;
        }
    }


    private void FixedUpdate()
    {
        //Update Dir
        xRotControl.Update(Time.deltaTime, rocket.transform.rotation.eulerAngles.x, 0f);
        yRotControl.Update(Time.deltaTime, rocket.transform.rotation.eulerAngles.y, targetRotY);
        zRotControl.Update(Time.deltaTime, rocket.transform.rotation.eulerAngles.z, 0f);

        //Use RotateTowards to reduce jitter
        Quaternion fromRotation = Quaternion.FromToRotation(forceAdder.ForceDir, Vector3.up);
        Quaternion toRotation = Quaternion.Euler(-xRotControl.Value, -yRotControl.Value, -zRotControl.Value); //Goal is the opposite rotation of the rocket
        float rotationSpeedDelta = rotationSpeed * Time.fixedDeltaTime;

        Quaternion endRotation = Quaternion.RotateTowards(fromRotation, toRotation, rotationSpeedDelta);
        forceAdder.ForceDir = endRotation * Vector3.up; //Rotation to Direction Vector

        //Update Y
        yAxisControl.Update(Time.deltaTime, rocket.transform.position.y, TargetPosY);
        forceAdder.ForceMagnitude = yAxisControl.Value;
    }

    private void OnValidate()
    {
        try
        {
            TargetPosY = targetPosY;
        }
        catch { }
    }
}
