using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;

public class RocketTestCases : MonoBehaviour
{
    public enum TestCaseType { One, Two, Three, Four }

    [Header("CSV File")]
    [SerializeField] private TextAsset csvFileTestCases;

    [Header("Data")]
    [SerializeField] private TestCasesData testCasesData;
    [SerializeField] private SimulatedCasesData simulatedCasesData;
    [SerializeField] private DifferenceCasesData differenceCasesData;

    [Header("Objects")]
    [SerializeField] private RocketPhysics rocketPhysics;
    [SerializeField] private GameObject[] testCasesUIScenes;
    [SerializeField] private MultiGraphRenderer[] testCase1Graphs, testCase2Graphs, testCase3Graphs, testCase4Graphs;

    [Header("Settings")]
    [SerializeField] private TestCaseType testCase;
    public TestCaseType TestCase
    {
        get => testCase;
        set
        {
            testCase = value;
            OnTestCaseChanged(value);
            try { ReadAndDisplayCalculatedData(); } catch { }
        }
    }
    float gravity = 9.81f;


    float lastZRotation;
    float totalZRotation;


    private void Start()
    {
        Physics.gravity = Vector3.zero;
        gravity = rocketPhysics.Gravity;
        RocketData.RocketRigidbody.maxAngularVelocity = Mathf.Infinity;

        ReadAndDisplayCalculatedData();

        // Prepare Rocket
        ApplyStartCase();

        //StartCoroutine(GraphEverySecond());

        lastZRotation = transform.eulerAngles.z;
    }
    void ReadAndDisplayCalculatedData()
    {
        // Read
        testCasesData.ClearData();
        simulatedCasesData.ClearData();
        differenceCasesData.ClearData();
        testCasesData.ReadCSV(csvFileTestCases);

        // Display calculated data
        testCase1Graphs[0].ClearPoints(0);
        testCase2Graphs[0].ClearPoints(0);
        testCase3Graphs[0].ClearPoints(0);
        testCase3Graphs[1].ClearPoints(0);
        testCase4Graphs[0].ClearPoints(0);
        testCase4Graphs[1].ClearPoints(0);
        testCase4Graphs[2].ClearPoints(0);

        testCasesData.AddExtraCaseData4(0.1f);

        for (int i = 0; i < testCasesData.time.Count; i++)
        {
            testCase1Graphs[0].AddPoint(0, new Vector2(testCasesData.time[i], testCasesData.testCaseData1[i]));
            testCase2Graphs[0].AddPoint(0, new Vector2(testCasesData.time[i], testCasesData.testCaseData2[i]));
            testCase3Graphs[0].AddPoint(0, new Vector2(testCasesData.time[i], testCasesData.testCaseData3[i].x));
            testCase3Graphs[1].AddPoint(0, new Vector2(testCasesData.time[i], testCasesData.testCaseData3[i].y));
        }

        for (int i = 0; i < testCasesData.extraTestCaseData4.Count; i++)
        {
            testCase4Graphs[0].AddPoint(0, new Vector2(testCasesData.extraTestCaseData4[i].w, testCasesData.extraTestCaseData4[i].x));
            testCase4Graphs[1].AddPoint(0, new Vector2(testCasesData.extraTestCaseData4[i].w, testCasesData.extraTestCaseData4[i].y));
            testCase4Graphs[2].AddPoint(0, new Vector2(testCasesData.extraTestCaseData4[i].w, testCasesData.extraTestCaseData4[i].z));
        }
    }
    void ApplyStartCase()
    {
        switch (testCase)
        {
            case TestCaseType.One:
                rocketPhysics.enabled = false;
                RocketData.RocketRigidbody.mass = 0.3f;
                RocketData.RocketTransform.position = new Vector3(0f, 0f, 0f);
                RocketData.RocketTransform.rotation = Quaternion.identity;
                RocketData.RocketRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                break;

            case TestCaseType.Two:
                rocketPhysics.enabled = false;
                RocketData.RocketRigidbody.mass = 0.3f;
                RocketData.RocketTransform.position = new Vector3(0f, 0f, 0f);
                RocketData.RocketTransform.rotation = Quaternion.identity;
                RocketData.RocketRigidbody.constraints = RigidbodyConstraints.FreezePosition;
                break;

            case TestCaseType.Three:
                rocketPhysics.enabled = false;
                RocketData.RocketRigidbody.mass = 0.3f;
                RocketData.RocketTransform.position = new Vector3(0f, 0f, 0f);
                RocketData.RocketTransform.rotation = Quaternion.Euler(-15f, 0f, 0f);
                RocketData.RocketRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                break;

            case TestCaseType.Four:
                rocketPhysics.enabled = true;
                rocketPhysics.FlowVelocity = 14f;
                rocketPhysics.controlFinPhysics[0].alphaAngleGoal = 0f;
                rocketPhysics.controlFinPhysics[1].alphaAngleGoal = 1f;
                rocketPhysics.controlFinPhysics[2].alphaAngleGoal = -1f;
                rocketPhysics.ignoreForceGravity = true;
                rocketPhysics.ignoreTorquePropeller = true;
                RocketData.RocketRigidbody.mass = 0.3f;
                RocketData.RocketTransform.position = new Vector3(0f, 0f, 0f);
                RocketData.RocketTransform.rotation = Quaternion.identity;
                RocketData.RocketRigidbody.automaticInertiaTensor = true;
                RocketData.RocketRigidbody.constraints = RigidbodyConstraints.None;
                break;

        }
    }

