using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    public int ammoCount = 50;

    void OnCollisionEnter(Collision col){
        // If projectile collided with agent, decrease agent health, reward parentAgent
        if(col.gameObject.CompareTag("agent")){
            col.gameObject.GetComponent<VisualAgent_FPS>().AddReward(1f);
            col.gameObject.GetComponent<VisualAgent_FPS>().SetAmmo(ammoCount);
            gameObject.SetActive(false);
            Invoke("reactivate", 10.0f); // re-enable the object after 10 seconds
        }
    }

    void reactivate(){
        gameObject.SetActive(true);
    }
}