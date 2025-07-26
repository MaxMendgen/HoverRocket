using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiGraphRenderer : MonoBehaviour
{
    public enum AutoSetAxisType { None, Shift, Scale }

    [Header("Objects")]
    [SerializeField] private List<UILineRenderer> uILineRenderers;
    [SerializeField] private UILineRenderer uIPointReaderLine;
    RectTransform linesAreaRectTrans;
    List<TMP_Text> texts = new();

    [Header("Settings")]
    [SerializeField] private bool autoRemovePastX = true;
    [SerializeField] private AutoSetAxisType autoSetXAxis = AutoSetAxisType.Shift;
    [SerializeField] private AutoSetAxisType autoSetYAxis = AutoSetAxisType.Shift;
    public (float, float) XAxisValueRange
    {
        get => (xAxisValueRange.x, xAxisValueRange.y);
        set
        {
            xAxisValueRange = new Vector2(value.Item1, value.Item2);
            UpdateTexts();
        }
    }
    [SerializeField] private Vector2 xAxisValueRange = new(0, 10);
    public (float, float) YAxisValueRange
    {
        get => (yAxisValueRange.x, yAxisValueRange.y);
        set
        {
            yAxisValueRange = new Vector2(value.Item1, value.Item2);
            UpdateTexts();
        }
    }
    [SerializeField] private Vector2 yAxisValueRange = new(-2, 2);

    [Header("Info")]
    [SerializeField] private Vector2 pointReaderPosition;
    [SerializeField] private bool showPointReader = false;
    public List<List<Vector2>> listOfPoints = new() { new() };
    [SerializeField] private List<Vector2> firstListOfPoints;
    [SerializeField] private List<Vector2> secondListOfPoints;
    Vector2 areaSize;

    float latestXAdded, latestYAdded;

    private void Awake()
    {
        FindObjects();
        GetAreaSize();
    }
    void GetAreaSize()
    {
        // Only works with anchors center middle, why idk
        Vector3[] corners = new Vector3[4];
        linesAreaRectTrans.GetLocalCorners(corners);
        areaSize = new Vector2(corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }
    void FindObjects()
    {
        //Get LinesArea RectTransform for areaSize
        try { linesAreaRectTrans = transform.Find("LinesArea").GetComponent<RectTransform>(); }
        catch { throw new System.NullReferenceException("MultiGraphRenderer can't find 'LinesArea'"); }

        //Get texts
        Transform textsParent = null;
        try { textsParent = transform.Find("Texts"); }
        catch { Debug.LogWarning("MultiGraphRenderer can't find Parent 'Texts'!"); }
        try
        {
            texts = new List<TMP_Text>
            {
                textsParent.Find("XMin").GetComponent<TMP_Text>(),
                textsParent.Find("XMid").GetComponent<TMP_Text>(),
                textsParent.Find("XMax").GetComponent<TMP_Text>(),
                textsParent.Find("YMin").GetComponent<TMP_Text>(),
                textsParent.Find("YMid").GetComponent<TMP_Text>(),
                textsParent.Find("YMax").GetComponent<TMP_Text>()
            };
        }
        catch
        {
            Debug.LogWarning("MultiGraphRenderer couldn't find one or more text objects!");
        }
    }

    private void Start()
    {
        GetAreaSize();
        UpdateTexts();
        UpdateLineRenderers();
    }

    private void FixedUpdate()
    {
        UpdateLineRenderers();
        UpdateAutos();
    }

    void UpdateTexts()
    {
        if (texts != null && texts.Count >= 6)
            for (int i = 0; i < 6; i++)
            {
                texts[i].text = i switch
                {
                    0 => xAxisValueRange.x.ToString("F2"),                                      //XMin
                    1 => ((xAxisValueRange.x + xAxisValueRange.y) * 0.5f).ToString("F2"),   //XMid
                    2 => xAxisValueRange.y.ToString("F2"),                                      //XMax
                    3 => yAxisValueRange.x.ToString("F2"),                                      //YMin
                    4 => ((yAxisValueRange.x + yAxisValueRange.y) * 0.5f).ToString("F2"),   //YMid
                    5 => yAxisValueRange.y.ToString("F2"),                                      //YMax
                    _ => "?"
                };
            }
    }
    void UpdateLineRenderers()
    {
        //Define ranges
        (float, float)
            valueRangeX = XAxisValueRange,
            valueRangeY = YAxisValueRange,
            uIRangeX = (areaSize.x * -0.5f, areaSize.x * 0.5f),
            uIRangeY = (areaSize.y * -0.5f, areaSize.y * 0.5f);

        //Remap points from value ranges to ui ranges
        List<List<Vector2>> remappedListOfPoints = new();
        if (listOfPoints != null)
        {
            foreach (List<Vector2> points in listOfPoints)
            {
                if (points != null)
                {
                    List<Vector2> remappedPoints = new();
                    foreach (Vector2 p in points)
                    {
                        remappedPoints.Add(new Vector2(
                            Mathf.Clamp(p.x.Remap(valueRangeX, uIRangeX), uIRangeX.Item1, uIRangeX.Item2),
                            Mathf.Clamp(p.y.Remap(valueRangeY, uIRangeY), uIRangeY.Item1, uIRangeY.Item2)
                            ));
                    }
                    remappedListOfPoints.Add(remappedPoints);
                }
            }
        }

        //Apply to LineRenderer
        if (uILineRenderers != null && uILineRenderers.Count > 0)
        {
            for (int i = 0; i < uILineRenderers.Count; i++)
            {
                try
                {
                    uILineRenderers[i].points = remappedListOfPoints[i].ToArray();
                    uILineRenderers[i].SetAllDirty();
                }
                catch { }
            }
        }
    }
    void UpdateAutos()
    {
        if (listOfPoints != null && listOfPoints.Count >= uILineRenderers.Count)
        {
            if (autoRemovePastX)
                AutoRemovePastX();

            if (autoSetXAxis != AutoSetAxisType.None)
                AutoShiftXAxis();
            if (autoSetYAxis != AutoSetAxisType.None)
                AutoSetYAxis();
        }
    }
    void UpdatePointReader()
    {
        //Check existents
        if (!uIPointReaderLine)
            return;

        //Show or hide (skip everything else if hide)
        if (!showPointReader)
        {
            uIPointReaderLine.enabled = false;
            return;
        }
        uIPointReaderLine.enabled = true;

        //Define ranges
        (float, float)
            valueRangeX = XAxisValueRange,
            valueRangeY = YAxisValueRange,
            uIRangeX = (areaSize.x * -0.5f, areaSize.x * 0.5f),
            uIRangeY = (areaSize.y * -0.5f, areaSize.y * 0.5f);

        //Define the points
        List<Vector2> points = new() {
            new(pointReaderPosition.x, YAxisValueRange.Item1),
            pointReaderPosition,
            pointReaderPosition,
            pointReaderPosition,
            pointReaderPosition,
            pointReaderPosition,
            pointReaderPosition,
            pointReaderPosition,
            new(XAxisValueRange.Item1, pointReaderPosition.y)
        };

        //Remap points
        List<Vector2> remappedPoints = new();
        foreach (Vector2 p in points)
        {
            Vector2 v = new(
                Mathf.Clamp(p.x.Remap(valueRangeX, uIRangeX), uIRangeX.Item1, uIRangeX.Item2),
                Mathf.Clamp(p.y.Remap(valueRangeY, uIRangeY), uIRangeY.Item1, uIRangeY.Item2)
            );
            remappedPoints.Add(v);
        }

        //Add center offests
        remappedPoints[1] += new Vector2(0f, -10f);
        remappedPoints[2] += new Vector2(-10f, -10f);
        remappedPoints[3] += new Vector2(-10f, 10f);
        remappedPoints[4] += new Vector2(10f, 10f);
        remappedPoints[5] += new Vector2(10f, -10f);
        remappedPoints[6] += new Vector2(-10f, -10f);
        remappedPoints[7] += new Vector2(-10f, 0f);

        //Apply
        uIPointReaderLine.points = remappedPoints.ToArray();
        uIPointReaderLine.SetAllDirty();
    }

    /// <summary>
    /// Adds a point to the graph
    /// </summary>
    /// <param name="index">Which line the point belongs to</param>
    /// <param name="position">Position of the point</param>
    public void AddPoint(int index, Vector2 position)
    {
        //Throw error if index out of range
        if (index < 0 || index > uILineRenderers.Count)
            throw new System.IndexOutOfRangeException($"AddPoint({index}, {position})");

        //Add
        if (listOfPoints == null || listOfPoints.Count != uILineRenderers.Count) //If null add empty lists
            for (listOfPoints = new(); listOfPoints.Count < uILineRenderers.Count; listOfPoints.Add(new())) { }
        listOfPoints[index].Add(position);

        //Set latest
        if (!listOfPoints.Any(l => l.Any(p => p.x > position.x))) //If all x are less then the new x
            latestXAdded = position.x;
        if (position.x > XAxisValueRange.Item1)
            latestYAdded = position.y;

        UpdateAutos();
    }
    /// <summary>
    /// Clears all points in a graph or to a specified line index 
    /// </summary>
    /// <param name="index">Which line to clear the points (leave -1 for all)</param>
    public void ClearPoints(int index = -1)
    {
        //Throw error if index out of range
        if (index < -1 || index > uILineRenderers.Count)
            throw new System.IndexOutOfRangeException($"ClearPoints({index})");

        //Clear points
        if (index == -1)
        {
            for (int i = 0; i < listOfPoints.Count; i++)
                listOfPoints[i].Clear();
        }
        else
        {
            listOfPoints[index].Clear();
        }

        UpdateAutos();
    }
    /// <summary>
    /// Sets the point readers position
    /// </summary>
    /// <param name="position"></param>
    public void SetPointReader(Vector2 position)
    {
        pointReaderPosition = position;
        UpdatePointReader();
    }
    /// <summary>
    /// Shows or hides point reader
    /// </summary>
    /// <param name="truth"></param>
    public void TogglePointReader(bool truth)
    {
        showPointReader = truth;
        UpdatePointReader();
    }


    void AutoRemovePastX()
    {
        listOfPoints.ForEach(l => l.RemoveAll(p => p.x < XAxisValueRange.Item1));
    }
    void AutoShiftXAxis()
    {
        bool addShiftOffset = autoSetXAxis == AutoSetAxisType.Shift;

        if (latestXAdded > XAxisValueRange.Item2) //If over
        {
            //Calculate offset
            float offset = latestXAdded - XAxisValueRange.Item2;

            //Add offset to Range
            XAxisValueRange = (XAxisValueRange.Item1 + (addShiftOffset ? offset : 0), XAxisValueRange.Item2 + offset);
        }
    }
    void AutoSetYAxis()
    {
        bool addShiftOffset = autoSetYAxis == AutoSetAxisType.Shift;

        if (latestYAdded > YAxisValueRange.Item2) //If over
        {
            //Calculate offset
            float offset = latestYAdded - YAxisValueRange.Item2;

            //Add offset to Range
            YAxisValueRange = (YAxisValueRange.Item1 + (addShiftOffset ? offset : 0), YAxisValueRange.Item2 + offset);
        }
        else if (latestYAdded < YAxisValueRange.Item1) //If under
        {
            //Calculate offset
            float offset = latestYAdded - YAxisValueRange.Item1;

            //Add offset to Range
            YAxisValueRange = (YAxisValueRange.Item1 + offset, YAxisValueRange.Item2 + (addShiftOffset ? offset : 0));
        }
    }

    //For Inspector
    private void OnValidate()
    {
        try { FindObjects(); } catch { }
        GetAreaSize();
        XAxisValueRange = (xAxisValueRange.x, xAxisValueRange.y);
        YAxisValueRange = (yAxisValueRange.x, yAxisValueRange.y);
        UpdateLineRenderers();
        UpdatePointReader();

        try
        {
            firstListOfPoints = listOfPoints[0];
            secondListOfPoints = listOfPoints[1];
        }
        catch { }
    }
}
