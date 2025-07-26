using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class AirfoilReader : MonoBehaviour
{
    [Header("CSV Files")]
    public TextAsset csvFileNcrit9;
    public TextAsset csvFileNcrit5;

    [Header("Graphs")]
    [SerializeField] private MultiGraphRenderer multiGraphRenderer1;
    [SerializeField] private MultiGraphRenderer multiGraphRenderer2;
    [SerializeField] private MultiGraphRenderer multiGraphRenderer3;

    [Header("Data")]
    public AirfoildData ncrit9Data;
    public AirfoildData ncrit5Data;

    void Awake()
    {
        ncrit9Data = ReadData(csvFileNcrit9);
        ncrit9Data.onConvert += OnDataConvert;
        ncrit5Data = ReadData(csvFileNcrit5);
        ncrit5Data.onConvert += OnDataConvert;
    }

    void Start()
    {
        DisplayData();
    }

    void DisplayData()
    {
        for (int i = 0; i < ncrit9Data.alpha.Count; i++)
        {
            multiGraphRenderer1?.AddPoint(0, new Vector2(ncrit9Data.alpha[i], ncrit9Data.cl[i]));
            multiGraphRenderer2?.AddPoint(0, new Vector2(ncrit9Data.alpha[i], ncrit9Data.cd[i]));
            multiGraphRenderer3?.AddPoint(0, new Vector2(ncrit9Data.alpha[i], ncrit9Data.cm[i]));
        }

        for (int i = 0; i < ncrit5Data.alpha.Count; i++)
        {
            multiGraphRenderer1?.AddPoint(1, new Vector2(ncrit5Data.alpha[i], ncrit5Data.cl[i]));
            multiGraphRenderer2?.AddPoint(1, new Vector2(ncrit5Data.alpha[i], ncrit5Data.cd[i]));
            multiGraphRenderer3?.AddPoint(1, new Vector2(ncrit5Data.alpha[i], ncrit5Data.cm[i]));
        }
    }

    public void OnDataConvert(float fromValue, AirfoildData.ValueType fromType, float toValue, AirfoildData.ValueType toType)
    {
        if (toType == AirfoildData.ValueType.Cl)
        {
            multiGraphRenderer1?.TogglePointReader(true);
            multiGraphRenderer1?.SetPointReader(new Vector2(fromValue, toValue));
        }
        else if (toType == AirfoildData.ValueType.Cd)
        {
            multiGraphRenderer2?.TogglePointReader(true);
            multiGraphRenderer2?.SetPointReader(new Vector2(fromValue, toValue));
        }
    }

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
    AirfoildData ReadData(TextAsset csvFile)
    {
        if (csvFile == null)
            return null;

        AirfoildData airfoildData = new();

        string[] dataLines = ReadCSV(csvFile.text);
        for (int i = 1; i < dataLines.Length; i++) //Skip header row
        {
            string[] values = dataLines[i].Split(",");

            if (values.Length == 7)
            {
                airfoildData.alpha.Add(float.Parse(values[0], CultureInfo.InvariantCulture));
                airfoildData.cl.Add(float.Parse(values[1], CultureInfo.InvariantCulture));
                airfoildData.cd.Add(float.Parse(values[2], CultureInfo.InvariantCulture));
                airfoildData.cdp.Add(float.Parse(values[3], CultureInfo.InvariantCulture));
                airfoildData.cm.Add(float.Parse(values[4], CultureInfo.InvariantCulture));
                airfoildData.topXtr.Add(float.Parse(values[5], CultureInfo.InvariantCulture));
                airfoildData.botXtr.Add(float.Parse(values[6], CultureInfo.InvariantCulture));
            }
        }

        return airfoildData;
    }

}

[System.Serializable]
public class AirfoildData
{
    public enum ValueType { Alpha, Cl, Cd, Cdp, Cm, TopXtr, BotXtr }

    public List<float> alpha = new();
    public List<float> cl = new();
    public List<float> cd = new();
    public List<float> cdp = new();
    public List<float> cm = new();
    public List<float> topXtr = new();
    public List<float> botXtr = new();

    public Dictionary<ValueType, List<float>> valueMap;

