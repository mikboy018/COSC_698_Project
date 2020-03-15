using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 50;
    public float speed = 10;

    public GameObject parentAgent; // agent who fired the shot, for scoring

    void Start(){
        Destroy(gameObject, 15);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(Vector3.forward*Time.deltaTime*speed);        
    }

    void OnCollisionEnter(Collision col){
        // If projectile collided with agent, decrease agent health, reward parentAgent
        if(col.gameObject.CompareTag("agent") && col.gameObject != parentAgent){
            parentAgent.GetComponent<VisualAgent_FPS>().AddReward(1000f);
            col.gameObject.GetComponent<VisualAgent_FPS>().SetHealth(damage);
        }
        // destroy projectile
        Destroy(gameObject);
    }
}
