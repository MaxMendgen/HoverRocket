using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    [SerializeField] private RocketPhysics rocketPhysics;

    [Header("Target")]
    [SerializeField] private Transform targetTrans;
    Transform oldTargetTrans;
    Vector3 toTargetVector;
    [Range(0f, 359f)][SerializeField] private float targetRotationY;

    Vector2 toTargetXZ;
    float distanceFromCenterXZ;

    [Header("PID Controllers")]
    [SerializeField] private PIDController heightYControl;
    [SerializeField] private PIDController rollYControl = new() { valueType = PIDController.ValueType.Angular };
    [Space]
    // [SerializeField] private PIDController rotationXZForwardControl = new() { valueType = PIDController.ValueType.Angular };
    // [SerializeField] private PIDController velocityXZForwardControl;
    // [SerializeField] private PIDController distanceXZControl;
    // [Space]
    // [SerializeField] private PIDController rotationXZSideControl = new() { valueType = PIDController.ValueType.Angular };
    // [SerializeField] private PIDController velocityXZSideControl;
    [SerializeField] private PIDController rotationXControl = new() { valueType = PIDController.ValueType.Angular };
    [SerializeField] private PIDController velocityXControl;
    [SerializeField] private PIDController distanceXControl;
    [Space]
    [SerializeField] private PIDController rotationZControl = new() { valueType = PIDController.ValueType.Angular };
    [SerializeField] private PIDController velocityZControl;
    [SerializeField] private PIDController distanceZControl;

    [Header("Fin Angle Factor")]
    [SerializeField] private Vector2 finAngleFactorRange = new(0.1f, 2f);
    public (float, float) FinAngleFactorRange
    {
        get => finAngleFactorRange.AsTupleRange();
        set => finAngleFactorRange = new Vector2(value.Item1, value.Item2);
    }
    [SerializeField] private float _currentFinAngleFactor = 1f;
    private float finAngleFactor = 1f;
    public float FinAngleFactor
    {
        get => finAngleFactor;
        set
        {
            _currentFinAngleFactor = value;
            finAngleFactor = value;
        }
    }

    [Header("Ignore")]
    [SerializeField] private bool ignoreHightYControl;
    [Space]
    [SerializeField] private bool ignoreRollYFinControl;
    [SerializeField] private bool ignoreXZFinControl;
    [SerializeField] private bool ignoreAllFinControl;
    [Space]
    [SerializeField] private bool ignoreFinAngleFactor;

    [Header("Debug Graphs")]
    [SerializeField] private MultiGraphRenderer multiGraphRenderer1;
    [SerializeField] private MultiGraphRenderer multiGraphRenderer2;
    [SerializeField] private MultiGraphRenderer multiGraphRenderer3;

    [Header("Debug Vectors")]
    [SerializeField] private float debugForcesScale = 2f;
    [SerializeField] private float debugDirScale = 0.5f;
    [SerializeField] private float debugYOffset = -0.2f;
    [Space]
    [SerializeField] private Transform debugGoalObj;
    [SerializeField] private LineRenderer debugGoalLR;
    [SerializeField] private Transform debugActualObj;
    [SerializeField] private LineRenderer debugActualLR;
    [SerializeField] private Transform debugTargetDirObj;
    [SerializeField] private LineRenderer debugTargetDirLR;

    Vector3 offsetActualObj, offsetGoalObj, offsetTargetDirObj;


    private void Start()
    {
        distanceFromCenterXZ = rocketPhysics.controlFinPhysics[0].DistanceFromCenterXZ;
    }

    private void FixedUpdate()
    {
        // Handle OnChange for targetTransform
        if (targetTrans != oldTargetTrans)
        {
            oldTargetTrans = targetTrans;
            OnTargetChanged(targetTrans);
        }

        // Difference between target and rocket, used for PID calculations
        toTargetVector = targetTrans.position - RocketData.RocketTransform.position;

        // PID calculations
        if (!ignoreHightYControl) HandleHeightControl();
        if (!ignoreAllFinControl) HandleFinControl();
    }

    void HandleHeightControl()
    {
        heightYControl.Update(Time.deltaTime, RocketData.RocketTransform.position.y, targetTrans.position.y);
        rocketPhysics.SetFlowVelocityAsForce(heightYControl.Value);
    }

    void HandleFinControl()
    {
        // Calculate forceXZGoal
        // Vector2 forceGoalXZ = Vector2.zero;
        // if (!ignoreXZFinControl)
        // {
        //     // Define projections of toTargetVector
        //     toTargetXZ = new(toTargetVector.x, toTargetVector.z);
        //     Vector3 toTargetDirectionXZ3 = Vector3.ProjectOnPlane(toTargetVector, Vector3.up).normalized; // targetDeltaXZ, but as a normalized Vector3 

        //     // Define normal planes along and perpendicular to the target direction
        //     Vector3 forwardNormalPlane3 = Vector3.Cross(toTargetDirectionXZ3, Vector3.up).normalized;
        //     Vector3 perpendicularToTargetDirectionXZ3 = forwardNormalPlane3;
        //     Vector3 sideNormalPlane3 = Vector3.Cross(perpendicularToTargetDirectionXZ3, Vector3.up).normalized;


        //     // Handle distanceXZControl
        //     float currentDistanceXZ = -toTargetXZ.magnitude; // allways negative
        //     distanceXZControl.Update(Time.fixedDeltaTime, currentDistanceXZ, 0f); // output allways positive


        //     // Handle velocityXZForwardControl and velocityXZSideControl

        //     // Vector3 currentVelocityDirectionXZ3 = Vector3.ProjectOnPlane(RocketData.RocketRigidbody.velocity, Vector3.up).normalized; // Get current velocity direction on XZ plane (as Vector3)
        //     // float currentVelocitySignXZ = Mathf.Sign(Vector3.Dot(currentVelocityDirectionXZ3, toTargetDirectionXZ3)); // if same direction dot = 1, if opposite direction dot = -1
        //     // float currentVelocityMagnitudeXZ = Vector3.ProjectOnPlane(RocketData.RocketRigidbody.velocity, Vector3.up).magnitude; // allways positive
        //     // float currentVelocityXZ = currentVelocityMagnitudeXZ * currentVelocitySignXZ; // PID input value

        //     Vector3 currentVelocityXZ3 = Vector3.ProjectOnPlane(RocketData.RocketRigidbody.velocity, Vector3.up);
        //     Vector3 currentVelocityDirectionXZ3 = currentVelocityXZ3.normalized;

        //     float currentVelocitySignXZForward = Mathf.Sign(Vector3.Dot(currentVelocityDirectionXZ3, toTargetDirectionXZ3)); // if same direction dot = 1, if opposite direction dot = -1
        //     float currentVelocityMagnitudeXZForward = Vector3.ProjectOnPlane(currentVelocityXZ3, forwardNormalPlane3).magnitude; // allways positive
        //     float currentVelocityXZForward = currentVelocityMagnitudeXZForward * currentVelocitySignXZForward; // PID input value
        //     velocityXZForwardControl.Update(Time.fixedDeltaTime, currentVelocityXZForward, distanceXZControl.Value);

        //     float currentVelocitySignXZSide = Mathf.Sign(Vector3.Dot(currentVelocityDirectionXZ3, perpendicularToTargetDirectionXZ3)); // if same direction dot = 1, if opposite direction dot = -1
        //     float currentVelocityMagnitudeXZSide = Vector3.ProjectOnPlane(currentVelocityXZ3, sideNormalPlane3).magnitude; // allways positive
        //     float currentVelocityXZSide = currentVelocityMagnitudeXZSide * currentVelocitySignXZSide; // PID input value
        //     velocityXZSideControl.Update(Time.fixedDeltaTime, currentVelocityXZSide, 0f);


        //     // Handle rotationXZForwardControl and rotationXZSideControl 

        //     // Vector3 rocketTiltDirectionXZ3 = Vector3.ProjectOnPlane(RocketData.RocketTransform.up, Vector3.up).normalized; // Get tilt direction by projecting rocket up vector onto XZ plane (as Vector3)
        //     // float currentAngleDifferenceSignXZ = Mathf.Sign(Vector3.Dot(rocketTiltDirectionXZ3, toTargetDirectionXZ3)); // if same direction (tilt) dot = 1, if opposite direction (tilt) dot = -1
        //     // float currentAbsoluteAngleDifferenceXZ = Vector3.Angle(Vector3.up, RocketData.RocketTransform.up); // allways positive
        //     // float currentAngleDifferenceXZ = currentAbsoluteAngleDifferenceXZ * currentAngleDifferenceSignXZ; // PID input value

        //     Vector3 rocketTiltDirectionXZ3 = Vector3.ProjectOnPlane(RocketData.RocketTransform.up, Vector3.up).normalized; // Get tilt direction by projecting rocket up vector onto XZ plane (as Vector3)
        //     Vector3 rocketUpForward3 = Vector3.ProjectOnPlane(RocketData.RocketTransform.up, forwardNormalPlane3);
        //     Vector3 rocketUpSide3 = Vector3.ProjectOnPlane(RocketData.RocketTransform.up, sideNormalPlane3);

        //     float currentAngleDifferenceSignXZForward = Mathf.Sign(Vector3.Dot(rocketTiltDirectionXZ3, toTargetDirectionXZ3)); // if same direction (tilt) dot = 1, if opposite direction (tilt) dot = -1
        //     float currentAbsoluteAngleDifferenceXZForward = Vector3.Angle(Vector3.up, rocketUpForward3); // allways positive
        //     float currentAngleDifferenceXZForward = currentAbsoluteAngleDifferenceXZForward * currentAngleDifferenceSignXZForward; // PID input value
        //     rotationXZForwardControl.Update(Time.fixedDeltaTime, currentAngleDifferenceXZForward, velocityXZForwardControl.Value);

        //     float currentAngleDifferenceSignXZSide = Mathf.Sign(Vector3.Dot(rocketTiltDirectionXZ3, perpendicularToTargetDirectionXZ3)); // if same direction (tilt) dot = 1, if opposite direction (tilt) dot = -1
        //     float currentAbsoluteAngleDifferenceXZSide = Vector3.Angle(Vector3.up, rocketUpSide3); // allways positive
        //     float currentAngleDifferenceXZSide = currentAbsoluteAngleDifferenceXZSide * currentAngleDifferenceSignXZSide; // PID input value
        //     rotationXZSideControl.Update(Time.fixedDeltaTime, currentAngleDifferenceXZSide, velocityXZSideControl.Value);


        //     // Calculate direction to target
        //     float angleGoalY = Vector2.SignedAngle(Vector2.right, toTargetXZ) + 180f; // add 180° to tilt against the target direction, because fin forces are applied at the bottom of the rocket
        //     Vector2 forceDirectionGoalForward = ExtraClass.AngleToVector2(angleGoalY);
        //     Vector2 forceDirectionGoalSide = ExtraClass.AngleToVector2(angleGoalY + 90f);


        //     // End force
        //     forceGoalXZ = forceDirectionGoalForward * rotationXZForwardControl.Value + forceDirectionGoalSide * rotationXZSideControl.Value; // forceDirectionGoalZ = direction to target, forceDirectionGoalX = direction to the side of the rocket (90°)

        //     // Ajust end force to rocket XZ plane
        //     Vector3 forceGoalXZ3 = new(forceGoalXZ.x, 0f, forceGoalXZ.y);
        //     Vector3 forceGoalXZProjected = Vector3.ProjectOnPlane(forceGoalXZ3, RocketData.RocketTransform.up);
        //     forceGoalXZ = new(forceGoalXZProjected.x, forceGoalXZProjected.z);


        //     // Debug
        //     offsetGoalObj = ExtraClass.ConvertPointBetweenPlanes(new Vector3(forceGoalXZ.x, 0f, forceGoalXZ.y), Vector3.up, RocketData.RocketTransform.up) * debugForcesScale;
        //     offsetTargetDirObj = ExtraClass.ConvertPointBetweenPlanes(new Vector3(toTargetXZ.x, 0f, toTargetXZ.y), Vector3.up, RocketData.RocketTransform.up).normalized * debugDirScale;

        //     // offsetGoalObj = new Vector3(forceGoalXZ.x, 0f, forceGoalXZ.y) * debugVectorScale;
        //     // offsetTargetDirObj = new Vector3(forceDirectionGoalXZ.x, 0f, forceDirectionGoalXZ.y).normalized * debugVectorScale * 0.25f;
        // }

        Vector2 forceGoalXZ = Vector2.zero;
        if (!ignoreXZFinControl)
        {
            // Handle distance Control
            float currentDistanceX = -toTargetVector.x;
            distanceXControl.Update(Time.fixedDeltaTime, Mathf.Abs(currentDistanceX), 0f);

            float currentDistanceZ = -toTargetVector.z;
            distanceZControl.Update(Time.fixedDeltaTime, Mathf.Abs(currentDistanceZ), 0f);


            // Handle velocity 

            float currentVelocityX = -RocketData.RocketRigidbody.velocity.x * Mathf.Sign(toTargetVector.x);
            velocityXControl.Update(Time.fixedDeltaTime, currentVelocityX, distanceXControl.Value);

            float currentVelocityZ = -RocketData.RocketRigidbody.velocity.z * Mathf.Sign(toTargetVector.z);
            velocityZControl.Update(Time.fixedDeltaTime, currentVelocityZ, distanceZControl.Value);


            // Handle rotation Control
            Vector3 rocketUpX3 = Vector3.ProjectOnPlane(RocketData.RocketTransform.up, Vector3.forward);
            float currentAngleDifferenceX = Vector3.SignedAngle(rocketUpX3, Vector3.up, Vector3.forward);
            rotationXControl.Update(Time.fixedDeltaTime, -currentAngleDifferenceX, velocityXControl.Value * Mathf.Sign(toTargetVector.x));

            Vector3 rocketUpZ3 = Vector3.ProjectOnPlane(RocketData.RocketTransform.up, Vector3.left);
            float currentAngleDifferenceZ = Vector3.SignedAngle(rocketUpZ3, Vector3.up, Vector3.left);
            rotationZControl.Update(Time.fixedDeltaTime, -currentAngleDifferenceZ, velocityZControl.Value * Mathf.Sign(toTargetVector.z));


            // End force
            forceGoalXZ = new Vector2(rotationXControl.Value, rotationZControl.Value);

            // Ajust end force to rocket XZ plane
            Vector3 forceGoalXZ3 = new(forceGoalXZ.x, 0f, forceGoalXZ.y);
            Vector3 forceGoalXZProjected = Vector3.ProjectOnPlane(forceGoalXZ3, RocketData.RocketTransform.up);
            forceGoalXZ = new(forceGoalXZProjected.x, forceGoalXZProjected.z);


            // Debug
            offsetGoalObj = ExtraClass.ConvertPointBetweenPlanes(new Vector3(forceGoalXZ.x, 0f, forceGoalXZ.y), Vector3.up, RocketData.RocketTransform.up) * debugForcesScale;
            offsetTargetDirObj = ExtraClass.ConvertPointBetweenPlanes(new Vector3(toTargetXZ.x, 0f, toTargetXZ.y), Vector3.up, RocketData.RocketTransform.up).normalized * debugDirScale;

            // offsetGoalObj = new Vector3(forceGoalXZ.x, 0f, forceGoalXZ.y) * debugVectorScale;
            // offsetTargetDirObj = new Vector3(forceDirectionGoalXZ.x, 0f, forceDirectionGoalXZ.y).normalized * debugVectorScale * 0.25f;
        }

        // Calculate torqueYGoal
        float torqueYGoal = 0f;
        if (!ignoreRollYFinControl)
        {
            // Handle rollYControl
            float currentAngleDifferenceY = RocketData.RocketTransform.eulerAngles.y.GetAngle180(); // [-180, 180]
            rollYControl.Update(Time.fixedDeltaTime, currentAngleDifferenceY, targetRotationY);

            // End torque
            torqueYGoal = rollYControl.Value * Mathf.Deg2Rad;
        }

        // Convert forceGoalXZ to forceGoalXZAsXY, because Calculate3FinFormularV3 needs a (x, y) instead of (x, z)
        Vector3 forceGoalAsXY3 = ExtraClass.ConvertPointBetweenPlanes(new Vector3(forceGoalXZ.x, 0f, forceGoalXZ.y), Vector3.up, Vector3.forward);
        Vector2 forceGoalXZAsXY = new(forceGoalAsXY3.x, forceGoalAsXY3.y);

        // Calculate fin angles
        (float finAngle1, float finAngle2, float finAngle3) = Calculate3FinFormularV3(forceGoalXZAsXY, torqueYGoal, distanceFromCenterXZ, 90f);

        // Calculate fin angle factor
        if (!ignoreFinAngleFactor)
        {
            // Force == heightYControl.Value
            FinAngleFactor = Mathf.Abs(heightYControl.Value.Remap(heightYControl.outputRange.AsTupleRange(), FinAngleFactorRange));
        }
        else
        {
            FinAngleFactor = 1f;
        }

        // Apply fin angles
        rocketPhysics.controlFinPhysics[0].alphaAngleGoal = finAngle1 * FinAngleFactor;
        rocketPhysics.controlFinPhysics[1].alphaAngleGoal = finAngle2 * FinAngleFactor;
        rocketPhysics.controlFinPhysics[2].alphaAngleGoal = finAngle3 * FinAngleFactor;


        // Graph pid values
        // multiGraphRenderer1?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, rocketPhysics.controlFinPhysics[0].alphaAngleGoal));
        // multiGraphRenderer1?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, rocketPhysics.controlFinPhysics[0].AlphaAngleCurrent));
        // multiGraphRenderer2?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, rocketPhysics.controlFinPhysics[1].alphaAngleGoal));
        // multiGraphRenderer2?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, rocketPhysics.controlFinPhysics[1].AlphaAngleCurrent));
        // multiGraphRenderer3?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, rocketPhysics.controlFinPhysics[2].alphaAngleGoal));
        // multiGraphRenderer3?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, rocketPhysics.controlFinPhysics[2].AlphaAngleCurrent));

        // multiGraphRenderer1?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, distanceXZControl.Value));
        // multiGraphRenderer1?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, 0f));
        // multiGraphRenderer2?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, velocityXZForwardControl.Value));
        // multiGraphRenderer2?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, velocityXZSideControl.Value));
        // multiGraphRenderer3?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, rotationXZForwardControl.Value + 1f));
        // multiGraphRenderer3?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, rotationXZSideControl.Value - 1f));

        multiGraphRenderer1?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, distanceXControl.Value));
        multiGraphRenderer1?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, distanceZControl.Value));
        multiGraphRenderer2?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, velocityXControl.Value));
        multiGraphRenderer2?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, velocityZControl.Value));
        multiGraphRenderer3?.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, rotationXControl.Value + 1f));
        multiGraphRenderer3?.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, rotationZControl.Value - 1f));


        // Visual Debug
        Vector3 combinedForces = new();
        foreach (ControlFinPhysics controlFinPhysics in rocketPhysics.controlFinPhysics)
            combinedForces += controlFinPhysics.ForceLift;
        offsetActualObj = combinedForces * debugForcesScale;
    }

    // Particulary works, but torqueY is not calculated
    (float, float, float) Calculate3FinFormularV1(Vector2 forceXZGoal)
    {
        Vector2 scaledforceXZGoal = forceXZGoal / 1.5f;
        float forceFin1 = scaledforceXZGoal.x * Mathf.Cos(0f * Mathf.Deg2Rad) + scaledforceXZGoal.y * Mathf.Sin(0f * Mathf.Deg2Rad);
        float forceFin2 = scaledforceXZGoal.x * Mathf.Cos(120f * Mathf.Deg2Rad) + scaledforceXZGoal.y * Mathf.Sin(240f * Mathf.Deg2Rad);
        float forceFin3 = scaledforceXZGoal.x * Mathf.Cos(120f * Mathf.Deg2Rad) + scaledforceXZGoal.y * Mathf.Sin(240f * Mathf.Deg2Rad);
        float finAngle1 = rocketPhysics.controlFinPhysics[0].GetAlphaValueFromLift(forceFin1);
        float finAngle2 = rocketPhysics.controlFinPhysics[1].GetAlphaValueFromLift(forceFin2);
        float finAngle3 = rocketPhysics.controlFinPhysics[2].GetAlphaValueFromLift(forceFin3);
        return (finAngle1, finAngle2, finAngle3);
    }

    // Doesen't work, idk what Jan did
    (float, float, float) Calculate3FinFormularV2(float alphaGoal, float rollXZ, float rollY)
    {
        float finAngle1 = Mathf.Cos((alphaGoal - 0f) * Mathf.Deg2Rad) * rollXZ + rollY;
        float finAngle2 = Mathf.Cos((alphaGoal - 120f) * Mathf.Deg2Rad) * rollXZ + rollY;
        float finAngle3 = Mathf.Cos((alphaGoal - 240f) * Mathf.Deg2Rad) * rollXZ + rollY;
        return (finAngle1, finAngle2, finAngle3);
    }

    // From David K, https://math.stackexchange.com/questions/5036713/the-3-fin-problem-how-to-calculate-fin-forces-for-a-desired-force-and-torque
    (float, float, float) Calculate3FinFormularV3(Vector2 forceXYGoal, float torqueZGoal, float distanceFromCenter, float angleOffset)
    {
        float forceFin1 = 2f / 3f * (forceXYGoal.x * Mathf.Cos((0f + angleOffset) * Mathf.Deg2Rad) + forceXYGoal.y * Mathf.Sin((0f + angleOffset) * Mathf.Deg2Rad)) + 1f / (3f * distanceFromCenter) * torqueZGoal;
        float forceFin2 = 2f / 3f * (forceXYGoal.x * Mathf.Cos((120f + angleOffset) * Mathf.Deg2Rad) + forceXYGoal.y * Mathf.Sin((120f + angleOffset) * Mathf.Deg2Rad)) + 1f / (3f * distanceFromCenter) * torqueZGoal;
        float forceFin3 = 2f / 3f * (forceXYGoal.x * Mathf.Cos((240f + angleOffset) * Mathf.Deg2Rad) + forceXYGoal.y * Mathf.Sin((240f + angleOffset) * Mathf.Deg2Rad)) + 1f / (3f * distanceFromCenter) * torqueZGoal;
        float finAngle1 = rocketPhysics.controlFinPhysics[0].GetAlphaValueFromLift(forceFin1);
        float finAngle2 = rocketPhysics.controlFinPhysics[1].GetAlphaValueFromLift(forceFin2);
        float finAngle3 = rocketPhysics.controlFinPhysics[2].GetAlphaValueFromLift(forceFin3);
        return (finAngle1, finAngle2, finAngle3);
    }


    private void LateUpdate()
    {
        // Visual Debug
        try
        {
            debugActualObj.position = RocketData.RocketTransform.position + ExtraClass.ConvertPointBetweenPlanes(offsetActualObj, Vector3.up, RocketData.RocketTransform.up) + new Vector3(0f, debugYOffset, 0f);
            debugGoalObj.position = RocketData.RocketTransform.position + ExtraClass.ConvertPointBetweenPlanes(offsetGoalObj, Vector3.up, RocketData.RocketTransform.up) + new Vector3(0f, debugYOffset, 0f);
            debugTargetDirObj.position = RocketData.RocketTransform.position + ExtraClass.ConvertPointBetweenPlanes(offsetTargetDirObj, Vector3.up, RocketData.RocketTransform.up) + new Vector3(0f, debugYOffset, 0f);

            debugActualLR.SetPosition(0, RocketData.RocketTransform.position + new Vector3(0f, debugYOffset, 0f));
            debugActualLR.SetPosition(1, debugActualObj.position);
            debugGoalLR.SetPosition(0, RocketData.RocketTransform.position + new Vector3(0f, debugYOffset, 0f));
            debugGoalLR.SetPosition(1, debugGoalObj.position);
            debugTargetDirLR.SetPosition(0, RocketData.RocketTransform.position + new Vector3(0f, debugYOffset, 0f));
            debugTargetDirLR.SetPosition(1, debugTargetDirObj.position);
        }
        catch { }
    }

    private void OnTargetChanged(Transform newTransform)
    {
        // Reset PIDs
        heightYControl.Reset();
        rollYControl.Reset();
        // distanceXZControl.Reset();
        // velocityXZForwardControl.Reset();
        // rotationXZForwardControl.Reset();
        distanceXControl.Reset();
        distanceZControl.Reset();
        velocityXControl.Reset();
        velocityZControl.Reset();
        rotationXControl.Reset();
        rotationZControl.Reset();
    }


    void OnValidate()
    {
        // Reset PID debug variables for debugging in the inspector
        heightYControl.DebugResetInfo();
        rollYControl.DebugResetInfo();
        // distanceXZControl.DebugResetInfo();
        // velocityXZForwardControl.DebugResetInfo();
        // rotationXZForwardControl.DebugResetInfo();
        distanceXControl.DebugResetInfo();
        distanceZControl.DebugResetInfo();
        velocityXControl.DebugResetInfo();
        velocityZControl.DebugResetInfo();
        rotationXControl.DebugResetInfo();
        rotationZControl.DebugResetInfo();

        // For inspector
        FinAngleFactor = finAngleFactor;
        FinAngleFactorRange = finAngleFactorRange.AsTupleRange();
    }
}