    public System.Action<float, ValueType, float, ValueType> onConvert;

    public AirfoildData()
    {
        alpha = new();
        cl = new();
        cd = new();
        cdp = new();
        cm = new();
        topXtr = new();
        botXtr = new();
    }

    public void UpdateValueMap()
    {
        valueMap = new()
        {
            { ValueType.Alpha, alpha },
            { ValueType.Cl, cl },
            { ValueType.Cd, cd },
            { ValueType.Cdp, cdp },
            { ValueType.Cm, cm },
            { ValueType.TopXtr, topXtr },
            { ValueType.BotXtr, botXtr }
        };
    }

    // public float ConvertValue(float value, ValueType from, ValueType to)
    // {
    //     UpdateValueMap();

    //     // Invalid ValueType
    //     if (!valueMap.ContainsKey(from) || !valueMap.ContainsKey(to))
    //     {
    //         onConvert?.Invoke(value, from, 0f, to);
    //         return 0f;
    //     }

    //     List<float> fromList = valueMap[from];
    //     List<float> toList = valueMap[to];

    //     // Convert fromType to toType via index
    //     int index = fromList.IndexOf(ExtraClass.ClosestValue(value, fromList));
    //     if (index == -1 || index >= toList.Count) // Avoid out-of-bounds errors
    //     {
    //         onConvert?.Invoke(value, from, 0f, to);
    //         return 0f;
    //     }
    //     float result = toList[index];

    //     // Return result and invoke onConvert
    //     onConvert?.Invoke(value, from, result, to);
    //     return result;
    // }

    public float ConvertValue(float value, ValueType from, ValueType to)
    {
        UpdateValueMap();

        // Invalid ValueType
        if (!valueMap.ContainsKey(from) || !valueMap.ContainsKey(to))
        {
            onConvert?.Invoke(value, from, 0f, to);
            return 0f;
        }

        List<float> fromList = valueMap[from];
        List<float> toList = valueMap[to];

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
            if (index == -1 || index >= toList.Count)
            {
                onConvert?.Invoke(value, from, 0f, to);
                return 0f;
            }
            float result = toList[index];
            onConvert?.Invoke(value, from, result, to);
            return result;
        }

        // Get values for interpolation
        float x0 = fromList[lowerIndex], x1 = fromList[upperIndex];
        float y0 = toList[lowerIndex], y1 = toList[upperIndex];

        // Linear interpolation formula: y = y0 + (value - x0) * (y1 - y0) / (x1 - x0)
        float interpolatedValue = y0 + (value - x0) * (y1 - y0) / (x1 - x0);

        // Return interpolated result and invoke event
        onConvert?.Invoke(value, from, interpolatedValue, to);
        return interpolatedValue;
    }

    public float GetMaxValue(ValueType checkType) => GetMaxValue(checkType, checkType);
    public float GetMaxValue(ValueType checkType, ValueType outputType)
    {
        UpdateValueMap();

        // Invalid ValueType
        if (!valueMap.ContainsKey(checkType) || !valueMap.ContainsKey(outputType))
            return 0f;

        // Get max
        float maxValue = valueMap[checkType].Max();

        // Convert to outputType via index
        int index = valueMap[checkType].IndexOf(maxValue);
        if (index == -1 || index >= valueMap[outputType].Count) // Avoid out-of-bounds errors
            return 0f;

        // Return
        return valueMap[outputType][index];
    }

    public float GetMinValue(ValueType checkType) => GetMinValue(checkType, checkType);
    public float GetMinValue(ValueType checkType, ValueType outputType)
    {
        UpdateValueMap();

        // Invalid ValueType
        if (!valueMap.ContainsKey(checkType) || !valueMap.ContainsKey(outputType))
            return 0f;

        // Get min
        List<float> l = valueMap[checkType];
        float maxValue = l.Min();

        // Convert to outputType via index
        int index = valueMap[checkType].IndexOf(maxValue);
        if (index == -1 || index >= valueMap[outputType].Count) // Avoid out-of-bounds errors
            return 0f;

        // Return
        return valueMap[outputType][index];
    }
}
