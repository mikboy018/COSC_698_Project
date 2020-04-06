using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using MLAgents;
using MLAgents.Sensors;

public class VisualAgent_FPS : Agent
{
    public GameObject area;
    PyramidArea m_MyArea;
    Rigidbody m_AgentRb;
    PyramidSwitch m_SwitchLogic;
    //public GameObject areaSwitch;
    public bool useVectorObs;

    // added for crouching & firing
    public bool crouch = false;
    public bool fire = false;
    public bool jump = false;
    public bool walk = false;
    public GameObject projectilePrefab;
    public Transform spawnPoint;

    // health
    public int health = 100;

    // ammo
    public int ammoCount = 1000;
    public int maxAmmoCount = 1000;
    public int fireDelay = 20;
    public int fireCount = 0;
    public bool isFiring = false;
    public bool isGrounded = false;
    public Collider myCol;

    // animation controls
    Animator animator;

    // camera
    public Camera agentCamera;
    public float sensitivityH = 100.0f;
    public float sensitivityV = 25.0f;
    public float smoothing = 2.0f;
    Vector2 smoothV;
    Vector2 mouseLook;
    public float speedScale = 0.2f;

    public int deathCount = 0;
    public int numKills = 0;

    // keep child close
    public GameObject model;

    public int score = 0;
    int angleCount = 0;

    float xRot = 0f;

    public int matchNum = 1;

    public bool playerControl = false;
    public GameObject manager;

    // instead of adding a penalty for not moving, this allows us to detect change in movement, and reward
    float prevXdir = 0f;
    float prevZdir = 0f;

    void Start(){
        animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        agentCamera.transform.localRotation = Quaternion.Euler(-80,0,0);
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<PyramidArea>();
        //m_SwitchLogic = areaSwitch.GetComponent<PyramidSwitch>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs) //Vector Observation — a feature vector consisting of an array of floating point numbers.
        {
            //sensor.AddObservation(m_SwitchLogic.GetState());
            // 
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        }

        sensor.AddObservation(health);
        sensor.AddObservation(ammoCount);
        sensor.AddObservation(numKills);
        sensor.AddObservation(deathCount);
        sensor.AddObservation(this.transform.position);
        sensor.AddObservation(this.transform.rotation);
        sensor.AddObservation(agentCamera.transform.localRotation);

    }

    public void MoveAgent(float[] act)
    {
        //model.transform.localPosition = new Vector3(0,-3.25f,0);
        //model.transform.rotation = transform.rotation;
        var camAngle = agentCamera.transform.localRotation;
        camAngle.y = 0;
        agentCamera.transform.localRotation = camAngle;
        //var dirToGo = Vector3.zero;
        var zDir = 0.0f;
        var xDir = 0.0f;
        var yDir = 0.0f;
        //var strafeDir = Vector3.zero;

        var navigation = Mathf.FloorToInt(act[0]);
        var jumpCrouch = Mathf.FloorToInt(act[1]);
        var shoot = Mathf.FloorToInt(act[2]);

        // look H will turn entire prefab
        var lookH = 0f;
        // look V will only move camera up/down
        var lookV = 0f;

        // check if agent is grounded
        Vector3 capsuleCast = new Vector3(myCol.bounds.center.x,myCol.bounds.min.y-0.1f,myCol.bounds.center.z);
        isGrounded = Physics.CheckCapsule(myCol.bounds.center,capsuleCast,0.4f, 1);
        
        switch (navigation)
        {
            case 1:
                //dirToGo = transform.forward * -1f;
                zDir = 1f*speedScale;
                //fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 2:
                //dirToGo = transform.forward * 1f;
                zDir = -1f*speedScale;
                //fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 3:
                //strafeDir = transform.right * 1f;
                xDir = 1f*speedScale;
                //fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 4:
                //strafeDir = transform.right * -1f;
                xDir = -1f*speedScale;
                //fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 0:
                //fire = false;
                jump = false;
                walk = false;
                crouch = false;
                break;
        }

        switch(jumpCrouch){
            case 1: // added jump
                //dirToGo = transform.up * 1;
                //yDir = 1f;
                fire = false;

                if(isGrounded)
                    jump = true;
                else
                    jump = false;
                walk = false;
                crouch = false;
                break;
            case 2: // added crouch -- TODO add alternate collision box for ducking
                crouch = true;
                fire = false;
                jump = false;
                walk = false;
                break;

        }
        if(!isFiring){
            switch(shoot){
                case 1: // added fire
                    fire = true;
                    break;
            }   
        } 

        if(playerControl)
        {
            lookH = act[3];
            lookV = act[4];
            lookH *= sensitivityH * Time.fixedDeltaTime * 2;
            lookV *= sensitivityV * Time.fixedDeltaTime * 2;
        } else 
        {
            // lookH and lookV are grouped by 0, 1-10, 11-20 for a total of 21 values
/*             if(act[3] != 0){
                // 1-10
                if(act[3] < 11){
                    lookH = act[3];
                } else { //11-20
                    lookH = act[3]-10; // 11 => 1, 12 => 2, ..., 20 => 10, but must flip sign
                    lookH = -lookH;
                }
            } // 0
             else {
                lookH = 0;
            } */
            switch(act[3]){
                case 0:
                    lookH = 0;
                    break;
                case 1:
                    lookH = 200f * Time.fixedDeltaTime;
                    break;
                case 2:
                    lookH = -200f * Time.fixedDeltaTime;
                    break;
            }

            if(act[4] != 0){
                // 1 up
                if(act[4] == 1 ){
                    lookV = act[4];
                } else { // 2 down
                    lookV = act[4]/2; 
                    lookV = -lookV;
                }
            } // 0
             else {
                lookV = 0;
            }
        }

        xRot -= lookV;
        
        if(Mathf.Abs(xRot) > 20){
            AddReward(-0.05f);
        }

        xRot = Mathf.Clamp(xRot, -45f, 45f);

        agentCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up, lookH);
       
