using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSkin : MonoBehaviour
{
    public List<Material> skins;

    // Start is called before the first frame update
    void Start()
    {
      // set skin to randomly chosen
      GetComponentsInChildren<Renderer>()[0].material = skins[Random.Range(0,skins.Count)];   
    }
}
