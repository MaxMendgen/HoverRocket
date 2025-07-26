using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WayPoint
{
    public Vector3 position;

    public float holdForSeconds;

    public bool slowDecent;
}

public class FlightProfiler : MonoBehaviour
{

    [SerializeField] List<WayPoint> targetCoordinates = new List<WayPoint>();

    public float maxDesentRate;

    public bool active;

    public int currentWayPoint = 0;

    private int currentWayPointHidden = -1;

    private bool isInside;

    private float timeSinceInside;
    //private Collider collider;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!active)
        {
            return;
        }

        if (targetCoordinates.Count == 0)
        {
            throw new System.NullReferenceException("No target coordinates set for the flight profiler!");
        }

        WayPoint currentTargetCordinates = targetCoordinates[currentWayPoint];


        if (currentWayPoint != currentWayPointHidden)
        {
            currentWayPointHidden = currentWayPoint;
            currentWayPoint -= 1;
            UpdatePosition();
        }

        if (currentTargetCordinates.slowDecent)
        {
            if (currentTargetCordinates.position.y < transform.position.y)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - maxDesentRate * Time.deltaTime, transform.position.z);
            }
            else
            {
                UpdatePosition();
            }
        }


        if (isInside && currentTargetCordinates.holdForSeconds != 0f)
        {
            timeSinceInside += Time.deltaTime;
        }
        
        
        if (timeSinceInside >= currentTargetCordinates.holdForSeconds && currentTargetCordinates.holdForSeconds != 0f)
        {
            UpdatePosition();
        }
    }

    //on colition enter from a smal box and velocity lower the threshhold then move to next 

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Rocket" && active)
        {
            isInside = true;

            if (targetCoordinates[currentWayPoint].holdForSeconds == 0)
            {
                UpdatePosition();
            }
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Rocket" && active)
        {
            isInside = false;
            timeSinceInside = 0f;
        }
    }

    void UpdatePosition()
    {
        if (currentWayPoint < targetCoordinates.Count - 1)
        {
            currentWayPoint += 1;
            if (!targetCoordinates[currentWayPoint].slowDecent)
            {
                transform.position = targetCoordinates[currentWayPoint].position;
            }
            else
            {
                transform.position = new Vector3(targetCoordinates[currentWayPoint].position.x, transform.position.y, targetCoordinates[currentWayPoint].position.z);
            }
        }
        else
        {
            print("Finished!");
            active = false;
        }
    }
}
