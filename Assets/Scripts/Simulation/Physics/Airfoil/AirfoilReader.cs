using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

// TODO:
// - In ControlFinPhysics.cs and RockPhysics.cs, add setting to change between Ncrit9 and Ncrit5
// - Implement code to graph data

public enum PolarNcritType { Ncrit9, Ncrit5 }
public enum PolarValueType { Alpha, Cl, Cd, Cdp, Cm, TopXtr, BotXtr }

/// <summary>
/// Reads airfoil data from CSV files and converts between different polar values. (Note: It is assumed that alpha values have the same spacing in all CSV files)
/// </summary>
public class AirfoilReader : MonoBehaviour
{
    [Header("CSV Files")]
    public DataFile[] csvFilesNcrit9 = new DataFile[3];
    public DataFile[] csvFilesNcrit5 = new DataFile[3];

    [Header("Graphs")]
    public MultiGraphRenderer multiGraphRendererCl;
    public MultiGraphRenderer multiGraphRendererCd;
    public MultiGraphRenderer multiGraphRendererCm;

    [Header("Data")]
    public MultiplePolarData polarDataNcrit9 = new(PolarNcritType.Ncrit9);
    public MultiplePolarData polarDataNcrit5 = new(PolarNcritType.Ncrit5);

    void Awake()
    {
        // Clear all data
        polarDataNcrit9.ClearPolarDatas();
        polarDataNcrit5.ClearPolarDatas();

        // Read all data from CSV files
        ReadAndAddAllData();

        // Set up event listeners
        polarDataNcrit9.onConvert += OnDataConvert;
        polarDataNcrit5.onConvert += OnDataConvert;
    }

    void Start()
    {
        DisplayData();
    }

    void ReadAndAddAllData()
    {
        foreach (DataFile dataFile in csvFilesNcrit9)
        {
            SinglePolarData polarData = dataFile.ReadData();
            if (polarData != null)
                polarDataNcrit9.AddPolarData(polarData);
        }
        foreach (DataFile dataFile in csvFilesNcrit5)
        {
            SinglePolarData polarData = dataFile.ReadData();
            if (polarData != null)
                polarDataNcrit5.AddPolarData(polarData);
        }
    }

    void DisplayData()
    {
        // Clear all graphs
        multiGraphRendererCl?.ClearPoints();
        multiGraphRendererCd?.ClearPoints();
        multiGraphRendererCm?.ClearPoints();

        // Display data for Ncrit9
        for (int i = 0; i < polarDataNcrit9.polarDatas.Count; i++)
        {
            SinglePolarData polarData = polarDataNcrit9.polarDatas[i];
            for (int j = 0; j < polarData[PolarValueType.Alpha].Count; j++)
            {
                float alpha = polarData[PolarValueType.Alpha][j];
                float cl = polarData[PolarValueType.Cl][j];
                float cd = polarData[PolarValueType.Cd][j];
                float cm = polarData[PolarValueType.Cm][j];

                multiGraphRendererCl?.AddPoint(i, new Vector2(alpha, cl));
                multiGraphRendererCd?.AddPoint(i, new Vector2(alpha, cd));
                multiGraphRendererCm?.AddPoint(i, new Vector2(alpha, cm));
            }
        }
    }

    bool onDataConvertClIsScheduled = false, onDataConvertCdIsScheduled = false;
    Vector2 lastClPoint, lastCdPoint;

    // Note: this function gets called a LOT of times, so be careful with performance
    void OnDataConvert(float fromValue, PolarValueType fromType, float toValue, PolarValueType toType)
    {
        if (toType == PolarValueType.Cl)
        {
            lastClPoint = new Vector2(fromValue, toValue);

            if (!onDataConvertClIsScheduled)
            {
                onDataConvertClIsScheduled = true;
                StartCoroutine(DeferredRenderOnDataConvertCl());
            }
        }
        else if (toType == PolarValueType.Cd)
        {
            lastCdPoint = new Vector2(fromValue, toValue);

            if (!onDataConvertCdIsScheduled)
            {
                onDataConvertCdIsScheduled = true;
                StartCoroutine(DeferredRenderOnDataConvertCd());
            }
        }
    }
    IEnumerator DeferredRenderOnDataConvertCl()
    {
        yield return new WaitForEndOfFrame(); // Defer to end of frame
        onDataConvertClIsScheduled = false;

        multiGraphRendererCl?.TogglePointReader(true);
        multiGraphRendererCl?.SetPointReader(lastClPoint);
    }
    IEnumerator DeferredRenderOnDataConvertCd()
    {
        yield return new WaitForEndOfFrame(); // Defer to end of frame
        onDataConvertCdIsScheduled = false;

        multiGraphRendererCd?.TogglePointReader(true);
        multiGraphRendererCd?.SetPointReader(lastCdPoint);
    }

