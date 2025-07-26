using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddForcesTest : MonoBehaviour
{
    [SerializeField] private Vector3 force = new();
    [SerializeField] private Vector3 torque = new();

    private void FixedUpdate()
    {
        RocketData.RocketRigidbody.AddForce(force);
        RocketData.RocketRigidbody.AddTorque(torque);
    }
}
