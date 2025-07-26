using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads data from the rocket
/// </summary>
public class RocketData : MonoBehaviour
{
    [SerializeField] private Vector3 _rotation; //For Inspector

    private static RocketData self;

    private static Vector3 eulerRotation;
    /// <summary>
    /// The correct orientation of the rockets rotation in euler angles
    /// </summary>
    public static Vector3 EulerRotation
    {
        get
        {
            try
            {
                self.UpdateValues();
                return eulerRotation;
            }
            catch
            {
                return Vector3.zero;
            }
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

    private static Rigidbody rocketRigidbody;
    /// <summary>
    /// The rigidbody of the rocket
    /// </summary>
    public static Rigidbody RocketRigidbody
    {
        get => rocketRigidbody;
    }

    public static Vector3 acceleration;
    /// <summary>
    /// The acceleration of the rigidbody of the rocket
    /// </summary>
    public static Vector3 Acceleration
    {
        get => acceleration;
    }
    Vector3 lastVelocity;


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
        rocketRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        UpdateValues();

        // Acceleration
        acceleration = (RocketRigidbody.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = RocketRigidbody.velocity;
    }

    private void UpdateValues()
    {
        try
        {
            SetStaticValues();
        }
        catch { }

        //Get signed angles
        Vector3 gottenEulerRotation = new(
            GetSignedAngle(RotType.X),
            GetSignedAngle(RotType.Y),
            GetSignedAngle(RotType.Z)
            );

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
