using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using Unity.VisualScripting;

public class IOTest : MonoBehaviour
{

    [Header("Definitions")]
    [SerializeField] private int serialBaud = 9600;
    [SerializeField] private string comChannel = "COM4";

    [Header("Settings")]
    [SerializeField] private int readTimeOut = 1000;
    [SerializeField] private int writeTimeOut = 1000;

    private SerialPort data_stream;
    private string recivedstring;

    private bool portConnected = true;



    private void Start()
    {
        OpenDatastream();
        StartCoroutine(WriteDataStream());
        //StartCoroutine(ReadDataStream()); //Reading muss be in a differned thread, or Unity freezes to death
    }

    private void FixedUpdate()
    {
        if (!portConnected)
        {
            return;
        }

        //Nothing rn...
    }

    private void OnDisable()
    {
        //Just to be sure
        StopCoroutine(WriteDataStream());
        StopCoroutine(ReadDataStream());
    }

    public void OpenDatastream()
    {
        try
        {
            data_stream = new SerialPort(comChannel, serialBaud);
            data_stream.Open();
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Cant open Datastream on port " + comChannel);
            portConnected = false;
        }

        if (data_stream != null)
        {
            data_stream.ReadTimeout = readTimeOut;
            data_stream.WriteTimeout = writeTimeOut;
        }
    }

    IEnumerator ReadDataStream()
    {
        if (portConnected)
            while (true)
            {
                //Reading
                recivedstring = "null";
                try
                {
                    if (data_stream.IsOpen && data_stream.BytesToRead > 0)
                        recivedstring = data_stream.ReadLine();
                    else
                        throw new System.TimeoutException();
                    Debug.Log(recivedstring);
                }
                catch (System.TimeoutException)
                {
                    Debug.LogWarning($"TimeoutException: ReadLine on {comChannel} failed.");
                }

                yield return null;
            }
    }

    IEnumerator WriteDataStream()
    {
        bool alternate = false;
        if (portConnected)
            while (true)
            {
                yield return new WaitForSeconds(2f);

                //Writing
                try
                {
                    if (alternate)
                    {
                        data_stream.WriteLine("ON");
                        print("Wrote Line ON!");
                    }
                    else
                    {
                        data_stream.WriteLine("OFF");
                        print("Wrote Line OFF!");
                    }

                    alternate = !alternate;
                }
                catch (System.TimeoutException)
                {
                    Debug.LogWarning($"TimeoutException: WriteLine on {comChannel} failed.");
                }
            }
    }

    public void OnClickRetry()
    {
        Start();
    }
}