        if(jump && isGrounded){
            //Debug.Log("JUMP");
            this.GetComponent<Rigidbody>().AddForce(Vector3.up*100);
        }

        // update animation states
        animator.SetBool("running", walk);
        animator.SetBool("jumping", jump);
        //animator.SetBool("crouching", crouch);
        animator.SetBool("shooting", fire);

        // Navigation
/*         if(xDir == 0 || zDir == 0){
            AddReward(-0.01f);
        } */
        
        /*if(xDir != 0 || zDir != 0){
            AddReward(0.01f);
        }*/
        // updated 2 Apr to match F1 more closely
        var xDist = Mathf.Abs(xDir - prevXdir);
        if(xDist > 0)
            AddReward(Mathf.Clamp(0.000045f*xDist, 0f, 8f)); // smaller values to encoure more forward motion

        var zDist = Mathf.Abs(zDir - prevZdir);
        if(zDist > 0)
            AddReward(Mathf.Clamp(0.00009f*zDist, 0f, 15f));

        if(xDist == 0 && zDist==0)
            AddReward(-0.03f); // match f1 penalty for not moving
        // store last value
        prevXdir = xDir;
        prevZdir = zDir;
        
        transform.Translate(xDir, 0, zDir);
        
        if(isFiring){
            fireCount++;
            fire = false;
            if(fireCount > fireDelay){
                fireCount = 0;
                isFiring = false;
            }
        } 

