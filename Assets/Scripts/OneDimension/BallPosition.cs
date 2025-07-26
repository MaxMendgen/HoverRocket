using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BallPosition : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;

    private Transform ball;
    // Start is called before the first frame update
    void Awake()
    {
        ball = GameObject.Find("Ball").transform;
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = $"Ball Position: {ball.position.x:F2}";
    }
}
