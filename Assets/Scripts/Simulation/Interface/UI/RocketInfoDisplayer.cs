using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RocketInfoDisplayer : MonoBehaviour
{
    [Header("Text Objects")]
    [SerializeField] private Transform positionTexts;
    [SerializeField] private Transform rotationTexts, linearVelocityTexts, angularVelocityTexts;
    [SerializeField] private List<TMP_Text> _positionTexts, _rotationTexts, _linearVelocityTexts, _angularVelocityTexts;

    private void Awake()
    {
        //Get text components
        _positionTexts.Add(positionTexts.Find("X").GetComponent<TMP_Text>());
        _positionTexts.Add(positionTexts.Find("Y").GetComponent<TMP_Text>());
        _positionTexts.Add(positionTexts.Find("Z").GetComponent<TMP_Text>());

        _rotationTexts.Add(rotationTexts.Find("X").GetComponent<TMP_Text>());
        _rotationTexts.Add(rotationTexts.Find("Y").GetComponent<TMP_Text>());
        _rotationTexts.Add(rotationTexts.Find("Z").GetComponent<TMP_Text>());

        _linearVelocityTexts.Add(linearVelocityTexts.Find("X").GetComponent<TMP_Text>());
        _linearVelocityTexts.Add(linearVelocityTexts.Find("Y").GetComponent<TMP_Text>());
        _linearVelocityTexts.Add(linearVelocityTexts.Find("Z").GetComponent<TMP_Text>());

        _angularVelocityTexts.Add(angularVelocityTexts.Find("X").GetComponent<TMP_Text>());
        _angularVelocityTexts.Add(angularVelocityTexts.Find("Y").GetComponent<TMP_Text>());
        _angularVelocityTexts.Add(angularVelocityTexts.Find("Z").GetComponent<TMP_Text>());
    }

    private void FixedUpdate()
    {
        if (!RocketData.RocketTransform)
            return;

        for (int i = 0; i < 3; i++)
        {
            //Position
            if (_positionTexts != null && _positionTexts.Count > 0)
                _positionTexts[i].text = (i switch
                {
                    0 => RocketData.RocketTransform.position.x,
                    1 => RocketData.RocketTransform.position.y,
                    2 => RocketData.RocketTransform.position.z,
                    _ => 0
                }).ToString("F2");

            //Rotation
            if (_rotationTexts != null && _rotationTexts.Count > 0)
                _rotationTexts[i].text = (i switch
                {
                    0 => RocketData.RocketTransform.rotation.eulerAngles.x.GetAngle180(),
                    1 => RocketData.RocketTransform.rotation.eulerAngles.y.GetAngle180(),
                    2 => RocketData.RocketTransform.rotation.eulerAngles.z.GetAngle180(),
                    _ => 0
                }).ToString("F2");

            //Linear velocity
            if (_linearVelocityTexts != null && _linearVelocityTexts.Count > 0)
                _linearVelocityTexts[i].text = (i switch
                {
                    0 => RocketData.RocketRigidbody.velocity.x,
                    1 => RocketData.RocketRigidbody.velocity.y,
                    2 => RocketData.RocketRigidbody.velocity.z,
                    _ => 0
                }).ToString("F2");

            //Angular velocity
            //float rps2rpm = 60f / (Mathf.PI * 2f);
            float rps2dps = 180f / Mathf.PI;
            if (_angularVelocityTexts != null && _angularVelocityTexts.Count > 0)
                _angularVelocityTexts[i].text = (i switch
                {
                    0 => RocketData.RocketRigidbody.angularVelocity.x * rps2dps,
                    1 => RocketData.RocketRigidbody.angularVelocity.y * rps2dps,
                    2 => RocketData.RocketRigidbody.angularVelocity.z * rps2dps,
                    _ => 0
                }).ToString("F2");
        }
    }
}
