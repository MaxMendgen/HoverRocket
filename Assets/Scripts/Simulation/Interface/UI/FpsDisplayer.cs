using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FpsDisplayer : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float iterateTime = 0.1f;

    float currentFps, lastFps, fixedFps;

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
