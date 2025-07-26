using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FpsDisplayer : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float iterateTime = 0.1f;

    float currentFps, lastFps, fixedFps;

    int countSlowFrames = 0, countSlowFramesMax = 10;

    private void Start()
    {
        StartCoroutine(CoroutineLastFps());
    }

    private void Update()
    {
        if (Time.deltaTime > 0f)
            currentFps = 1 / Time.deltaTime;
        else
            currentFps = 0f;

        fpsText.text = $"Current FPS: {currentFps:F2}\nLast FPS: {lastFps:F2}\nFixed FPS: {fixedFps:F2}";

        // Safety: Break Simulation if FPS is too low
        if (1 / Time.deltaTime < 1f)
        {
            countSlowFrames++;
            if (countSlowFrames >= countSlowFramesMax)
            {
                Debug.LogWarning("FPS is too low! Stopping simulation.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
        else
        {
            countSlowFrames = 0;
        }
    }

    private void FixedUpdate()
    {
        if (Time.fixedDeltaTime > 0f)
            fixedFps = 1 / Time.fixedDeltaTime;
        else
            fixedFps = 0f;
    }

    IEnumerator CoroutineLastFps()
    {
        while (true)
        {
            lastFps = 1 / Time.deltaTime;
            yield return new WaitForSeconds(iterateTime);
        }
    }
}
