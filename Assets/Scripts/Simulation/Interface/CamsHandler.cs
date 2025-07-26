using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CamsHandler : MonoBehaviour
{
    public enum Mode { View1, View2, View3, View4 }

    [SerializeField] private Transform camsParent;
    [SerializeField] private TMP_Text topCamText, bottomCamText;

    [Header("Keybinds")]
    [SerializeField] private KeyCode keyMode1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode keyMode2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode keyMode3 = KeyCode.Alpha3;
    [SerializeField] private KeyCode keyMode4 = KeyCode.Alpha4;
    [SerializeField] private KeyCode keyZoomIn = KeyCode.LeftShift;

    [Header("Mode")]
    [SerializeField] private Mode viewMode;
    public Mode ViewMode
    {
        get => viewMode;
        set
        {
            viewMode = value;
            UpdateView();
        }
    }


    CamController[] cams;

    void Start()
    {
        TryGettingCams();
    }

    void TryGettingCams()
    {
        try
        {
            cams = new CamController[camsParent.childCount];
            for (int i = 0; i < cams.Length; i++)
                cams[i] = camsParent.GetChild(i).GetComponent<CamController>();
        }
        catch
        {
            Debug.LogWarning("Can't change the Cameras View, couldn't find them.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(keyMode1))
        {
            ViewMode = Mode.View1;
            UpdateView();
        }
        if (Input.GetKeyDown(keyMode2))
        {
            ViewMode = Mode.View2;
            UpdateView();
        }
        if (Input.GetKeyDown(keyMode3))
        {
            ViewMode = Mode.View3;
            UpdateView();
        }
        if (Input.GetKeyDown(keyMode4))
        {
            ViewMode = Mode.View4;
            UpdateView();
        }

        if (Input.GetKeyDown(keyZoomIn))
        {
            UpdateZoom(true);
        }
        else if (Input.GetKeyUp(keyZoomIn))
        {
            UpdateZoom(false);
        }
    }

    void UpdateView()
    {
        if (cams == null || cams.Length == 0)
        {
            TryGettingCams();
        }
        else
        {
            switch (ViewMode)
            {
                case Mode.View1:
                    cams[0].SetActive(true);
                    cams[1].SetActive(true);
                    cams[2].SetActive(false);
                    cams[3].SetActive(false);
                    topCamText.text = cams[0].name;
                    bottomCamText.text = cams[1].name;
                    break;
                case Mode.View2:
                    cams[0].SetActive(true);
                    cams[1].SetActive(false);
                    cams[2].SetActive(false);
                    cams[3].SetActive(true);
                    topCamText.text = cams[0].name;
                    bottomCamText.text = cams[3].name;
                    break;
                case Mode.View3:
                    cams[0].SetActive(false);
                    cams[1].SetActive(true);
                    cams[2].SetActive(true);
                    cams[3].SetActive(false);
                    topCamText.text = cams[2].name;
                    bottomCamText.text = cams[1].name;
                    break;
                case Mode.View4:
                    cams[0].SetActive(false);
                    cams[1].SetActive(false);
                    cams[2].SetActive(true);
                    cams[3].SetActive(true);
                    topCamText.text = cams[2].name;
                    bottomCamText.text = cams[3].name;
                    break;
            }
        }
    }

    void UpdateZoom(bool truth)
    {
        if (cams == null || cams.Length == 0)
        {
            TryGettingCams();
        }
        else
        {
            foreach (CamController c in cams)
                c.ZoomedIn = truth;
        }
    }

    // For inspector
    void OnValidate()
    {
        ViewMode = viewMode;
    }
}
