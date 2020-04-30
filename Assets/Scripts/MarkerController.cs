using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerController : MonoBehaviour
{
    public Text markerCoors;
    float xCoor = 0f;
    float zCoor = 0f;
    Renderer rend;

    // Show the coordinate so of the marker when mouse hovers a marker
    void OnMouseOver(){
        markerCoors.text = "Marker location is: ("+xCoor.ToString()+","+zCoor.ToString()+")";
    }
    
    // Remove UI text on mouseExit
    void OnMouseExit(){
        markerCoors.text = "";
    }

    // Set the coordinates of the marker; if is the origin, make it green
    public void SetCoors(float x, float z){
        xCoor = x;
        zCoor = z;
        rend = GetComponent<Renderer>();
        if (xCoor == 0f && zCoor == 0f){
            rend.material.color = Color.green;
        }
    }

}
