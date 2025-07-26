using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using ObsoleteScripts;


public class RotatePear : MonoBehaviour
{
    [SerializeField] private float targetPosition; 

    [SerializeField] private PIDControllerValues pIDController;

    [SerializeField] private iotest2 ioTest2;
    private Transform ball;

    // Start is called before the first frame update
    void Awake()
    {
        ball = GameObject.Find("Ball").transform;
        ioTest2 = GetComponent<iotest2>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.transform.Rotate(0,0,-ObsoleteScripts.PIDController.PID(Time.deltaTime, ball.position.x, targetPosition, pIDController));
    
        ioTest2.SetDataOut("0");
    }
}
