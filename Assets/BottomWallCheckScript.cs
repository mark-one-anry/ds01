using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomWallCheckScript : MonoBehaviour
{
    private GameObject parent;
    private Collider2D viewArea;
    private SpitterAI parentScript; 
    public bool isTouching; 

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;        
        parentScript = parent.GetComponent<SpitterAI>();
        isTouching = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        LayerMask lm = LayerMask.GetMask("Wall");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            isTouching = true;
        }
  
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LayerMask lm = LayerMask.GetMask("Wall");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            isTouching = false;
        }
    }
}
