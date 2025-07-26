using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ObsoleteScripts
{
    public enum DerivativeMeasurement
    {
        value,
        errorRateOfChange
    }

    public enum PIDAxis
    {
        Vertical,

        XRot,
        ZRot
    }

    public class RocketController : MonoBehaviour
    {

        [SerializeField] private PIDControllerValues pIDControllerX = new();
        [SerializeField] private PIDControllerValues pIDControllerY = new();
        [SerializeField] private PIDControllerValues pIDControllerZ = new();


        //SerializeFields

        [Header("Rocket")]

        [SerializeField] private GameObject rocket;


        [SerializeField] private ForceAdder forceAdder;


        [Header("Target")]
        [SerializeField] private float targetY;

        public float TargetY
        {
            get { return targetY; }
            set
            {
                pIDControllerX.ResetDerivativeInitialization();
                pIDControllerY.ResetDerivativeInitialization();
                pIDControllerZ.ResetDerivativeInitialization();

                targetY = value;

            }

        }

        [SerializeField] private Vector2 targetRotation = new(0, 0);

        [SerializeField] private float rotationMultiplyer;

        [SerializeField] private bool XOn, YOn, ZOn;

        [Header("Tester")]

        [SerializeField] private bool testerOn;

        [SerializeField] private Vector2 testRange = new(0, 0);

        [SerializeField] private float testerTollerance = 0.2f;

        [SerializeField] private float testerTimestep = 0.2f;




        //not Serialized

        private bool testerRestart = true;



        private IEnumerator TesterFunction()
        {
            while (Mathf.Abs(rocket.transform.position.y - TargetY) > testerTollerance)
            {
                print($"pos: {rocket.transform.position.y:F2} Tar: {TargetY:F2} PID: {forceAdder.ForceMagnitude:F2}");

                yield return new WaitForSeconds(testerTimestep);
            }

            TargetY = MathF.Round(UnityEngine.Random.Range(testRange.x, testRange.y));

            testerRestart = true;

        }




        //Functions

        private void FixedUpdate()
        {
            print($"Rotation: {rocket.transform.rotation.eulerAngles.x:F5}");//.x:F5}");//

            if (testerOn && testerRestart)
            {
                testerRestart = false;
                StartCoroutine(TesterFunction());
            }


            if (YOn)
            {
                forceAdder.ForceMagnitude = PIDController.PID(Time.deltaTime, rocket.transform.position.y, TargetY, pIDControllerY);
            }





            if (XOn)
            {
                forceAdder.ForceDir = new Vector3(
                    PIDController.PID(Time.deltaTime, RocketData.EulerRotation.x, targetRotation.x, pIDControllerX),
                    forceAdder.ForceDir.y,
                    forceAdder.ForceDir.z
                    );
                print($"Force: {forceAdder.ForceDir.x}");
            }



            if (ZOn)
            {
                forceAdder.ForceDir = new Vector3(
                    forceAdder.ForceDir.x,
                    forceAdder.ForceDir.y,
                    PIDController.PID(Time.deltaTime, RocketData.EulerRotation.z, targetRotation.y /*targetRotation is a z component in a Vector2*/, pIDControllerZ)
                    );
            }

        }






        private void OnValidate()
        {
            TargetY = targetY;
        }


    }





    [System.Serializable]
    public class PIDControllerValues
    {

        public PIDAxis pIDAxis;

        public bool isAngle;

        [Header("PID Settings")]
        public float proportionalGain = 1;
        public float integralGain = 1;
        public float derivativeGain = 0.01f;

        [Space(15f)]
        public Vector2 outputClamp = new(-1, 1);

        [Space(15f)]
        public float integralSaturation;

        [Space(15f)]
        public DerivativeMeasurement derivativeMeasurement;

        [HideInInspector] public float integrationStored;

        [HideInInspector] public float lastError;

        [HideInInspector] public float lastValue;

        [HideInInspector] public bool derivativeInitialized;

        [HideInInspector] public float errorRateOfChange;

        [HideInInspector] public float valueRateOfChange;

        [HideInInspector] public float deriveMeasure;

        [HideInInspector] public float error;

        [HideInInspector] public float P;

        [HideInInspector] public float I;

        [HideInInspector] public float D;

        public void ResetDerivativeInitialization()
        {
            derivativeInitialized = false;
        }

    }
}
