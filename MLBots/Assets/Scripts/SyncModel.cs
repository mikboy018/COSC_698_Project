using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncModel : MonoBehaviour
{
    public GameObject model;

    // Update is called once per frame
    void FixedUpdate()
    {
        model.transform.localPosition = new Vector3(0,-3.25f,0);
        model.transform.rotation = transform.rotation;
        if(transform.position.y < -200)
        {
            gameObject.GetComponent<VisualAgent_FPS>().SetHealth(200);
        }
    }
}
