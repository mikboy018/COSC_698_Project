using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<GameObject> agents;

    // Calls "Done()" for each agent
    public void ResetScenario(){
        for(int i = 0; i < agents.Count; i++){
            agents[i].GetComponent<VisualAgent_FPS>().Done();
        }
    }
}