    void OnValidate()
    {
        polarDataNcrit9.UpdateInspector();
        polarDataNcrit5.UpdateInspector();
    }


    [System.Serializable]
    public class DataFile
    {
        public TextAsset csvFile;
        public int raynoldsNumber;

        string[] ReadCSV(string csvContent)
        {
            using StringReader reader = new(csvContent);
            List<string> lines = new();
            bool dataStart = false;

            string line;
            while ((line = reader.ReadLine()) != null) //Iterates through every Line
            {
                //Detect header and start processing
                if (line.StartsWith("Alpha,"))
                    dataStart = true;

                if (dataStart && !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }

            return lines.ToArray();
        }

        public SinglePolarData ReadData()
        {
            if (csvFile == null)
                return null;

            SinglePolarData polarData = new(raynoldsNumber);

            string[] dataLines = ReadCSV(csvFile.text);
            for (int i = 1; i < dataLines.Length; i++) //Skip header row
            {
                string[] values = dataLines[i].Split(",");

                if (values.Length == 7)
                {
                    polarData.AddValue(PolarValueType.Alpha, float.Parse(values[0], CultureInfo.InvariantCulture));
                    polarData.AddValue(PolarValueType.Cl, float.Parse(values[1], CultureInfo.InvariantCulture));
                    polarData.AddValue(PolarValueType.Cd, float.Parse(values[2], CultureInfo.InvariantCulture));
                    polarData.AddValue(PolarValueType.Cdp, float.Parse(values[3], CultureInfo.InvariantCulture));
                    polarData.AddValue(PolarValueType.Cm, float.Parse(values[4], CultureInfo.InvariantCulture));
                    polarData.AddValue(PolarValueType.TopXtr, float.Parse(values[5], CultureInfo.InvariantCulture));
                    polarData.AddValue(PolarValueType.BotXtr, float.Parse(values[6], CultureInfo.InvariantCulture));
                }
            }

            return polarData;
        }
    }
}

[System.Serializable]
public class SinglePolarData
{
    public int raynoldsNumber;
    public Dictionary<PolarValueType, List<float>> ValueMap
    {
        get => valueMap;
        set
        {
            valueMap = value;
            UpdateInspector();
        }
    }
    private Dictionary<PolarValueType, List<float>> valueMap;

    [SerializeField] private InspectableValueMap _inspectableValueMap; // For inspector display

    public System.Action<float, PolarValueType, float, PolarValueType> onConvert;

    public List<float> this[PolarValueType type] => ValueMap[type];

    public SinglePolarData(int raynoldsNumber)
    {
        this.raynoldsNumber = raynoldsNumber;
        // Initialize ValueMap with all possible PolarValueTypes
        ValueMap = System.Enum.GetValues(typeof(PolarValueType))
            .Cast<PolarValueType>()
            .ToDictionary(type => type, type => new List<float>());
    }
    public SinglePolarData(int raynoldsNumber, List<PolarValueType> types)
    {
        this.raynoldsNumber = raynoldsNumber;
        ValueMap = new();
        foreach (PolarValueType type in types)
            ValueMap[type] = new List<float>();
    }

    public void AddValue(PolarValueType type, float value)
    {
        if (ValueMap.ContainsKey(type))
            ValueMap[type].Add(value);
    }
    public void ClearValues()
    {
        foreach (var key in ValueMap.Keys)
            ValueMap[key].Clear();
    }

