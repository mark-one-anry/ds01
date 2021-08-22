using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewAreaScript : MonoBehaviour
{
    private GameObject parent;
    private Collider2D viewArea;
    private SpitterAI parentScript; 
    
    void Start()
    {
        parent = transform.parent.gameObject;        
        viewArea = GetComponent<Collider2D>();
        parentScript = parent.GetComponent<SpitterAI>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("DETECTED");
        //if(viewArea.IsTouchingLayers(LayerMask.GetMask("Player")))
        LayerMask lm = LayerMask.GetMask("Player", "Guard");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            // raycast to touching object to check if it behind the walls             
            parentScript.ObjectDetected(other);       
        }
        
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LayerMask lm = LayerMask.GetMask("Player", "Guard");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            // raycast to touching object to check if it behind the walls             
            parentScript.ObjectLost(other);       
        }
    }


}
