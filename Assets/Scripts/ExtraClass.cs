using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extra methods that Unity doesn't have
/// </summary>
public static class ExtraClass
{
    #region Random
    /// <summary>
    /// A better version of Random.Range() for lower range of integers.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive).</param>
    /// <returns>A random integer between min and max.</returns>
    public static int HighRandomRange(int min, int max)
    {
        InitRandomSeed();
        if (min > max)
            throw new System.ArgumentException();
        return (Random.Range(0, max * 100) % (max + 1 - min)) + min;
    }

    /// <summary>
    /// Determines whether a random event occurs based on a given percentage chance.
    /// </summary>
    /// <param name="percentage">Chance of success (0-100).</param>
    /// <returns>True if the event occurs, otherwise false.</returns>
    public static bool RandomChance(float percentage)
    {
        InitRandomSeed();
        if (percentage < 0f || percentage > 100f)
            throw new System.ArgumentOutOfRangeException();
        return Random.value > 1f - percentage * 0.01f;
    }

    /// <summary>
    /// Randomly shuffles a list.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    /// <returns>A new shuffled list.</returns>
    public static List<T> ShuffleList<T>(List<T> list)
    {
        List<T> shuffled = new();
        int listFinishedCount = list.Count;
        for (int i = 0; i < listFinishedCount; i++)
        {
            int r = HighRandomRange(0, list.Count - 1);
            shuffled.Add(list[r]);
            list.RemoveAt(r);
        }
        return shuffled;
    }

    /// <summary>
    /// Initializes the random seed to ensure better randomness.
    /// </summary>
#pragma warning disable
    public static void InitRandomSeed() => Random.InitState(System.Environment.TickCount * Random.seed / 7);
#pragma warning restore
    #endregion

    #region Lerping
    /// <summary>
    /// Calculates the time factor for interpolating via lerp.
    /// </summary>
    /// <param name="lerpSpeed">The speed of interpolation.</param>
    /// <returns>The lerp time factor.</returns>
    public static float CalculateLerpTime(float lerpSpeed) => 1 - Mathf.Pow(2, -Time.deltaTime * lerpSpeed);

    /// <summary>
    /// Calculates the fixed time factor for interpolating via lerp.
    /// </summary>
    /// <param name="lerpSpeed">The speed of interpolation.</param>
    /// <returns>The fixed lerp time factor.</returns>
    public static float CalculateFixedLerpTime(float lerpSpeed) => 1 - Mathf.Pow(2, -Time.fixedDeltaTime * lerpSpeed);

    /// <summary>
    /// Smoothly interpolates a float value.
    /// </summary>
    public static float LerpToFloat(float from, float to, float t, float minDistance = 0.01f) => Mathf.Abs(to - from) < minDistance ? to : Mathf.Lerp(from, to, t);

    /// <summary>
    /// Smoothly interpolates a Vector2 value.
    /// </summary>
    public static Vector2 LerpToVector2(Vector2 from, Vector2 to, float t, float minDistance = 0.01f) => Vector2.Distance(from, to) < minDistance ? to : Vector2.Lerp(from, to, t);

    /// <summary>
    /// Smoothly interpolates a Quaternion value.
    /// </summary>
    public static Quaternion LerpToQuaternion(Quaternion from, Quaternion to, float t, float minDistance = 0.01f) => Vector3.Distance(from.eulerAngles, to.eulerAngles) < minDistance ? to : Quaternion.Lerp(from, to, t);

    /// <summary>
    /// Smoothly interpolates a Quaternion value using Slerp.
    /// </summary>
    public static Quaternion SlerpToQuaternion(Quaternion from, Quaternion to, float t, float minDistance = 0.01f) => Vector3.Distance(from.eulerAngles, to.eulerAngles) < minDistance ? to : Quaternion.Slerp(from, to, t);
    #endregion

    #region Math
    /// <summary>
    /// Remaps a float value from one range to another.
    /// </summary>
    public static float Remap(this float value, (float, float) range1, (float, float) range2) => (value - range1.Item1) / (range1.Item2 - range1.Item1) * (range2.Item2 - range2.Item1) + range2.Item1;

    /// <summary>
    /// Converts an angle to the range -180 to 180 degrees.
    /// </summary>
    public static float GetAngle180(this float angle)
    {
        angle %= 360;
        return angle > 180 ? angle - 360 : angle;
    }

    /// <summary>
    /// Finds the closest value to a given number from a list.
    /// </summary>
    public static float ClosestValue(float number, List<float> list) => list.Aggregate((x, y) => Mathf.Abs(x - number) < Mathf.Abs(y - number) ? x : y);

    /// <summary>
    /// Combines two vectors by multiplying their components.
    /// </summary>
    public static Vector3 CombineVectors(Vector3 v1, Vector3 v2) => new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

    public static float Min(this (float, float) range) => new float[] { range.Item1, range.Item2 }.Min();
    public static float Max(this (float, float) range) => new float[] { range.Item1, range.Item2 }.Max();

    public static (float, float) AsTupleRange(this Vector2 range) => (range.x, range.y);

    /// <summary>
    /// Converts an angle in degrees to a Vector2 representation using Cosine for x and Sine for y.
    /// </summary>
    /// <param name="angle">the angle in degress</param>
    /// <returns></returns>
    public static Vector2 AngleToVector2(float angle) => new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

    /// <summary>
    /// Projects a point from one plane to another, preserving its position relative to the original plane.
    /// </summary>
    /// <param name="point">The original point lying on the fromNormalPlane.</param>
    /// <param name="fromNormal">The normal of the original plane.</param>
    /// <param name="toNormal">The normal of the target plane.</param>
    /// <returns>The transformed point on the target plane.</returns>
    public static Vector3 ConvertPointBetweenPlanes(Vector3 point, Vector3 fromNormal, Vector3 toNormal)
    {
        // Normalize the normals
        fromNormal = fromNormal.normalized;
        toNormal = toNormal.normalized;

        // Compute the rotation from the fromNormal to toNormal
        Quaternion rotation = Quaternion.FromToRotation(fromNormal, toNormal);

        // Apply the rotation to the point
        Vector3 rotatedPoint = rotation * point;

        // Project the rotated point onto the new plane to remove any offset from the plane
        float distanceToPlane = Vector3.Dot(toNormal, rotatedPoint);
        Vector3 projectedPoint = rotatedPoint - toNormal * distanceToPlane;

        return projectedPoint;
    }
    #endregion


    /// <summary>
    /// Changes the alpha value of a color.
    /// </summary>
    public static Color ChangeAlpha(this Color c, float a) => new(c.r, c.g, c.b, a);
}
