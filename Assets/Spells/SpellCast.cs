using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class SpellCast : MonoBehaviour
{ 
    public Transform shotpos;
    public GameObject Fire; 
    public float spellcost = 2f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Instantiate(Fire,  shotpos.transform.position, transform.rotation);
            GetComponent<SimplePlayerController>().cast(spellcost);

        }
    }
}
