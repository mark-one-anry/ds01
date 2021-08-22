using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class invisibilitySpell : MonoBehaviour
{
    
    private SpriteRenderer character;
    private Color col;
    private float activationTime;
    private bool invisible;


    void Start()
    {
        
        character = GetComponentInChildren<SpriteRenderer>();
        activationTime = 0;
        invisible = false;
        col = character.color;
    }

    // Update is called once per frame
    void Update()
    {
        activationTime += Time.deltaTime;
        if(invisible && activationTime >=3)
        {
            invisible = false;
            col.a = 1;
            character.color = col;
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            invisible = true;
            activationTime = 0;
            col.a = .2f;
            character.color = col;


        }
    }
}
