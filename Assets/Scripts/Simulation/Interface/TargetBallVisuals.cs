using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBallVisuals : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    private void Update()
    {
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        if (!lineRenderer)
            return;

        lineRenderer.SetPosition(0, RocketData.RocketTransform.position);
        lineRenderer.SetPosition(1, transform.position);
    }
}
