using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class invisibilitySpell : MonoBehaviour
{
    private Component[] WizRender;
    private SpriteRenderer character;
    private Color col;
    private float activationTime;
    private bool invisible;
    public float spellcost = 5f;
    // IDictionary<string, MonoBehaviour> subscribers = new Dictionary<string, MonoBehaviour>();

    void Start()
    {
        
        WizRender = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer cc in WizRender)
        {
            character = cc;
        }

        activationTime = 0;
        invisible = false;

    }

    // Update is called once per frame
    void Update()
    {
        activationTime += Time.deltaTime;
        if(invisible && activationTime >=3)
        {
            invisible = false;
            foreach(SpriteRenderer cc in WizRender)
            {
                col = cc.color;
                col.a = 1;
                cc.color = col;
            }

        }

        if(Input.GetKeyDown(KeyCode.I) && !invisible)

        {
            invisible = true;
            GetComponent<SimplePlayerController>().cast(spellcost);
            activationTime = 0;
            foreach(SpriteRenderer cc in WizRender)
            {
                col = cc.color;
                col.a = .2f;
                cc.color = col;
            }

        }
    }

    public bool isInvisible()
    {
        return invisible;
    }
}
