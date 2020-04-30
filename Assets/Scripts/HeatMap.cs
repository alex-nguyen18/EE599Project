using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMap : MonoBehaviour
{
    public MouseControl mouse;
    public string sequence;
    public bool isLoop = false;
    Renderer rend;

    // Get the renderer object so I can manipulate the color of the object
    void Start(){
        rend = GetComponent<Renderer>(); 
    }

    // This function is on each obstacle (with exception of a few walls)
    // Comares the mouse sequence to the obstacle's sequence to update the color map
    void UpdateColor(){
        int len = mouse.maxSequence;
        // Don't check until at least 1 turn is recorded
        if (mouse.turnSequence != ""){
            // Take advantage of Contains function to check the string sequence
            if(sequence.Contains(mouse.turnSequence)){
                rend.material.color = Color.magenta;
            }
            else {
                rend.material.color = Color.white;
            }
            if(!mouse.turnSequence.Contains("l") && isLoop){
                rend.material.color = Color.magenta;
            }
        }
    }

}