    private void FixedUpdate()
    {
        ApplyConstantCase();
        GraphSimulatedData(Time.timeSinceLevelLoad);
    }
    void ApplyConstantCase()
    {
        Vector3 forceGravity = gravity * RocketData.RocketRigidbody.mass * Vector3.down;
        Vector3 force = new();
        switch (testCase)
        {
            // Force = 4N up + Gravity
            case TestCaseType.One:
                force = 4f * RocketData.RocketTransform.up;
                RocketData.RocketRigidbody.AddForce(force + forceGravity);
                break;

            // Torque = 1.5N
            case TestCaseType.Two:
                force = 1.5f * RocketData.RocketTransform.up;
                RocketData.RocketRigidbody.AddTorque(force);
                break;

            // Force = 4.5N, angled rocket. + Gravity
            case TestCaseType.Three:
                force = Quaternion.AngleAxis(75f, Vector3.forward) * Vector3.right * 4.5f;
                //force = 4.5f * RocketData.RocketTransform.up;
                RocketData.RocketRigidbody.AddForce(force + forceGravity);
                break;
        }
    }
    void GraphSimulatedData(float t)
    {
        if (t <= testCasesData.time.Last() + 0.1f)
        {
            // Read and calculate rotation
            float currentZRotation = RocketData.RocketTransform.eulerAngles.z;
            float deltaZ = Mathf.DeltaAngle(lastZRotation, currentZRotation) * Mathf.Deg2Rad;

            totalZRotation += deltaZ;
            lastZRotation = currentZRotation;

            // Read simulation
            simulatedCasesData.AddLine(
                t,
                RocketData.RocketTransform.position.y,
                totalZRotation,
                new Vector2(
                    RocketData.RocketTransform.position.x,
                    RocketData.RocketTransform.position.y
                ),
                new Vector3(
                    RocketData.acceleration.x,
                    RocketData.acceleration.y,
                    totalZRotation
                )
            );

            // Index of added simulation line
            int i = simulatedCasesData.time.Count - 1;

            // Calculate difference
            differenceCasesData.AddLine(
                t,
                testCasesData.GetValueAtTime(simulatedCasesData.time[i], CasesData.Type.One) - simulatedCasesData.testCaseData1[i],
                testCasesData.GetValueAtTime(simulatedCasesData.time[i], CasesData.Type.Two) - simulatedCasesData.testCaseData2[i],
                testCasesData.GetVector2AtTime(simulatedCasesData.time[i]) - simulatedCasesData.testCaseData3[i],
                testCasesData.GetVector3AtTime(simulatedCasesData.time[i]) - simulatedCasesData.testCaseData4[i]
            );

            // Display
            testCase1Graphs[0].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData1[i]));
            testCase2Graphs[0].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData2[i]));
            testCase3Graphs[0].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData3[i].x));
            testCase3Graphs[1].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData3[i].y));
            testCase4Graphs[0].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData4[i].x));
            testCase4Graphs[1].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData4[i].y));
            testCase4Graphs[2].AddPoint(1, new Vector2(simulatedCasesData.time[i], simulatedCasesData.testCaseData4[i].z));

            testCase1Graphs[1].AddPoint(0, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData1[i]));
            testCase2Graphs[1].AddPoint(0, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData2[i]));
            testCase3Graphs[2].AddPoint(0, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData3[i].x));
            testCase3Graphs[2].AddPoint(1, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData3[i].y));
            testCase4Graphs[3].AddPoint(0, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData4[i].x));
            testCase4Graphs[3].AddPoint(1, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData4[i].y));
            testCase4Graphs[3].AddPoint(2, new Vector2(differenceCasesData.time[i], differenceCasesData.testCaseData4[i].z));
        }
    }

    float lastTime;
    IEnumerator GraphEverySecond()
    {
        while (true)
        {
            GraphSimulatedData(Time.timeSinceLevelLoad);
            lastTime = Time.timeSinceLevelLoad;
            yield return new WaitForSeconds(1f);
        }
    }

    void OnTestCaseChanged(TestCaseType newTestCase)
    {
        testCasesUIScenes[0].SetActive(newTestCase == TestCaseType.One);
        testCasesUIScenes[1].SetActive(newTestCase == TestCaseType.Two);
        testCasesUIScenes[2].SetActive(newTestCase == TestCaseType.Three);
        testCasesUIScenes[3].SetActive(newTestCase == TestCaseType.Four);
    }

    // For inspector
    private void OnValidate()
    {
        TestCase = testCase;
        try { ReadAndDisplayCalculatedData(); } catch { }
    }


    [System.Serializable]
    public class CasesData
    {
        public enum Type { One, Two, ThreeX, ThreeY, FourX, FourY, FourRad }

        public List<float> time = new();
        public List<float> testCaseData1 = new();
        public List<float> testCaseData2 = new();
        public List<Vector2> testCaseData3 = new();
        public List<Vector3> testCaseData4 = new();

        public List<Vector4> extraTestCaseData4 = new();

        public void AddLine(float timeValue, float testCaseData1Value = 0, float testCaseData2Value = 0, Vector2 testCaseData3Value = new(), Vector3 testCaseData4Value = new())
        {
            time.Add(timeValue);
            testCaseData1.Add(testCaseData1Value);
            testCaseData2.Add(testCaseData2Value);
            testCaseData3.Add(testCaseData3Value);
            testCaseData4.Add(testCaseData4Value);
        }

        public void ClearData()
        {
            if (time != null && time.Count > 0)
                time.Clear();
            if (testCaseData1 != null && testCaseData1.Count > 0)
                testCaseData1.Clear();
            if (testCaseData2 != null && testCaseData2.Count > 0)
                testCaseData2.Clear();
            if (testCaseData3 != null && testCaseData3.Count > 0)
                testCaseData3.Clear();
            if (testCaseData4 != null && testCaseData4.Count > 0)
                testCaseData4.Clear();
            if (extraTestCaseData4 != null && extraTestCaseData4.Count > 0)
                extraTestCaseData4.Clear();
        }

        // public float GetValueAtTime(float targetTime, Type type)
        // {
        //     // Ensure we have data
        //     if (time.Count == 0)
        //         return 0f;

        //     List<float> targetList = type switch
        //     {
        //         Type.One => testCaseData1,
        //         Type.Two => testCaseData2,
        //         Type.ThreeX => testCaseData3.Select(v => v.x).ToList(),
        //         Type.ThreeY => testCaseData3.Select(v => v.y).ToList(),
        //         _ => null
        //     };

        //     if (targetList == null || targetList.Count != time.Count)
        //         return 0f; // Invalid type or mismatched data

        //     // Find two closest points
        //     int lowerIndex = -1, upperIndex = -1;
        //     for (int i = 0; i < time.Count - 1; i++)
        //     {
        //         if (time[i] <= targetTime && time[i + 1] >= targetTime)
        //         {
        //             lowerIndex = i;
        //             upperIndex = i + 1;
        //             break;
        //         }
        //     }

        //     // If no valid range is found, return the closest value
        //     if (lowerIndex == -1 || upperIndex == -1)
        //     {
        //         float closestTime = ExtraClass.ClosestValue(targetTime, time);
        //         int index = time.IndexOf(closestTime);
        //         if (index == -1 || index >= targetList.Count)
        //             return 0f;

        //         return targetList[index];
        //     }

        //     // Get values for interpolation
        //     float t0 = time[lowerIndex], t1 = time[upperIndex];
        //     float v0 = targetList[lowerIndex], v1 = targetList[upperIndex];

        //     // Linear interpolation formula: v = v0 + (targetTime - t0) * (v1 - v0) / (t1 - t0)
        //     return v0 + (targetTime - t0) * (v1 - v0) / (t1 - t0);
        // }
        // public Vector2 GetVectorAtTime(float targetTime)
        // {
        //     // Ensure we have data
        //     if (time.Count == 0 || testCaseData3.Count != time.Count)
        //         return Vector2.zero;

        //     // Find two closest points
        //     int lowerIndex = -1, upperIndex = -1;
        //     for (int i = 0; i < time.Count - 1; i++)
        //     {
        //         if (time[i] <= targetTime && time[i + 1] >= targetTime)
        //         {
        //             lowerIndex = i;
        //             upperIndex = i + 1;
        //             break;
        //         }
        //     }

        //     // If no valid range is found, return the closest value
        //     if (lowerIndex == -1 || upperIndex == -1)
        //     {
        //         float closestTime = ExtraClass.ClosestValue(targetTime, time);
        //         int index = time.IndexOf(closestTime);
        //         if (index == -1 || index >= testCaseData3.Count)
        //             return Vector2.zero;

        //         return testCaseData3[index];
        //     }

        //     // Get values for interpolation
        //     float t0 = time[lowerIndex], t1 = time[upperIndex];
        //     Vector2 v0 = testCaseData3[lowerIndex], v1 = testCaseData3[upperIndex];

        //     // Linear interpolation for Vector2 components
        //     float interpolatedX = v0.x + (targetTime - t0) * (v1.x - v0.x) / (t1 - t0);
        //     float interpolatedY = v0.y + (targetTime - t0) * (v1.y - v0.y) / (t1 - t0);

        //     return new Vector2(interpolatedX, interpolatedY);
        // }

        public float GetValueAtTime(float targetTime, Type type)
        {
            if (time.Count == 0)
            {
                Debug.LogWarning($"CasesData.GetValueAtTime({targetTime}, {type}): No data available for interpolation.");
                return 0f;
            }

            List<float> targetList = type switch
            {
                Type.One => testCaseData1,
                Type.Two => testCaseData2,
                Type.ThreeX => testCaseData3.Select(v => v.x).ToList(),
                Type.ThreeY => testCaseData3.Select(v => v.y).ToList(),
                Type.FourX => testCaseData4.Select(v => v.x).ToList(),
                Type.FourY => testCaseData4.Select(v => v.y).ToList(),
                Type.FourRad => testCaseData4.Select(v => v.z).ToList(),
                _ => null
            };

            if (targetList == null || targetList.Count != time.Count)
                return 0f;

            return InterpolateAtTime(targetTime, time, targetList);
        }

        public Vector2 GetVector2AtTime(float targetTime)
        {
            if (time.Count == 0 || testCaseData3.Count != time.Count)
            {
                Debug.LogWarning($"CasesData.GetVector2AtTime({targetTime}): No data available for interpolation.");
                return Vector2.zero;
            }

            float interpolatedX = InterpolateAtTime(targetTime, time, testCaseData3.Select(v => v.x).ToList());
            float interpolatedY = InterpolateAtTime(targetTime, time, testCaseData3.Select(v => v.y).ToList());

            return new Vector2(interpolatedX, interpolatedY);
        }

        public Vector3 GetVector3AtTime(float targetTime)
        {
            if (time.Count == 0)
            {
                Debug.LogWarning($"CasesData.GetVector3AtTime({targetTime}): No data available for interpolation.");
                return Vector3.zero;
            }

            float interpolatedX = InterpolateAtTime(targetTime, extraTestCaseData4.Select(v => v.w).ToList(), extraTestCaseData4.Select(v => v.x).ToList());
            float interpolatedY = InterpolateAtTime(targetTime, extraTestCaseData4.Select(v => v.w).ToList(), extraTestCaseData4.Select(v => v.y).ToList());
            float interpolatedZ = InterpolateAtTime(targetTime, extraTestCaseData4.Select(v => v.w).ToList(), extraTestCaseData4.Select(v => v.z).ToList());

            return new Vector3(interpolatedX, interpolatedY, interpolatedZ);
        }

        private float InterpolateAtTime(float targetTime, List<float> times, List<float> values)
        {
            int lowerIndex = -1, upperIndex = -1;
            for (int i = 0; i < times.Count - 1; i++)
            {
                if (times[i] <= targetTime && times[i + 1] >= targetTime)
                {
                    lowerIndex = i;
                    upperIndex = i + 1;
                    break;
                }
            }

            if (lowerIndex == -1 || upperIndex == -1)
            {
                float closestTime = ExtraClass.ClosestValue(targetTime, times);
                int index = times.IndexOf(closestTime);
                return (index == -1 || index >= values.Count) ? 0f : values[index];
            }

            float t0 = times[lowerIndex], t1 = times[upperIndex];
            float v0 = values[lowerIndex], v1 = values[upperIndex];

            return v0 + (targetTime - t0) * (v1 - v0) / (t1 - t0);
        }
    }


    [System.Serializable]
    public class TestCasesData : CasesData
    {
        // public void ReadTestCasesData(TextAsset csvFile)
        // {
        //     TestCasesData testData = new();
        //     string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        //     if (lines.Length < 3) // Ensure there's enough data
        //     {
        //         time = testData.time;
        //         testCaseData1 = testData.testCaseData1;
        //         testCaseData2 = testData.testCaseData2;
        //         testCaseData3 = testData.testCaseData3;
        //         return;
        //     }

        //     for (int i = 2; i < lines.Length; i++) // Start from line 2 (skip headers)
        //     {
        //         string[] values = ParseCSVLine(lines[i]);

        //         if (values.Length >= 5) // Ensure there are enough columns
        //         {
        //             float time = ParseFloat(values[0]);
        //             float testCase1 = ParseFloat(values[1]);
        //             float testCase2 = ParseFloat(values[2]);

        //             // Handling Vector2 (two separate float values)
        //             float testCase3_x = ParseFloat(values[4]);
        //             float testCase3_y = ParseFloat(values[3]);

        //             testData.time.Add(time);
        //             testData.testCaseData1.Add(testCase1);
        //             testData.testCaseData2.Add(testCase2);
        //             testData.testCaseData3.Add(new Vector2(testCase3_x, testCase3_y));
        //         }
        //     }

        //     // Set
        //     time = testData.time;
        //     testCaseData1 = testData.testCaseData1;
        //     testCaseData2 = testData.testCaseData2;
        //     testCaseData3 = testData.testCaseData3;
        // }

        // string[] ParseCSVLine(string line)
        // {
        //     // Regular expression to match CSV values, handling quotes correctly
        //     MatchCollection matches = Regex.Matches(line, @"(?<=^|,)(\""(.*?)\""|[^,]*)");

        //     List<string> extractedValues = new List<string>();
        //     foreach (Match match in matches)
        //     {
        //         string value = match.Value.Trim('"'); // Remove surrounding quotes
        //         extractedValues.Add(value);
        //     }

        //     return extractedValues.ToArray();
        // }
        // float ParseFloat(string value)
        // {
        //     value = value.Replace("\"", "").Replace(".", "").Replace(",", "."); // Convert German-style commas to dots
        //     return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : 0f;
        // }

        public void ReadCSV(TextAsset csvFile)
        {
            string[] lines = csvFile.text.Split('\n');

            // Start at Line 3 (Index 2)
            for (int i = 2; i < 18; i++)
            {
                string[] values = lines[i].Split(';');
                if (values.Length < 8) continue; // Safety

                try
                {
                    float time = ParseFloat(values[0]);
                    float testCaseData1 = ParseFloat(values[1]);
                    float testCaseData2 = ParseFloat(values[2]);
                    float testCaseData3Y = ParseFloat(values[3]);
                    float testCaseData3X = ParseFloat(values[4]);
                    float testCaseData4Rad = ParseFloat(values[5]);
                    float testCaseData4Y = ParseFloat(values[6]);
                    float testCaseData4X = ParseFloat(values[7]);

                    this.time.Add(time);
                    this.testCaseData1.Add(testCaseData1);
                    this.testCaseData2.Add(testCaseData2);
                    this.testCaseData3.Add(new Vector2(testCaseData3X, testCaseData3Y));
                    this.testCaseData4.Add(new Vector3(testCaseData4X, testCaseData4Y, testCaseData4Rad));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error on parsing on line {i + 1}: {e.Message}");
                }
            }
        }
        float ParseFloat(string value)
        {
            return float.Parse(value.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
        }


        public void AddExtraCaseData4(float step)
        {
            for (float t = 0; t < 15f; t += step)
                extraTestCaseData4.Add(new Vector4(FunctionTestCase4X(t), FunctionTestCase4Y(t), FunctionTestCase4Rad(t), t));
        }

        [Header("Error factors")]
        [SerializeField] float errorfactorRad = 10f;
        [SerializeField] float errorfactorX = -1f;
        [SerializeField] float errorfactorY = 1f;

        public float FunctionTestCase4Rad(float t) => -0.5f * t * t * 0.879075231f /*0.0627910879357029f*/ * errorfactorRad + 0.5f * Mathf.PI;
        public float FunctionTestCase4X(float t) => 3.70682800382367f / 0.3f * Mathf.Cos(FunctionTestCase4Rad(t)) * errorfactorX;
        public float FunctionTestCase4Y(float t) => 3.70682800382367f / 0.3f * Mathf.Sin(FunctionTestCase4Rad(t)) * errorfactorY;
    }

    [System.Serializable]
    public class SimulatedCasesData : CasesData
    {
        // public float TestCaseData2TotalRotation => testCaseData2TotalRotation;
        // private Quaternion testCaseData2LastRotation; 
        // private float testCaseData2TotalRotation;
        // private Quaternion testCaseData4LastRotation; 
        // private float testCaseData4TotalRotation;

        // public void AddTestCaseData2Difference(Quaternion currentRotation)
        // {
        //     AddTestCaseData2TotalRotation(currentRotation);
        // }

        // public void AddLineWithDifferences(float timeValue, float testCaseData1Value = 0, Quaternion testCaseData2AsDiffernce = new(), Vector2 testCaseData3Value = new(), Vector2 testCaseData4ValueXY = new(), Quaternion testCaseData4ValueRot = new())
        // {
        //     time.Add(timeValue);
        //     testCaseData1.Add(testCaseData1Value);
        //     testCaseData2.Add(AddTestCaseData2TotalRotation(testCaseData2AsDiffernce));
        //     testCaseData3.Add(testCaseData3Value);
        //     testCaseData4.Add(new Vector3(testCaseData4ValueXY.x, testCaseData4ValueXY.y, AddTestCaseData4TotalRotation(testCaseData4ValueRot)));
        // }

        // float AddTestCaseData2TotalRotation(Quaternion currentRotation)
        // {
        //     Quaternion deltaRotation = Quaternion.Inverse(testCaseData2LastRotation) * currentRotation;

        //     float deltaY = Mathf.Atan2(2.0f * (deltaRotation.y * deltaRotation.w), 1.0f - 2.0f * (deltaRotation.y * deltaRotation.y));

        //     testCaseData2TotalRotation += deltaY * Mathf.Deg2Rad;

        //     testCaseData2LastRotation = currentRotation;

        //     return testCaseData2TotalRotation;
        // }
        // float AddTestCaseData4TotalRotation(Quaternion currentRotation)
        // {
        //     float deltaRotation = Quaternion.Angle(testCaseData2LastRotation, currentRotation) * Mathf.Deg2Rad;

        //     testCaseData4TotalRotation += deltaRotation * Mathf.Deg2Rad;

        //     testCaseData4LastRotation = currentRotation;

        //     return testCaseData4TotalRotation;
        // }
    }

    [System.Serializable]
    public class DifferenceCasesData : CasesData
    {

    }
}
