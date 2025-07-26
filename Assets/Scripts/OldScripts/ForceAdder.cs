using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObsoleteScripts
{
    /// <summary>
    /// Adds a given force from the scripts position to a rigid body
    /// </summary>
    public class ForceAdder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Rigidbody applyTo; //Apply to rigid body
        [SerializeField] private Vector3 forceEulerRotation; //Force looking in degrees (for auto recalculation to direction, but depended on forceDir)
        /// <summary>
        /// The rotation of the force converted in degrees
        /// </summary>
        public Vector3 ForceEulerRotation
        {
            get => DirectionToEulerRotation(forceDir).eulerAngles;
            set
            {
                forceDir = EulerRotationToDirection(value);
                forceEulerRotation = DirectionToEulerRotation(forceDir).eulerAngles;
                UpdateVisuals();
            }
        }

        private Vector3 forceDir; //Force looking in direction vector (main rotation calculation)
        /// <summary>
        /// The direction of the force as a vector
        /// </summary>
        public Vector3 ForceDir
        {
            get => forceDir;
            set
            {
                forceDir = value.normalized;
                forceEulerRotation = DirectionToEulerRotation(forceDir).eulerAngles;
                UpdateVisuals();
            }
        }

        [SerializeField] private float forceMagnitude;
        /// <summary>
        /// The length of the force
        /// </summary>
        public float ForceMagnitude
        {
            get => forceMagnitude;
            set
            {
                forceMagnitude = value;
                UpdateVisuals();
            }
        }

        [SerializeField] private ForceMode forceMode = ForceMode.Acceleration; //Force mode

        Vector3 force;

        //Conversion functions
        private Vector3 EulerRotationToDirection(Vector3 rot) => Quaternion.Euler(rot) * Vector3.up;
        private Quaternion DirectionToEulerRotation(Vector3 dir) => Quaternion.FromToRotation(Vector3.up, dir);

        private void CalculateForce() => force = forceDir * forceMagnitude;

        private void Start()
        {
            if (!applyTo)
                Debug.LogWarning("ForceAdderV2 doesn't have a rigid body to apply force to!");
        }

        private void FixedUpdate()
        {
            if (!applyTo)
                return;

            //Add force
            CalculateForce();
            applyTo.AddForceAtPosition(applyTo.transform.rotation * force, transform.position, forceMode);
        }

        private void UpdateVisuals()
        {
            //Update rotation of visual vector
            CalculateForce();
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, force.normalized);
        }
        private void OnValidate()
        {
            //For inspector
            ForceMagnitude = forceMagnitude;
            ForceEulerRotation = forceEulerRotation;
            ForceDir = forceDir;
        }
    }
}
