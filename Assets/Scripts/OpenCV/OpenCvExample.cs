using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;
using OpenCvSharp.Face;


public class OpenCvExample : MonoBehaviour
{
    RawImage rawimage;

    [SerializeField] private bool useOpenCV;

    WebCamTexture webcamTexture;

    void Start()
    {
        webcamTexture = new WebCamTexture();
        rawimage = GetComponent<RawImage>();

        if(!useOpenCV){
            rawimage.texture = webcamTexture;
            rawimage.material.mainTexture = webcamTexture;
        }

        

        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            //Debug.Log(devices[i].name);
        }

        webcamTexture.Play();
        
        



        
    }

    void Update() {
        Mat image = OpenCvSharp.Unity.TextureToMat(webcamTexture);

        //var transform = gameObject.GetComponent<UnityEngine.RectTransform>();
        //transform.sizeDelta = new UnityEngine.Vector2(image.Width, image.Height);

        // Load an image using OpenCvSharp

        //print(OpenCvSharp.Unity.MatToTexture(image));
        //print(WebCamTexture.devices);
        
        if(useOpenCV){
            rawimage.texture = OpenCvSharp.Unity.MatToTexture(image);
            rawimage.material.mainTexture = OpenCvSharp.Unity.MatToTexture(image);
        }
    }
}