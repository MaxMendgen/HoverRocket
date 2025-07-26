using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPhysics : MonoBehaviour
{
    public List<ControlFinPhysics> controlFinPhysics;
    [SerializeField] private Transform propellerTransform;

    [Header("Fixed values")]
    [SerializeField] private float gravity = 9.81f;
    public float Gravity => gravity;
    [SerializeField] private float rocketBottomRadius;
    [SerializeField] private float airDensity = 1.204f;
    [SerializeField] private float dynamicViscosity = 0.0000172f;
    [SerializeField] private float lengthOfFinProflies = 0.1f;
    [SerializeField] private float finServoSecPer60Deg = 0.11f;
    [SerializeField] private float lengthOfPropeller = 0.2f;
    [SerializeField] private float propellerAdvanceRatio = 0.8f;
    [SerializeField] private Vector3 massMomentOfInertia = new(0.0215254f, 0.00317358f, 0.020875f);

    [Header("Changeable values")]
    [SerializeField] private float flowVelocity;
    public float FlowVelocity
    {
        get => flowVelocity;
        set
        {
            UpdatesValues();
            flowVelocity = value;
        }
    }
    public void SetFlowVelocityAsForce(float force)
    {
        FlowVelocity = Mathf.Sqrt(force / (airDensity * Mathf.PI * rocketBottomRadius * rocketBottomRadius * 0.5f));
    }

    [Header("Ignore")]
    public bool ignoreForcePropeller;
    [HideInInspector] public bool ignoreTorquePropeller = true;
    public bool ignoreForceGravity;
    public bool ignoreAllForceLift;
    public bool ignoreAllForceDrag;

    [Header("Debug")]
    [SerializeField] private float _setForcePropeller = 0f;
    [SerializeField] private bool _setForcePropellerActive = false;

    [Header("Infos")]
    [SerializeField] private Vector3 forcePropeller;
    [SerializeField] private Vector3 torquePropeller;
    [SerializeField] private Vector3 forceGravity;
    //[SerializeField] private Vector3 forcePropellerLeverArm, forceGravityLeverArm;
    //[SerializeField] private Vector3 torqueOfInertia;



    //Objects
    Rigidbody mainRB;


    private void Start()
    {
        Physics.gravity = Vector3.zero;
        mainRB.maxAngularVelocity = Mathf.Infinity;
        mainRB.inertiaTensor = massMomentOfInertia;
        UpdatesValues();

        ignoreTorquePropeller = true; // Temp
    }

    private void FixedUpdate()
    {
        CalculateForces();

        //Apply forces
        mainRB.AddForce((ignoreForcePropeller ? Vector3.zero : forcePropeller) + (ignoreForceGravity ? Vector3.zero : forceGravity));
        mainRB.AddTorque(ignoreTorquePropeller ? Vector3.zero : torquePropeller);

        // Propeller rotation (Visual)
        float degressPerSecond = 360f * FlowVelocity / (propellerAdvanceRatio * lengthOfPropeller);
        if (ignoreForcePropeller)
            degressPerSecond = 0f;
        propellerTransform.Rotate(0f, -degressPerSecond * Time.fixedDeltaTime, 0f);

        // Inspector
        if (_setForcePropellerActive)
        {
            SetFlowVelocityAsForce(_setForcePropeller);
            _setForcePropellerActive = false;
        }
    }

    private void OnValidate()
    {
        try
        {
            UpdatesValues();
        }
        catch { }
    }

    void UpdatesValues()
    {
        //Get objects
        if (!mainRB)
            mainRB = transform.GetComponent<Rigidbody>();

        //Set fin values
        if (controlFinPhysics.Count <= 0)
            return;

        for (int i = 0; i < controlFinPhysics.Count; i++)
        {
            controlFinPhysics[i].SetFixedValues(
                lengthOfProflie: lengthOfFinProflies,
                airDensity: airDensity,
                dynamicViscosity: dynamicViscosity,
                servoSecPer60Deg: finServoSecPer60Deg
            );
            controlFinPhysics[i].flowVelocity = FlowVelocity;
        }
    }

    void CalculateForces()
    {
        //Directions of the vector
        Vector3 directionForcePropeller = RocketData.RocketTransform.up;
        Vector3 directionTorquePropeller = RocketData.RocketTransform.up;
        Vector3 directionForceGravity = Vector3.down;

        //Calculations
        float rocketBottomArea = Mathf.PI * rocketBottomRadius * rocketBottomRadius;

        //Length of the vector
        float lengthForcePropeller = FlowVelocity * FlowVelocity * airDensity * rocketBottomArea * 0.5f;
        float lengthTorquePropeller = PropellerForceToTorque(lengthForcePropeller);
        float lengthForceGravity = gravity * mainRB.mass;

        forcePropeller = directionForcePropeller * lengthForcePropeller;
        torquePropeller = directionTorquePropeller * lengthTorquePropeller;
        forceGravity = lengthForceGravity * directionForceGravity;


        /// Torque of inertia (faild, used Rigidbody inertiaTensor instead)

        //Vector3 distanceVector = RocketData.RocketRigidbody.centerOfMass;

        //forcePropellerLeverArm = Vector3.Cross(distanceVector, forcePropeller);
        //forceGravityLeverArm = Vector3.Cross(distanceVector, forceGravity);

        // // Torque of inertia
        // Vector3 sumLeverArms = Vector3.zero;
        // foreach (ControlFinPhysics fin in controlFinPhysics) {
        //     sumLeverArms += fin.forceLiftLeverArm;
        //     sumLeverArms += fin.forceDragLeverArm;
        // }
        // sumLeverArms += forcePropellerLeverArm;
        // sumLeverArms += forceGravityLeverArm;
        // torqueOfInertia = new Vector3(
        //     -sumLeverArms.x * massMomentOfInertia.x,
        //     -sumLeverArms.y * massMomentOfInertia.y,
        //     -sumLeverArms.z * massMomentOfInertia.z
        // );
    }

    public float PropellerForceToTorque(float force)
    {
        //Using IRL Data, and regression
        double[] a = new double[] {
            0.00006979352f,
            -0.001440736f,
            0.01193092f,
            -0.05103281f,
            0.1213321f,
            -0.1605992f,
            0.123761f,
            -0.029144f
        };

        double torque =
            Mathf.Pow(force, 7) * a[0] +
            Mathf.Pow(force, 6) * a[1] +
            Mathf.Pow(force, 5) * a[2] +
            Mathf.Pow(force, 4) * a[3] +
            Mathf.Pow(force, 3) * a[4] +
            Mathf.Pow(force, 2) * a[5] +
            force * a[6] +
            a[7];

        return (float)torque;
    }

}
