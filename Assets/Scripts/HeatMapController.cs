using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeatMapController : MonoBehaviour
{
    public GameObject marker;
    public MouseControl mouse;

    // Call the UpdateColor function on all obstacles asynchronously
    public void UpdateMap(){
        gameObject.BroadcastMessage("UpdateColor");
    }

    public void SpawnMarker(){
        GameObject m = (GameObject)Instantiate(marker,mouse.transform.position,mouse.transform.rotation);
        m.GetComponent<MarkerController>().SetCoors(mouse.xCoor, mouse.zCoor);
    }

    public void ReloadScenario(){
        SceneManager.LoadScene("Apartment",LoadSceneMode.Single);
    }

    public void CloseGame(){
        Application.Quit();
    }
}