        if(fire){
        
            if(ammoCount > 0){
                var firingAngle = agentCamera.transform.rotation;
                var projectileInstance = Instantiate(projectilePrefab, spawnPoint.position, firingAngle);
                //projectileInstance.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
                projectileInstance.GetComponent<Projectile>().parentAgent = gameObject;
                
                ammoCount -= 1;
                float reward = 0.0001f * (maxAmmoCount-ammoCount)/maxAmmoCount; // increases penalty as ammo lower
                 if(ammoCount <= 0){
                    AddReward(-0.05f); // penalty for running out of ammo
                }
                // updated to match F1 -- agents learned to not shoot with this high of a reward
                //AddReward(-0.04f);
                isFiring = true;
            }
        }
    }

    public override void AgentAction(float[] vectorAction)
    {
        // matching F1 AddReward(-1f / maxStep); // motivate AI to find positive reward faster
        AddReward(-0.008f);
        MoveAgent(vectorAction);
    }

    // If you set Behavior Type to Heuristic Only, the Agent will use its Heuristic() method to make decisions
    // which can allow you to control the Agent manually or write your own Policy.
    public override float[] Heuristic() 
    {
        int navAction = 0;
        int jumpCrouchAction = 0;
        int shootAction = 0;
        float lookLR = 0f;
        float lookUD = 0f;
        

        if (Input.GetKey(KeyCode.D))
        {
            //return new float[] { 3 };
            navAction = 3;
        }
        if (Input.GetKey(KeyCode.W))
        {
            //return new float[] { 1 };
            navAction = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //return new float[] { 4 };
            navAction = 4;
        }
        if (Input.GetKey(KeyCode.S))
        {
            //return new float[] { 2 };
            navAction = 2;
        }
        if (Input.GetKey(KeyCode.Space)){
            //return new float[] { 5 }; // Jump
            jumpCrouchAction = 1;
        }
/*         if (Input.GetKey(KeyCode.C)){
            //return new float[] { 6 }; // Crouch
            jumpCrouchAction = 2;
        } */
        if (Input.GetKey(KeyCode.Mouse0)){
            //return new float[] { 7 };  // Fire
            shootAction = 1;
        }

        if(playerControl){
            lookLR = Input.GetAxis("Mouse X");
            lookUD = Input.GetAxis("Mouse Y");
        } else {

            if(Input.GetKey(KeyCode.LeftArrow)){
                lookLR = 2f;
            } else if (Input.GetKey(KeyCode.RightArrow)){
                lookLR = 1f;
            }

            if(Input.GetKey(KeyCode.UpArrow)){
                lookUD = 1f;
            } else if (Input.GetKey(KeyCode.DownArrow)){
                lookUD = 2f;
            }
        }

        //return new float[] { 0 };
        return new float[] { navAction, jumpCrouchAction, shootAction, lookLR, lookUD };
    }

    public override void AgentReset()
    {   
        numKills = 0;
        agentCamera.transform.localRotation = Quaternion.Euler(-80,0,0);
        matchNum++; // shows how many times a match has completed (10 frags to first player)
        var enumerable = Enumerable.Range(0, 9).OrderBy(x => Guid.NewGuid()).Take(9);
        var items = enumerable.ToArray();

        m_MyArea.CleanPyramidArea();

        m_AgentRb.velocity = Vector3.zero;
        m_MyArea.PlaceObject(gameObject, items[0]);
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        

        //m_SwitchLogic.ResetSwitch(items[1], items[2]);
        m_MyArea.CreateStonePyramid(1, items[3]);
        m_MyArea.CreateStonePyramid(1, items[4]);
        m_MyArea.CreateStonePyramid(1, items[5]);
        m_MyArea.CreateStonePyramid(1, items[6]);
        m_MyArea.CreateStonePyramid(1, items[7]);
        m_MyArea.CreateStonePyramid(1, items[8]);

        ammoCount = maxAmmoCount;
        health = 100;
        Debug.Log("Round: " + matchNum);
    }

    void respawn(){
        var enumerable = Enumerable.Range(0, 9).OrderBy(x => Guid.NewGuid()).Take(9);
        var items = enumerable.ToArray();
        m_AgentRb.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        m_MyArea.PlaceObject(gameObject, items[0]);
    }

// need to move this to scripts for ammo/health
    void OnCollisionEnter(Collision collision)
    {
        // reward player for picking up health/ammo
         if (collision.gameObject.CompareTag("healthPack") || collision.gameObject.CompareTag("ammoPack"))
        {
            AddReward(.15f); // 2 apr was 1f, updated to align with F1 ammo pickup
        }
    } 

    public bool SetHealth(int damage){
        health -= damage;
        if(damage < 0) // penalty for taking damage
            AddReward(0.05f*damage);
        if(health <= 0){
            //AddReward(-0.5f); // motivate AI to not die -- removed to align with f1, placing -0.05 penalty * projectile damage
            //AgentReset();
            deathCount++;
            health = 100;
            ammoCount = maxAmmoCount;
            //numDeaths++;
            //Debug.Log(gameObject.name + ", was killed");
            //if(deathCount == 10){
                //deathCount = 0;
                //AgentReset();
                
            //} else{
                respawn();
                
            //}

            return true;
        }

        if(health >= 100){
            health = 100;
        }

        return false; // returns false if player still alive
    }

    public void AddScore(int increment){
        score += increment;
        if(numKills == 10){
            //numKills = 0;
            Debug.Log("Player: " + gameObject.name + " Kills this Round: " + numKills);
            numKills = 0;
            Debug.Log("Player: " + gameObject.name + " Total deaths: " + deathCount);
            deathCount = 0;
            AddReward(5f); // won the match
            //AgentReset();
            //Done();
            manager.GetComponent<GameManager>().ResetScenario();
        }
    }

    public void SetAmmo(int ammo){
        ammoCount += ammo;

        if(ammoCount >= maxAmmoCount){
            ammoCount = maxAmmoCount;
        }
    }

    void OnApplicationQuit(){
        Debug.Log("Player: " + gameObject.name + " Score: " + score);
    }

}
