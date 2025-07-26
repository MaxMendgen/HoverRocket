using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//standart PID Controller nach https://vazgriz.com/621/pid-controllers/

namespace ObsoleteScripts
{
    public class PIDController : MonoBehaviour
    {
        public static float PID(float dt, float currentValue, float targetValue, PIDControllerValues pIDControllerValues)
        {
            //dt = timestepp
            // P = spring
            // D = dampener
            // I = eliminate steady state error

            // IntegralSaturation should be set to the inputrange of the system

            if (pIDControllerValues.isAngle)
            {
                //Compute Error
                pIDControllerValues.error = AngleDifference(targetValue, currentValue);

                //Compute errorRateOfChange
                pIDControllerValues.errorRateOfChange = AngleDifference(pIDControllerValues.error, pIDControllerValues.lastError) / dt;

                //Compute valueRateOfChange
                pIDControllerValues.valueRateOfChange = AngleDifference(currentValue, pIDControllerValues.lastValue) / dt;


            }
            else
            {
                //for liniar PID Control

                //Compute Error
                pIDControllerValues.error = targetValue - currentValue;


                //Compute Proportional
                pIDControllerValues.P = pIDControllerValues.proportionalGain * pIDControllerValues.error;
                //Compute errorRateOfChange


                //Compute Integral
                pIDControllerValues.integrationStored = Mathf.Clamp(pIDControllerValues.integrationStored + (pIDControllerValues.error * dt), -pIDControllerValues.integralSaturation, pIDControllerValues.integralSaturation);
                pIDControllerValues.I = pIDControllerValues.integralGain * pIDControllerValues.integrationStored;

                pIDControllerValues.errorRateOfChange = (pIDControllerValues.error - pIDControllerValues.lastError) / dt;


                //Compute valueRateOfChange
                pIDControllerValues.valueRateOfChange = (currentValue - pIDControllerValues.lastValue) / dt;

            }

            pIDControllerValues.lastError = pIDControllerValues.error;

            pIDControllerValues.lastValue = currentValue;










            //Compute Derivative
            pIDControllerValues.deriveMeasure = 0;
            //skip first iteration of derivative measurement to avoid it beeing P because there is no previos tick
            if (pIDControllerValues.derivativeInitialized)
            {
                if (pIDControllerValues.derivativeMeasurement == DerivativeMeasurement.value)
                {
                    //Use valueRateOfChange to remove DerivativeTick
                    pIDControllerValues.deriveMeasure = -pIDControllerValues.valueRateOfChange;
                }
                else
                {
                    //Use errorRateOfChange to use DerivativeTick
                    pIDControllerValues.deriveMeasure = pIDControllerValues.errorRateOfChange;
                }
            }
            else
            {
                //activate derivative measurement for the next tick
                pIDControllerValues.derivativeInitialized = true;
            }

            pIDControllerValues.D = pIDControllerValues.derivativeGain * pIDControllerValues.deriveMeasure;

            //print($"P: {pIDControllerValues.P:F2} I: {pIDControllerValues.I:F2} D: {pIDControllerValues.D:F2}");

            //return the clamped controlvalue
            return Mathf.Clamp(pIDControllerValues.P + pIDControllerValues.I + pIDControllerValues.D, pIDControllerValues.outputClamp.x, pIDControllerValues.outputClamp.y);
        }

        public static float PIDPOW(float dt, float currentValue, float targetValue, PIDControllerValues pIDControllerValues)
        {
            //dt = timestepp
            // P = spring
            // D = dampener
            // I = eliminate steady state error

            // IntegralSaturation should be set to the inputrange of the system

            if (pIDControllerValues.isAngle)
            {
                //Compute Error
                pIDControllerValues.error = Mathf.Pow(AngleDifference(targetValue, currentValue), 2);

                //Compute errorRateOfChange
                pIDControllerValues.errorRateOfChange = AngleDifference(pIDControllerValues.error, pIDControllerValues.lastError) / dt;

                //Compute valueRateOfChange
                pIDControllerValues.valueRateOfChange = AngleDifference(currentValue, pIDControllerValues.lastValue) / dt;


            }
            else
            {
                //for liniar PID Control

                //Compute Error
                pIDControllerValues.error = targetValue - currentValue;


                //Compute Proportional
                pIDControllerValues.P = pIDControllerValues.proportionalGain * pIDControllerValues.error;
                //Compute errorRateOfChange


                //Compute Integral
                pIDControllerValues.integrationStored = Mathf.Clamp(pIDControllerValues.integrationStored + (pIDControllerValues.error * dt), -pIDControllerValues.integralSaturation, pIDControllerValues.integralSaturation);
                pIDControllerValues.I = pIDControllerValues.integralGain * pIDControllerValues.integrationStored;

                pIDControllerValues.errorRateOfChange = (pIDControllerValues.error - pIDControllerValues.lastError) / dt;


                //Compute valueRateOfChange
                pIDControllerValues.valueRateOfChange = (currentValue - pIDControllerValues.lastValue) / dt;

            }

            pIDControllerValues.lastError = pIDControllerValues.error;

            pIDControllerValues.lastValue = currentValue;










            //Compute Derivative
            pIDControllerValues.deriveMeasure = 0;
            //skip first iteration of derivative measurement to avoid it beeing P because there is no previos tick
            if (pIDControllerValues.derivativeInitialized)
            {
                if (pIDControllerValues.derivativeMeasurement == DerivativeMeasurement.value)
                {
                    //Use valueRateOfChange to remove DerivativeTick
                    pIDControllerValues.deriveMeasure = -pIDControllerValues.valueRateOfChange;
                }
                else
                {
                    //Use errorRateOfChange to use DerivativeTick
                    pIDControllerValues.deriveMeasure = pIDControllerValues.errorRateOfChange;
                }
            }
            else
            {
                //activate derivative measurement for the next tick
                pIDControllerValues.derivativeInitialized = true;
            }

            pIDControllerValues.D = pIDControllerValues.derivativeGain * pIDControllerValues.deriveMeasure;

            //print($"P: {pIDControllerValues.P:F2} I: {pIDControllerValues.I:F2} D: {pIDControllerValues.D:F2}");

            //return the clamped controlvalue
            return Mathf.Clamp(pIDControllerValues.P + pIDControllerValues.I + pIDControllerValues.D, pIDControllerValues.outputClamp.x, pIDControllerValues.outputClamp.y);
        }

        public static float AngleDifference(float a, float b)
        {
            return (a - b + 540) % 360 - 180;   //calculate modular difference, and remap to [-180, 180]
        }
    }
}
