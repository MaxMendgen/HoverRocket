using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeSlower : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    [Header("Keybinds")]
    [SerializeField] private KeyCode pauseKey = KeyCode.P;
    [SerializeField] private KeyCode stepKey = KeyCode.Space;

    string state;

    [Header("Info")]
    public bool paused;
    public float currentTimeScale;

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            paused = !paused;
            state = paused ? "Paused" : "Running";
        }

        if (paused)
        {
            if (Input.GetKey(stepKey))
            {
                Time.timeScale = Time.fixedDeltaTime * 5f;
                state = "Stepping";
            }
            else
            {
                Time.timeScale = 0f;
                state = "Paused";
            }
        }
        else
        {
            if (Input.GetKey(stepKey))
            {
                Time.timeScale = 2f;
                state = "Speeding Up";
            }
            else
            {
                Time.timeScale = 1f;
                state = "Running";
            }
        }

        currentTimeScale = Time.timeScale;

        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(Time.timeSinceLevelLoadAsDouble);
        string _timeText = string.Format("{0:D2}:{1:D2}:{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

        timeText.text = $"Time: {_timeText}\nTime Scale: {currentTimeScale:F2}\nState: {state}";
    }
}
