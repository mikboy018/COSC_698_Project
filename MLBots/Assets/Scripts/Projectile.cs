using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 50;
    public float speed = 10;
    public bool playerKilled = false;
    bool playerHit = false;

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
/*         if(col.gameObject.CompareTag("agent") && col.gameObject != parentAgent){
            playerHit = true;
            parentAgent.GetComponent<VisualAgent_FPS>().AddReward(0.5f);
            Debug.Log("Player Hit");
            playerKilled = col.gameObject.GetComponent<VisualAgent_FPS>().SetHealth(damage);
            if(playerKilled){
                parentAgent.GetComponent<VisualAgent_FPS>().AddReward(1f);
                parentAgent.GetComponent<VisualAgent_FPS>().AddScore(1);
                parentAgent.GetComponent<VisualAgent_FPS>().numKills++;
            }
        }  */

        // destroy projectile
        if(!col.gameObject.CompareTag("healthPack") && !col.gameObject.CompareTag("ammoPack")){
            Destroy(gameObject);
        }
    }

    void OnDestroy(){
        // splash damage
        areaDamage();
    }

    void areaDamage(){
        Collider[] objs = Physics.OverlapSphere(transform.position, 5f);
        int i = 0;
        while(i < objs.Length){
            if(objs[i].gameObject.CompareTag("agent") && objs[i].gameObject != parentAgent){
                playerKilled = objs[i].gameObject.GetComponent<VisualAgent_FPS>().SetHealth(damage);
                if(playerKilled){
                    parentAgent.GetComponent<VisualAgent_FPS>().AddReward(1f);
                    parentAgent.GetComponent<VisualAgent_FPS>().numKills++;
                    parentAgent.GetComponent<VisualAgent_FPS>().AddScore(1);
                    Debug.Log(parentAgent.name + " got a kill");
                }
            }
            i++;
        }
    }
}
