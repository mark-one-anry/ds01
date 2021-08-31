using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewAreaScript : MonoBehaviour
{
    private GameObject parent;
    private Collider2D viewArea;
    private SpitterAI parentScript;
    private int playerPartCount; // количество видимых фрагментов игрока

    
    void Start()
    {
        parent = transform.parent.gameObject;        
        viewArea = GetComponent<Collider2D>();
        parentScript = parent.GetComponent<SpitterAI>();
        playerPartCount = 0;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("DETECTED");
        //if(viewArea.IsTouchingLayers(LayerMask.GetMask("Player")))
        LayerMask lm = LayerMask.GetMask("Player", "Guard");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            // raycast to touching object to check if it behind the walls             
            Debug.Log("Player detected at " + other.transform.position.x);
            playerPartCount++;
            parentScript.ObjectDetected(other);       
        }
        
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LayerMask lm = LayerMask.GetMask("Player", "Guard");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            // raycast to touching object to check if it behind the walls             
            playerPartCount--;
            if(playerPartCount == 0)
                parentScript.ObjectLost(other);       
        }
    }


}
