using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseControl : MonoBehaviour
{
    //Allow the total mouse to interact with the Laser object
    public LaserControl topLaser;
    public LaserControl rightLaser;
    public LaserControl leftLaser;
    public CameraControl view;
    public HeatMapController obs;
    public Text seq;
    public Text seqLen;
    // Bathroom sink extended: rlrrrrlrrlrlrrrrlrrlrlrrrrlrrl

    // Outside loop: llrrlrrlllrrlrllrrrlllrrllrllrrllrlrrlrlllrrllrrllrlrl
    // Outside loop extended: llrrlrrlllrrlrllrrrlllrrllrllrrllrlrrlrlllrrllrrllrlrlllrrlrrlllrrlrllrrrlllrrllrllrrllrlrrlrlllrrllrrllrlrlllrrlrrlllrrlrllrrrlllrrllrllrrllrlrrlrlllrrllrrllrlrl
    // Bed frames (11): lrrllrllrrl
    // TV stand + closet (9): lrrlrrlll
    // If I can guarantee that the mouse is on the outerwall, then we can use RRR. Else, it could look like the table legs, chair, or toilet
    // Bathroom to closet (3): rrr
    // Bathroom to closet (4): rrrl
    // Chair to couch (6): rrllrr
    // Front door to closet (7): llrrlrr

    // Mouse control parameters
    float moveSpeed = 1f;
    public string turnSequence = ""; //.Length, 
    public int maxSequence = 4;
    bool camFollowMouse = true;
    List<Vector2> checkpoints = new List<Vector2>();

    // Asynchronous control variables
    bool readyMove = true; //if the mouse is able to move forward at the time of the call
    bool inState = false; //if the mouse is currently in another coroutine

    // State Machine sentinel variables
    bool aligned = false; //if the mouse has aligned to the walls/obstacles in the room using the perpindicular assumption
    bool mapped = false; //if the mouse has mapped the room

    // Mapping variables
    public float xCoor = 0.0f;
    public float zCoor = 0.0f;
    int mouseDir = 0;
    float currDist = 0.0f;
    float distRightLaser = 0.0f;
    //float distUpRightLaser = 0.0f;
    //float upLaser = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        seqLen.text = "Sequence Length: " + maxSequence;
        Vector3 spawnPos = new Vector3();
        int spawn = (int)(6*Random.value);
        //Debug.Log(spawn);
        Vector3 spawnHeight = Vector3.up * 0.05f;
        switch(spawn){
            // living room
            case 0: spawnPos = (2.5f*Random.value) * Vector3.right + (5f*Random.value-6f) * Vector3.forward + spawnHeight; break;
            // walkway
            case 1: spawnPos = (1f*Random.value-1.5f) * Vector3.right + (3f*Random.value-1f) * Vector3.forward + spawnHeight; break;
            // kitchen
            case 2: spawnPos = (2f*Random.value-3f) * Vector3.right + (5f*Random.value+2f) * Vector3.forward + spawnHeight; break;
            // bedroom
            case 3: spawnPos = (1.5f*Random.value+0.5f) * Vector3.right + (1.5f*Random.value+4f) * Vector3.forward + spawnHeight; break;
            // bedroom closet
            case 4: spawnPos = (1f*Random.value+3.5f) * Vector3.right + (1f*Random.value+4f) * Vector3.forward + spawnHeight; break;
            // bathroom
            case 5: spawnPos = (1f*Random.value+1.5f) * Vector3.right + (2f*Random.value+.25f) * Vector3.forward + spawnHeight; break;
        }
        transform.position = spawnPos;
        // Made things slightly easier by only allowing integer random angles
        int randAngle = (int)(Random.value*360f);
        transform.Rotate(Vector3.up*(float)randAngle);
        view.moveCam(transform.TransformPoint(new Vector3(0f,5f,-10f)));
    }

    //update the camera at the end of the from (after the mouse has moved)
    void LateUpdate()
    {
        if (camFollowMouse){
            view.moveCam(transform.TransformPoint(new Vector3(0f,5f,-10f)));
        }
        else{
            view.TopDown();
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Used coroutines to emulate a state machine
        if (!inState){
            if (!aligned){
                StartCoroutine("alignRoutine");
            }  
            else if (!mapped){
                StartCoroutine("mapRoom");
            }
            // WIP: Doing a mapping routine right now
            //else{
                //StartCoroutine("stateSelect");
            //}
        }
    }

    // Check if the new checkpoint is equivalent to another
    bool CheckPoint(float x, float z){
        Vector2 n = new Vector2(x,z);
        Debug.Log(checkpoints.Count);
        foreach (Vector2 v in checkpoints)
        {
            //float d = Mathf.Sqrt(Mathf.Pow(v[0]-x,2) + Mathf.Pow(v[1]-z,2));
            //Debug.Log((n-v).magnitude);
            if (.15f > (n-v).magnitude){
                return true;
            }
        }
        return false;
    }

    // Alter the view bool; toggle view
    public void SwitchView(){
        camFollowMouse = !camFollowMouse;
    }

    // Reduce the length of the string, min of 1 char
    public void SeqLengthReduce(){
        if (maxSequence > 1){
            maxSequence -= 1;
        }
        // Substring to update the string on the UI
        if (turnSequence.Length > maxSequence)
        {
            turnSequence = turnSequence.Substring(turnSequence.Length-maxSequence);

        }
        seqLen.text = "Sequence Length: " + maxSequence.ToString();
    }

    // Increase the string sequence up to 10 chars
    public void SeqLengthIncrease(){
        if (maxSequence < 10){
            maxSequence += 1;
        }
        seqLen.text = "Sequence Length: " + maxSequence.ToString();
    }

    // Update the string sequence by appending and substringing if too long
    void UpdateSequence(string turn){
            turnSequence = turnSequence + turn;
            if (turnSequence.Length > maxSequence)
            {
                turnSequence = turnSequence.Substring(1);
            }
            // Call HeatMapController to update the heatmap
            obs.UpdateMap();
            // Update text on the UI
            seq.text = "Current Sequence: " + turnSequence;
            if (!CheckPoint(xCoor,zCoor)){
                obs.SpawnMarker();
                checkpoints.Add(new Vector2(xCoor,zCoor));
                //Debug.Log(checkpoints);
            }
            
    }

    // Updates the orientation based on the current turn relative to the sequence of turns since alignment (origin)
    // turn:{0=straight; 1=left; 2=right}
    void updateCoor(int turn, float currDist){
        if(turn == 0){
            switch(mouseDir){
                case 0: zCoor += currDist; break;
                case 1: xCoor -= currDist; break;
                case 2: zCoor -= currDist; break;
                case 3: xCoor += currDist; break;
            }
        }
        else{
            if(turn==1){
                mouseDir += 1;
            }
            else{
                mouseDir -= 1;
            }
        }
        if(mouseDir < 0){
            mouseDir = 3;
        }
        else if(mouseDir > 3){
            mouseDir = 0;
        }
        return;
    }
    
    IEnumerator mapRoom(){
        //forward
        //move forward for a frame's worth of time
        if (topLaser.takeMeasurement() > 0.1f && readyMove){
            inState = true;
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            currDist = Time.deltaTime * moveSpeed;
            updateCoor(0,currDist);
            inState = false;
        }
        //left turn when the laser sees an obstacle .1 meters in front
        else if (topLaser.takeMeasurement() < 0.1f && readyMove){
            inState = true;
            distRightLaser = rightLaser.takeMeasurement();
            if (distRightLaser < 0.2f){
                yield return StartCoroutine("left90");
                //drop pointer
                
            }
            updateCoor(1,0f);
            UpdateSequence("l");
            inState = false;
        }
        //after moving, right turn when there is no right obstacle
        distRightLaser = rightLaser.takeMeasurement();
        if (distRightLaser > 0.2f){
            inState = true;
            readyMove = false;
            float tempRight = 0f;
            //track how far we have artifically forced on the corner turn
            currDist = 0f;
            while (tempRight < .075f){
                transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
                tempRight += Time.deltaTime * moveSpeed;
                currDist += Time.deltaTime * moveSpeed;
                yield return null;
            }
            updateCoor(0,currDist);
            UpdateSequence("r");
            updateCoor(2,0f);
            yield return StartCoroutine("right90");
            tempRight = 0f;
            //track how far we have artificially forced on the corner turn
            currDist = 0f;
            while (tempRight < .1f){
                transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
                tempRight += Time.deltaTime * moveSpeed;
                currDist += Time.deltaTime * moveSpeed;
                yield return null;
            }
            updateCoor(0,currDist);
            readyMove = true;
            inState = false;
        }
    }

    // Simple right turn over frames
    IEnumerator right90(){
        for(int i=0; i<90; i++){
            transform.Rotate(Vector3.up);
            yield return null;
        }
    }

    // Simple left turn over frames
    IEnumerator left90(){
        for(int i=0; i<90; i++){
            transform.Rotate(Vector3.down);
            yield return null;
        }
    }

    // Control coroutine that encapsulates the entire alignment stage. After alignment, this will no long be called
    IEnumerator alignRoutine(){
        inState = true;
        // go forward until wall
        if (topLaser.takeMeasurement() > 0.05f && readyMove){
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            inState = false;
        }
        else{
            readyMove = false;
            if(!aligned){
                yield return StartCoroutine("alignMouse");
                yield return new WaitForSeconds(.5f);
                yield return StartCoroutine("left90");
                // Set origin
                zCoor = 0.0f;
                xCoor = 0.0f;
                mouseDir = 0;
                obs.SpawnMarker();
                checkpoints.Add(new Vector2(xCoor,zCoor));
                readyMove = true;
                aligned = true;
            }
            inState = false;
        }

    }

    // Coroutine that attempts to align the mouse with the XZ plane by assuming perpindicularity
    IEnumerator alignMouse(){
        int minIndex = 0;
        float minDist = topLaser.takeMeasurement();
        float currDist = minDist;
        // take measurements on 360 degrees, point toward lowest measurement (assumed perpindicular)
        for(int i=1; i<361; i++){
            transform.Rotate(Vector3.up);
            currDist = topLaser.takeMeasurement();
            if (currDist < minDist){
                minIndex = i;
                minDist = currDist;
            }
            yield return null;
        }
        for(int i=0; i<minIndex; i++){
            transform.Rotate(Vector3.up);
            yield return null;
        }
        aligned = true;
    }
}
