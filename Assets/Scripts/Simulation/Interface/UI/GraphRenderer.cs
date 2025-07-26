using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//How to add a point: graphRenderer.points.Add(new Vector2(Time.time, value));

public class GraphRenderer : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private UILineRenderer uILineRenderer;
    [SerializeField] private Transform textsParent;
    List<TMP_Text> texts = new();

    [Header("Settings")]
    [SerializeField] private bool autoRemovePastX = true;
    [SerializeField] private bool autoShiftXAxis = true, autoShiftYAxis = true;
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
    public List<Vector2> points;
    Vector2 areaSize;

    private void Awake()
    {
        //Get areaSize
        areaSize = transform.Find("Area").GetComponent<RectTransform>().rect.size;

        //Get texts
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

    private void Start()
    {
        UpdateTexts();
        UpdateLineRenderer();
    }

    private void FixedUpdate()
    {
        UpdateLineRenderer();

        if (points != null && points.Count > 0)
        {
            if (autoRemovePastX)
                AutoRemovePastX();

            if (autoShiftXAxis)
                AutoShiftXAxis();
            if (autoShiftYAxis)
                AutoShiftYAxis();
        }
    }

    void UpdateTexts()
    {
        if (texts != null && texts.Count >= 6)
            for (int i = 0; i < 6; i++)
            {
                texts[i].text = i switch
                {
                    0 => xAxisValueRange.x.ToString("F1"),                                      //XMin
                    1 => ((xAxisValueRange.x + xAxisValueRange.y) * 0.5f).ToString("F1"),   //XMid
                    2 => xAxisValueRange.y.ToString("F1"),                                      //XMax
                    3 => yAxisValueRange.x.ToString("F1"),                                      //YMin
                    4 => ((yAxisValueRange.x + yAxisValueRange.y) * 0.5f).ToString("F1"),   //YMid
                    5 => yAxisValueRange.y.ToString("F1"),                                      //YMax
                    _ => "?"
                };
            }
    }
    void UpdateLineRenderer()
    {
        //Define ranges
        (float, float)
            valueRangeX = XAxisValueRange,
            valueRangeY = YAxisValueRange,
            uIRangeX = (areaSize.x * -0.5f, areaSize.x * 0.5f),
            uIRangeY = (areaSize.y * -0.5f, areaSize.y * 0.5f);

        //Remap points from value ranges to ui ranges
        List<Vector2> remappedPoints = new();
        if (points != null)
            foreach (Vector2 p in points)
            {
                remappedPoints.Add(new Vector2(
                    Mathf.Clamp(p.x.Remap(valueRangeX, uIRangeX), uIRangeX.Item1, uIRangeX.Item2),
                    Mathf.Clamp(p.y.Remap(valueRangeY, uIRangeY), uIRangeY.Item1, uIRangeY.Item2)
                    ));
            }

        //Apply to LineRenderer
        if (uILineRenderer)
        {
            uILineRenderer.points = remappedPoints.ToArray();
            uILineRenderer.SetAllDirty();
        }
    }

    void AutoRemovePastX()
    {
        points.RemoveAll(p => p.x < XAxisValueRange.Item1);
    }
    void AutoShiftXAxis()
    {
        if (points.Last().x > XAxisValueRange.Item2) //If over
        {
            //Get max x value
            float maxXValue = points[0].x;
            for (int i = 1; i < points.Count; i++)
                maxXValue = Mathf.Max(points[i].x, maxXValue);

            //Calculate offset
            float offset = maxXValue - XAxisValueRange.Item2;

            //Add offset to Range
            XAxisValueRange = (XAxisValueRange.Item1 + offset, XAxisValueRange.Item2 + offset);
        }
    }
    void AutoShiftYAxis()
    {
        if (points.Last().y > YAxisValueRange.Item2) //If over
        {
            //Get max y value
            float maxYValue = points[0].y;
            for (int i = 1; i < points.Count; i++)
                maxYValue = Mathf.Max(points[i].y, maxYValue);

            //Calculate offset
            float offset = maxYValue - YAxisValueRange.Item2;

            //Add offset to Range
            YAxisValueRange = (YAxisValueRange.Item1 + offset, YAxisValueRange.Item2 + offset);
        }
        else if (points.Last().y < YAxisValueRange.Item1) //If under
        {
            //Get min y value
            float minYValue = points[0].y;
            for (int i = 1; i < points.Count; i++)
                minYValue = Mathf.Min(points[i].y, minYValue);

            //Calculate offset
            float offset = minYValue - YAxisValueRange.Item1;

            //Add offset to Range
            YAxisValueRange = (YAxisValueRange.Item1 + offset, YAxisValueRange.Item2 + offset);
        }
    }

    //For Inspector
    private void OnValidate()
    {
        Awake();
        XAxisValueRange = (xAxisValueRange.x, xAxisValueRange.y);
        YAxisValueRange = (yAxisValueRange.x, yAxisValueRange.y);
        UpdateLineRenderer();
    }
}