    public float ConvertValue(float value, PolarValueType from, PolarValueType to)
    {
        // Ensure valid ValueType
        if (!ValueMap.ContainsKey(from) || !ValueMap.ContainsKey(to))
        {
            onConvert?.Invoke(value, from, 0f, to);
            return 0f;
        }

        List<float> fromList = ValueMap[from];
        List<float> toList = ValueMap[to];

        // Ensure valid input
        if (fromList.Count == 0 || toList.Count == 0 || fromList.Count != toList.Count)
        {
            onConvert?.Invoke(value, from, 0f, to);
            return 0f;
        }

        // Find two closest points for interpolation
        int lowerIndex = -1, upperIndex = -1;
        for (int i = 0; i < fromList.Count - 1; i++)
        {
            if (fromList[i] <= value && fromList[i + 1] >= value)
            {
                lowerIndex = i;
                upperIndex = i + 1;
                break;
            }
        }

        // If no valid range is found, return the closest value
        if (lowerIndex == -1 || upperIndex == -1)
        {
            float closestValue = ExtraClass.ClosestValue(value, fromList);
            int index = fromList.IndexOf(closestValue);

            float result = 0f;
            if (index != -1 && index < toList.Count)
                result = toList[index];
            else
                Debug.LogError($"Closest value not found in the list for {from} to {to}. Value: {value}");

            onConvert?.Invoke(value, from, result, to);
            return result;
        }

        // Linearly interpolate between the two points
        float x0 = fromList[lowerIndex], x1 = fromList[upperIndex];
        float y0 = toList[lowerIndex], y1 = toList[upperIndex];

        float percentage = (value - x0) / (x1 - x0);
        float interpolatedValue = Mathf.Lerp(y0, y1, percentage);

        // Return interpolated result
        onConvert?.Invoke(value, from, interpolatedValue, to);
        return interpolatedValue;
    }

    public float GetMaxValue(PolarValueType checkType) => GetMaxValue(checkType, checkType);
    public float GetMaxValue(PolarValueType checkType, PolarValueType outputType)
    {
        // Invalid ValueType
        if (!ValueMap.ContainsKey(checkType) || !ValueMap.ContainsKey(outputType))
            return 0f;

        // Get max
        float maxValue = ValueMap[checkType].Max();

        // Convert to outputType via index
        int index = ValueMap[checkType].IndexOf(maxValue);
        if (index == -1 || index >= ValueMap[outputType].Count) // Avoid out-of-bounds errors
            return 0f;

        // Return
        return ValueMap[outputType][index];
    }

    public float GetMinValue(PolarValueType checkType) => GetMinValue(checkType, checkType);
    public float GetMinValue(PolarValueType checkType, PolarValueType outputType)
    {
        // Invalid ValueType
        if (!ValueMap.ContainsKey(checkType) || !ValueMap.ContainsKey(outputType))
            return 0f;

        // Get min
        List<float> l = ValueMap[checkType];
        float maxValue = l.Min();

        // Convert to outputType via index
        int index = ValueMap[checkType].IndexOf(maxValue);
        if (index == -1 || index >= ValueMap[outputType].Count) // Avoid out-of-bounds errors
            return 0f;

        // Return
        return ValueMap[outputType][index];
    }

    public void UpdateInspector()
    {
        _inspectableValueMap = new InspectableValueMap(ValueMap);
    }

    [System.Serializable]
    public class InspectableValueMap
    {
        public List<PairValue> valueMap;

        public InspectableValueMap(Dictionary<PolarValueType, List<float>> valueMap)
        {
            this.valueMap = new();
            foreach (var kvp in valueMap)
                this.valueMap.Add(new PairValue(kvp.Key, kvp.Value));
        }

        [System.Serializable]
        public class PairValue
        {
            public PolarValueType type;
            public List<float> values;

            public PairValue(PolarValueType type, List<float> values)
            {
                this.type = type;
                this.values = values;
            }
        }
    }
}

[System.Serializable]
public class MultiplePolarData
{
    public PolarNcritType polarNcritType;
    public List<SinglePolarData> polarDatas;

    public System.Action<float, PolarValueType, float, PolarValueType> onConvert;

    public MultiplePolarData(PolarNcritType polarNcritType)
    {
        this.polarNcritType = polarNcritType;
        polarDatas = new();
    }

    public void AddPolarData(SinglePolarData polarData)
    {
        if (polarData != null)
            polarDatas.Add(polarData);
    }
    public void ClearPolarDatas()
    {
        foreach (var polarData in polarDatas)
            polarData.ClearValues();
    }

