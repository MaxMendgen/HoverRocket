using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WayPoint {
    public Vector3 position;

    public float holdForSeconds;

    public bool slowDecent;
}

public class FlightProfiler : MonoBehaviour
{

    [SerializeField] List<WayPoint> targetCordinates = new List<WayPoint>() ;

    public float maxDesentRate;

    public bool aktiv;

    public int currentWaypoint = 0;

    private int currentWaypointHidden = -1;

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
        if (!aktiv) {
            return;
        }

        WayPoint currentTargetCordinates = targetCordinates[currentWaypoint];


        if (currentWaypoint != currentWaypointHidden) {
            currentWaypointHidden = currentWaypoint;
            currentWaypoint -= 1;
            UpdatePosition();
        }

        if (currentTargetCordinates.slowDecent) {
            if (currentTargetCordinates.position.y < transform.position.y) {
                transform.position = new Vector3(transform.position.x, transform.position.y - maxDesentRate*Time.deltaTime, transform.position.z);
            }
            else {
                UpdatePosition();
            }
        }




        if (isInside && currentTargetCordinates.holdForSeconds != 0f) {
            timeSinceInside += Time.deltaTime;
        }
    

        if (timeSinceInside >= currentTargetCordinates.holdForSeconds && currentTargetCordinates.holdForSeconds != 0f) {
            UpdatePosition();
        }
    }

    //on colition enter from a smal box and velocity lower the threshhold then move to next 

    void OnTriggerEnter(Collider other) 
    {
        if (other.name == "Rocket" && aktiv) {
            isInside = true; 

            if (targetCordinates[currentWaypoint].holdForSeconds == 0) {
                UpdatePosition();
            }
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Rocket" && aktiv) {
            isInside = false;
            timeSinceInside = 0f;
        }
    }

    void UpdatePosition() {
        if (currentWaypoint < targetCordinates.Count-1) {
            currentWaypoint += 1;
            if (!targetCordinates[currentWaypoint].slowDecent) {
                transform.position = targetCordinates[currentWaypoint].position;
            }
            else {
                transform.position = new Vector3(targetCordinates[currentWaypoint].position.x,transform.position.y,targetCordinates[currentWaypoint].position.z);
            }
        }
        else {
            print("Finished!");
            aktiv = false;
        }
    }
}
