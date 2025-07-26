using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFinPhysics : MonoBehaviour
{
    [SerializeField] private AirfoilReader airfoilReader;

    [Header("Fixed Values (auto-set by rocket)")]
    [SerializeField] private float lengthOfProflie = 0.1f;
    [SerializeField] private float airDensity = 1.204f;
    [SerializeField] private float dynamicViscosity = 0.0000172f;
    [SerializeField] private float servoSecPer60Deg = 0.11f;

    [Header("Changeable Values")]
    [Range(-15, 15)] public float alphaAngleGoal;
    [Range(-15, 15)][SerializeField] private float alphaAngleCurrent;
    public float AlphaAngleCurrent => alphaAngleCurrent;
    public float flowVelocity;

    [Header("Ignore")]
    [SerializeField] private bool ignoreForceLift;
    [SerializeField] private bool ignoreForceDrag;

    [Header("Infos")]
    [SerializeField] private int raynoldsNumber;
    [SerializeField] private Vector3 forceLift, forceDrag;
    [SerializeField] private float lengthForceLift;
    (Vector3, Vector3) rangeForceLift;
    (float, float) rangeCl;

    public Vector3 ForceLift => forceLift;
    public float LengthForceLift => lengthForceLift;
    public (Vector3, Vector3) RangeForceLift => rangeForceLift;

    public float DistanceFromCenterXZ => Mathf.Abs((new Vector2(povitPos.position.x, povitPos.position.z) - new Vector2(RocketData.RocketTransform.position.x, RocketData.RocketTransform.position.z)).magnitude);

    //Objects
    Transform povitPos, povitRot;
    Rigidbody mainRB;

    [Header("Debug")]
    [SerializeField] Transform testObj;

    private void Start()
    {
        GetObjects();
        rangeCl = (airfoilReader.polarDataNcrit9.GetMinValue(PolarValueType.Cl, 100000), airfoilReader.polarDataNcrit9.GetMaxValue(PolarValueType.Cl, 100000));
    }

    private void FixedUpdate()
    {
        HandleServoRotation(); // For alphaAngle of the fins
        CalculateForces();
        UpdateVisuals();

        //Apply force
        mainRB.AddForceAtPosition((ignoreForceLift ? Vector3.zero : forceLift) + (ignoreForceDrag ? Vector3.zero : forceDrag), povitPos.position); //Point of force is at the povit object
    }

    private void OnValidate()
    {
        try
        {
            GetObjects();
            CalculateForces();
            UpdateVisuals();
        }
        catch { }
    }

    void GetObjects()
    {
        povitPos = transform.Find("PovitPos");
        povitRot = povitPos.Find("PovitRot");
        mainRB = RocketData.RocketRigidbody;
    }
    void UpdateVisuals()
    {
        //Rotation
        alphaAngleCurrent = Mathf.Clamp(alphaAngleCurrent, -15f, 15f); //instant rotation
        povitRot.localRotation = Quaternion.Euler(povitRot.localEulerAngles.x, povitRot.localRotation.y, alphaAngleCurrent);

        if (testObj)
            testObj.position = povitRot.position + forceLift.normalized * 0.1f;
    }

    void HandleServoRotation()
    {
        alphaAngleGoal = Mathf.Clamp(alphaAngleGoal, -15f, 15f);
        if (alphaAngleCurrent != alphaAngleGoal)
        {
            float degressDelta = alphaAngleGoal - alphaAngleCurrent;
            float degressPerSecond = 60f / servoSecPer60Deg;
            float degressDeltaPerSecond = degressDelta * degressPerSecond * Time.fixedDeltaTime;
            float degressAcceleration = degressDeltaPerSecond * Time.fixedDeltaTime;
            if (Mathf.Abs(degressAcceleration) > Mathf.Abs(degressDelta)) // Clamp so it doesn't overshoot
                degressAcceleration = degressDelta;
            alphaAngleCurrent += degressAcceleration;
        }
    }
    void CalculateForces()
    {
        //Directions of the vectors
        Vector3 directionForceLift = povitPos.right; //Directly perpendicular from the fin (with orientation of rocket)
        Vector3 directionForceDrag = -RocketData.RocketTransform.up; //Directly down from the fin (with orientation of rocket)

        //Calculations
        raynoldsNumber = (int)(flowVelocity * lengthOfProflie * airDensity / dynamicViscosity);
        float cl = airfoilReader.polarDataNcrit9.ConvertValue(alphaAngleCurrent, PolarValueType.Alpha, PolarValueType.Cl, raynoldsNumber);
        float cd = airfoilReader.polarDataNcrit9.ConvertValue(alphaAngleCurrent, PolarValueType.Alpha, PolarValueType.Cd, raynoldsNumber);

        float dynamicAirDensity = airDensity * flowVelocity * flowVelocity * 0.5f;
        //print($"{gameObject.name} dynamicAirDensity:{dynamicAirDensity} = airDensity:{airDensity} * flowVelocity:{flowVelocity} * flowVelocity:{flowVelocity} * 0.5f");

        float referencedArea = lengthOfProflie * lengthOfProflie; //Temp: Rough calculation
        //print($"{gameObject.name} referencedArea:{referencedArea} = lengthOfProflie:{lengthOfProflie} * lengthOfProflie:{lengthOfProflie}");

        //Length of the vectors
        lengthForceLift = cl * dynamicAirDensity * referencedArea;
        //print($"{gameObject.name} lengthForceLift:{lengthForceLift} = cl:{cl} * dynamicAirDensity:{dynamicAirDensity} * referencedArea:{referencedArea}");
        float lengthForceDrag = cd * dynamicAirDensity * referencedArea;
        (float, float) rangeLengthForceLift = (rangeCl.Item1 * dynamicAirDensity * referencedArea, rangeCl.Item2 * dynamicAirDensity * referencedArea);

        //Set values
        forceLift = directionForceLift * lengthForceLift;
        forceDrag = directionForceDrag * lengthForceDrag;
        rangeForceLift = (directionForceLift * rangeLengthForceLift.Item1, directionForceLift * rangeLengthForceLift.Item2);

        if (testObj)
            testObj.position = povitRot.position + directionForceLift * 0.1f;
    }

    public float GetAlphaValueFromLift(float forceLiftGoal)
    {
        raynoldsNumber = (int)(flowVelocity * lengthOfProflie * airDensity / dynamicViscosity);
        float dynamicairDensity = airDensity * flowVelocity * flowVelocity * 0.5f;
        float referencedArea = lengthOfProflie * lengthOfProflie;
        float cl = forceLiftGoal / (dynamicairDensity * referencedArea);
        return airfoilReader.polarDataNcrit9.ConvertValue(cl, PolarValueType.Cl, PolarValueType.Alpha, raynoldsNumber);
    }

    public void SetFixedValues(float lengthOfProflie, float airDensity, float dynamicViscosity, float servoSecPer60Deg)
    {
        this.lengthOfProflie = lengthOfProflie;
        this.airDensity = airDensity;
        this.dynamicViscosity = dynamicViscosity;
        this.servoSecPer60Deg = servoSecPer60Deg;
    }
}
