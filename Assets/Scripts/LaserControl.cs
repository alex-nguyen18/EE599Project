using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    // **Place any obstacle in the ObstacleMask layer
    int obstacleMask = 1 << 8;

    // Take a measurement to the next obstacle in direction of laser up to 2 units
    public float takeMeasurement()
    {
        //Creates a new ray along the object's Z-Axis
        Ray laserRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;
            //Shoot the laser at a given point up to 2 units
            //Return min(2,hit.distance)
            if (Physics.Raycast(laserRay, out hit, 2, obstacleMask))
            {
                //Debug.Log(hit.distance);
                return hit.distance;
            }
            //Debug.Log(2);
            return 2;
    }
}
