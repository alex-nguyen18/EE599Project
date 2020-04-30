using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject mouse;

    // Follow the mouse from behind
    public void moveCam(Vector3 camPos)
    {
        transform.rotation = mouse.transform.rotation;
        //Debug.Log(camPos);
        transform.position = camPos;
        transform.Rotate(Vector3.right*20f);
    }

    // Show bird's eye view so heat map and markers are easier to see
    public void TopDown(){
        transform.position = Vector3.up * 15f;
        transform.eulerAngles = Vector3.right * 90f;
    }

}
