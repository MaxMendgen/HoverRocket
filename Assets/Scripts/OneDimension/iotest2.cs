using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

public class iotest2 : MonoBehaviour
{

    [Header("ComDefines")]
    [SerializeField] private string comChannel = "COM4";

    private SerialPort data_stream;
    private string recivedstring;

    private bool portConnected = true;

    [HideInInspector] public string dataOut;

    public void SetDataOut(string dataOut_) {
        dataOut = dataOut_;
    }




    // Start is called before the first frame update
    private void Start()
    {
        try
        {
            data_stream =  new SerialPort(comChannel, 19200);
            data_stream.Open();
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Cant open Datastream on port " + comChannel);
            portConnected = false;
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!portConnected)
        {
            recivedstring = "No open Port!"; 

            return;
        }
            
        //only applys if port connenctect

        recivedstring = data_stream.ReadLine();

        print(recivedstring);

        data_stream.WriteLine(dataOut);  


    }
}
