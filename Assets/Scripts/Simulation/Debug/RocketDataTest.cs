using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads data from the rocket
/// </summary>
public class RocketDataTest : MonoBehaviour
{
    [SerializeField] private Vector3 _rotation; //For Inspector

    private static RocketDataTest self;

    private static Vector3 eulerRotation;
    /// <summary>
    /// The correct orientation of the rockets rotation in euler angles
    /// </summary>
    public static Vector3 EulerRotation
    {
        get
        {
            self.UpdateValues();
            return eulerRotation;
        }
    }

    private static Transform rocketTransform;
    /// <summary>
    /// The transform of the rocket
    /// </summary>
    public static Transform RocketTransform
    {
        get => rocketTransform;
    }

    private void OnValidate()
    {
        SetStaticValues();
    }
    private void OnEnable()
    {
        SetStaticValues();
    }
    private void Awake()
    {
        SetStaticValues();
    }

    void SetStaticValues()
    {
        self = this;
        rocketTransform = transform;
    }

    private void FixedUpdate()
    {
        UpdateValues();
    }

    private void UpdateValues()
    {
        //Get signed angles
        Vector3 gottenEulerRotation = new(
            GetSignedAngle(RotType.X),
            GetSignedAngle(RotType.Y),
            GetSignedAngle(RotType.Z)
            );

        // THIS DOESN'T WORK AHHHHHHHHHHHHH

        // //Correction under horizon 
        // bool xUnderHorizon = gottenEulerRotation.x <= -90 || gottenEulerRotation.x >= 90;
        // bool zUnderHorizon = gottenEulerRotation.z <= -90 || gottenEulerRotation.z >= 90;
        // print($"x:{xUnderHorizon} z:{zUnderHorizon}");
        // if (xUnderHorizon) //x under horizon 
        // {
        //     gottenEulerRotation += new Vector3(0f, 180f, 180f); //Rotate y,z 180
        //     gottenEulerRotation = RemapAngles(gottenEulerRotation);
        // }
        // if (zUnderHorizon && !xUnderHorizon) //z under horizon 
        // {
        //     gottenEulerRotation += new Vector3(180f, 0f, 0f); //Rotate x 180
        //     gottenEulerRotation = RemapAngles(gottenEulerRotation);
        // }

        //Set publicly for readability
        eulerRotation = gottenEulerRotation;

        //For inspector
        _rotation = eulerRotation;
    }

    enum RotType { X, Y, Z }

    float GetSignedAngle(RotType rotType)
    {
        return rotType switch
        {
            RotType.X => Vector2.SignedAngle(
                new Vector2(Vector3.up.y, Vector3.up.z),
                new Vector2(transform.up.y, transform.up.z)
                ),
            RotType.Y => Vector2.SignedAngle(
                new Vector2(Vector3.forward.z, Vector3.forward.x),
                new Vector2(transform.forward.z, transform.forward.x)
                ),
            RotType.Z => Vector2.SignedAngle(
                new Vector2(Vector3.up.x, Vector3.up.y),
                new Vector2(transform.up.x, transform.up.y)
                ),
            _ => 0,
        };
    }

    float RemapAngle(float angle) => (angle + 540) % 360 - 180;
    Vector3 RemapAngles(Vector3 angles) => new(RemapAngle(angles.x), RemapAngle(angles.y), RemapAngle(angles.z));
}
