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
    public int ammoCount = 100;
    public int fireDelay = 20;
    public int fireCount = 0;
    public bool isFiring = false;

    // animation controls
    Animator animator;

    // camera
    public Camera agentCamera;
    public float sensitivity = 5.0f;
    public float smoothing = 2.0f;
    Vector2 smoothV;
    Vector2 mouseLook;
    public float speedScale = 0.2f;

    void Start(){
        animator = GetComponentInChildren<Animator>();
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
    }

    public void MoveAgent(float[] act)
    {
        //var dirToGo = Vector3.zero;
        var zDir = 0.0f;
        var xDir = 0.0f;
        var yDir = 0.0f;
        //var strafeDir = Vector3.zero;

        var navigation = Mathf.FloorToInt(act[0]);
        var jumpCrouch = Mathf.FloorToInt(act[1]);
        var shoot = Mathf.FloorToInt(act[2]);

        // look H will turn entire prefab
        var lookH = Mathf.FloorToInt(act[3]);
        // look V will only move camera up/down
        var lookV = Mathf.FloorToInt(act[4]);

        var upDown = 0.0f;
        var leftRight = 0.0f;

        switch (navigation)
        {
            case 1:
                //dirToGo = transform.forward * -1f;
                zDir = 1f*speedScale;
                fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 2:
                //dirToGo = transform.forward * 1f;
                zDir = -1f*speedScale;
                fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 3:
                //strafeDir = transform.right * 1f;
                xDir = 1f*speedScale;
                fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;
            case 4:
                //strafeDir = transform.right * -1f;
                xDir = -1f*speedScale;
                fire = false;
                jump = false;
                walk = true;
                crouch = false;
                break;

        }

        switch(jumpCrouch){
            case 1: // added jump
                //dirToGo = transform.up * 1;
                //yDir = 1f;
                fire = false;
                jump = true;
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
                    jump = false;
                    walk = false;
                    crouch = false;
                    break;
            }   
        } 
        switch(lookH){
            case 1: // added left/right looking (turning)
                //rotateDir = transform.up * 1f;
                leftRight = 1f;
                break;
            case 2: 
                //rotateDir = transform.up * -1f;
                leftRight = -1f;
                break;
        }
        // https://www.youtube.com/watch?v=blO039OzUZc
        switch(lookV){
            case 1: // added up/down looking (on camera transform)
                upDown = 1.0f*speedScale;
                break;
            case 2:
                upDown = -1.0f*speedScale;
                break;
        }

        // update animation states
        animator.SetBool("walking", walk);
        animator.SetBool("jumping", jump);
        animator.SetBool("crouching", crouch);
        animator.SetBool("shooting", fire);


        var mouseDir = new Vector2(leftRight, upDown);
        mouseDir = Vector2.Scale(mouseDir, new Vector2(sensitivity*smoothing, sensitivity*smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, mouseDir.x, 1f*smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, mouseDir.y, 1f/smoothing);
        mouseLook += smoothV;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -75f, 75f);

        agentCamera.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        transform.rotation = Quaternion.Euler(0,mouseLook.x,0);
        //transform.localRotation = Quaternion.AngleAxis(mouseLook.x*5, transform.up);
        transform.Translate(xDir, 0, zDir);
        if(jump){
            this.GetComponent<Rigidbody>().AddForce(Vector3.up*200);
        }
        //m_AgentRb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);

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
                if(ammoCount <= 0){
                    AddReward(-1f); // penalty for running out of ammo
                }

                isFiring = true;
            }

        }



    }

    public override void AgentAction(float[] vectorAction)
    {
        AddReward(-1f / maxStep); // motivate AI to find positive reward faster
        MoveAgent(vectorAction);
    }

    // If you set Behavior Type to Heuristic Only, the Agent will use its Heuristic() method to make decisions
    // which can allow you to control the Agent manually or write your own Policy.
    public override float[] Heuristic() 
    {
        int navAction = 0;
        int jumpCrouchAction = 0;
        int shootAction = 0;
        int lookLR = 0;
        int lookUD = 0;
        

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
        if (Input.GetKey(KeyCode.C)){
            //return new float[] { 6 }; // Crouch
            jumpCrouchAction = 2;
        }
        if (Input.GetKey(KeyCode.Z)){
            //return new float[] { 7 };  // Fire
            shootAction = 1;
        }

        if(Input.GetAxisRaw("Mouse X") > 0){
            lookLR = 1;
        } else if (Input.GetAxisRaw("Mouse X") < 0){
            lookLR = 2;
        }

        if(Input.GetAxisRaw("Mouse Y") > 0)
        {
            lookUD = 1;
        } else if (Input.GetAxisRaw("Mouse Y") < 0){
            lookUD = 2;
        }
        //return new float[] { 0 };
        return new float[] { navAction, jumpCrouchAction, shootAction, lookLR, lookUD };
    }

    public override void AgentReset()
    {
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

        ammoCount = 50;
        health = 100;
    }

// need to move this to scripts for ammo/health
    void OnCollisionEnter(Collision collision)
    {
        // reward player for picking up health/ammo
         if (collision.gameObject.CompareTag("healthPack") || collision.gameObject.CompareTag("ammoPack"))
        {
            AddReward(1f);
            Done();
        }
    } 

    public void SetHealth(int damage){
        health -= damage;

        if(health <= 0){
            AddReward(-5f); // motivate AI to not die
            AgentReset();
        }

        if(health >= 100){
            health = 100;
        }
    }

    public void SetAmmo(int ammo){
        ammoCount += ammo;

        if(ammo >= 200){
            ammo = 200;
        }
    }

}