    public SinglePolarData GetSinglePolarData(int raynoldsNumber) => GetSinglePolarData(raynoldsNumber, System.Enum.GetValues(typeof(PolarValueType)).Cast<PolarValueType>().ToList());
    public SinglePolarData GetSinglePolarData(int raynoldsNumber, List<PolarValueType> types)
    {
        // Find the two closest polar data sets
        int lowerIndex = -1, upperIndex = -1;
        for (int i = 0; i < polarDatas.Count - 1; i++)
        {
            if (polarDatas[i].raynoldsNumber <= raynoldsNumber && polarDatas[i + 1].raynoldsNumber >= raynoldsNumber)
            {
                lowerIndex = i;
                upperIndex = i + 1;
                break;
            }
        }

        // If no valid range is found, return the closest polar data
        if (lowerIndex == -1 && upperIndex == -1)
        {
            // Find the closest polar data set
            float closestRaynoldsNumber = ExtraClass.ClosestValue(raynoldsNumber, polarDatas.Select(d => d.raynoldsNumber).ToList());
            int index = polarDatas.FindIndex(d => d.raynoldsNumber == closestRaynoldsNumber);
            if (index != -1)
                return polarDatas[index];
            else
            {
                Debug.LogError($"Closest polar data not found for Raynolds number: {raynoldsNumber}");
                return null; // No valid polar data found
            }
        }

        // Interpolate between the two polar data sets
        SinglePolarData lowerPolarData = polarDatas[lowerIndex];
        SinglePolarData upperPolarData = polarDatas[upperIndex];

        float percentage = (float)(raynoldsNumber - lowerPolarData.raynoldsNumber) / (upperPolarData.raynoldsNumber - lowerPolarData.raynoldsNumber);

        SinglePolarData result = new(raynoldsNumber, types);
        foreach (PolarValueType type in types)
        {
            // Interpolate the values for the given type
            List<float> lowerValues = lowerPolarData[type];
            List<float> upperValues = upperPolarData[type];

            List<float> interpolatedValues = InterpolateBetweenLists(lowerValues, upperValues, percentage);

            result.ValueMap[type] = interpolatedValues;
        }

        // Return the interpolated result
        return result;
    }
    List<float> InterpolateBetweenLists(List<float> listA, List<float> listB, float percentage)
    {
        if (listA == null || listB == null)
            throw new System.ArgumentNullException("One of the lists is null.");

        // Offset shorter list to center, then fill with closest values. Ensure both lists are the same length
        if (listA.Count < listB.Count)
        {
            int offset = (listB.Count - listA.Count) / 2;
            for (int i = 0; i < offset; i++)
            {
                listA.Insert(0, ExtraClass.ClosestValue(listB[i], listA));
                listA.Add(ExtraClass.ClosestValue(listB[listB.Count - 1 - i], listA));
            }

        }
        else if (listB.Count < listA.Count)
        {
            int offset = (listA.Count - listB.Count) / 2;
            for (int i = 0; i < offset; i++)
            {
                listB.Insert(0, ExtraClass.ClosestValue(listA[i], listB));
                listB.Add(ExtraClass.ClosestValue(listA[listA.Count - 1 - i], listB));
            }
        }

        // Interpolate between the two lists
        List<float> result = new List<float>(listA.Count);
        for (int i = 0; i < listA.Count; i++)
        {
            float interpolatedValue = Mathf.Lerp(listA[i], listB[i], percentage);
            result.Add(interpolatedValue);
        }

        return result;
    }

    public float ConvertValue(float value, PolarValueType from, PolarValueType to, int raynoldsNumber)
    {
        // Get the polar data for the given Raynolds number
        SinglePolarData polarData = GetSinglePolarData(raynoldsNumber, new List<PolarValueType> { from, to });

        // Add event listener for conversion
        polarData.onConvert += onConvert;

        // Convert value using the polar data
        return polarData.ConvertValue(value, from, to);
    }

    public float GetMaxValue(PolarValueType checkType, int raynoldsNumber) => GetMaxValue(checkType, checkType, raynoldsNumber);
    public float GetMaxValue(PolarValueType checkType, PolarValueType outputType, int raynoldsNumber)
    {
        // Get the polar data for the given Raynolds number
        SinglePolarData polarData = GetSinglePolarData(raynoldsNumber, new List<PolarValueType> { checkType, outputType });

        // Get max value using the polar data
        return polarData.GetMaxValue(checkType, outputType);
    }

    public float GetMinValue(PolarValueType checkType, int raynoldsNumber) => GetMinValue(checkType, checkType, raynoldsNumber);
    public float GetMinValue(PolarValueType checkType, PolarValueType outputType, int raynoldsNumber)
    {
        // Get the polar data for the given Raynolds number
        SinglePolarData polarData = GetSinglePolarData(raynoldsNumber, new List<PolarValueType> { checkType, outputType });

        // Get min value using the polar data
        return polarData.GetMinValue(checkType, outputType);
    }

    public void UpdateInspector()
    {
        foreach (var polarData in polarDatas)
            polarData.UpdateInspector();
    }
}
