using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRocketTest : MonoBehaviour
{
    [SerializeField] private Transform rocket;

    [Range(0f, 500f)]
    public float
        speedX = 200f,
        speedY = 50f,
        speedZ = 1f;

    [Range(0.1f, 10f)] public float factor = 1f;
    public bool randomize = false;

    private void FixedUpdate()
    {
        rocket.rotation *= Quaternion.Euler(
            speedX * factor * Time.fixedDeltaTime,
            speedY * factor * Time.fixedDeltaTime,
            speedZ * factor * Time.fixedDeltaTime
            );

        if (randomize)
        {
            speedX = Random.Range(0f, 500f);
            speedY = Random.Range(0f, 500f);
            speedZ = Random.Range(0f, 500f);
        }
    }
}
