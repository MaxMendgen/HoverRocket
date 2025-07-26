using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Proportional–integral–derivative controller, used for approaching values with automatic adjustments. The creation was partially followed by https://vazgriz.com/621/pid-controllers/
/// </summary>
[System.Serializable]
public class PIDController
{
    public enum DerivativeMeasurement { Velocity, ErrorRateOfChange }
    public enum ValueType { Linear, Angular }

    public ValueType valueType;

    [Header("Gain Settings")]
    public float proportionalGain;
    public float integralGain;
    public float derivativeGain;

    [Header("Limit Settings")]
    public Vector2 outputRange = new(-1f, 1f);
    public float integralSaturation = 1f;
    public DerivativeMeasurement derivativeMeasurement;

    [Header("Debug")]
    public bool manualTargetMode;
    public float manualTargetValue;
    // public bool manualOutputMode;
    // public float manualOutputValue;

    private bool derivativeInitialized;
    private float errorLast;
    private float valueLast;

    private float integrationStored;

    private float value;
    /// <summary>
    /// The value of the PIDController. Use Update() to update the value.
    /// </summary>
    public float Value
    {
        get => value;
    }

    [Header("Info")]
    [SerializeField] private float _currentValue;
    [SerializeField] private float _targetValue;
    [SerializeField] private float _outputValue;


    /// <summary>
    /// Updates the values of the PID
    /// </summary>
    /// <param name="dt">difference of time</param>
    /// <param name="currentValue">the current value</param>
    /// <param name="targetValue">the target value</param>
    /// <returns></returns>
    public float Update(float dt, float currentValue, float targetValue)
    {
        // if (manualOutputMode)
        // {
        //     value = manualOutputValue;
        //     return Value;
        // }

        if (manualTargetMode)
            targetValue = manualTargetValue;

        float error;

        if (valueType == ValueType.Linear)
        {
            error = targetValue - currentValue;
        }
        else
        {
            error = PIDControllerExtensions.AngleDifference(targetValue, currentValue);
        }

        //float error = valueType == ValueType.Linear ?  : PIDControllerV2Extensions.AngleDifference(targetValue, currentValue);
        errorLast = error;

        // Calculate P term
        float P = proportionalGain * error;

        // Calculate both D term
        float errorRateOfChange = (valueType == ValueType.Linear ? (error - errorLast) : PIDControllerExtensions.AngleDifference(error, errorLast)) / dt;
        errorLast = error;
        float valueRateOfChange = (valueType == ValueType.Linear ? (currentValue - valueLast) : PIDControllerExtensions.AngleDifference(currentValue, valueLast)) / dt;
        valueLast = currentValue;

        // Choose D term to use
        float deriveMeasure = 0;
        if (derivativeInitialized)
            deriveMeasure = derivativeMeasurement == DerivativeMeasurement.Velocity ? -valueRateOfChange : errorRateOfChange;
        else
            derivativeInitialized = true;
        float D = derivativeGain * deriveMeasure;

        // calculate I term
        integrationStored = Mathf.Clamp(integrationStored + (error * dt), -integralSaturation, integralSaturation);
        float I = integralGain * integrationStored;

        // Clamp
        value = Mathf.Clamp(P + I + D, outputRange.x, outputRange.y);

        // For Info
        _currentValue = currentValue;
        _targetValue = targetValue;
        _outputValue = value;

        return Value;
    }

    /// <summary>
    /// Resets the derivative initialization. Use this on teleportation or non use for a long period of time.
    /// </summary>
    /// <returns></returns>
    public void Reset()
    {
        derivativeInitialized = false;
    }

    /// <summary>
    /// Resets the info variables for the inspector. Use this for debugging purposes.
    /// </summary>
    public void DebugResetInfo()
    {
        _currentValue = 0;
        _targetValue = 0;
        _outputValue = 0;
    }
}

public static class PIDControllerExtensions
{
    /// <summary>
    /// Calculates the angle difference by remapping the range to [-180, 180]
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float AngleDifference(float a, float b) => (a - b + 180) % 360 - 180;

    /// <summary>
    /// Runs Reset() on all PIDControllers
    /// </summary>
    /// <param name="list"></param>
    public static void ResetAll(this List<PIDController> list)
    {
        foreach (PIDController x in list)
            x.Reset();
    }
}
