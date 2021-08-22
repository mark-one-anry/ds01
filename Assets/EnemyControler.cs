using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControler : MonoBehaviour
{
    public int MaxHealth = 10;
    private int health; 

    private Animator anim; 
    // Start is called before the first frame update
    void Start()
    {
        health = MaxHealth;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hit(int dmg)
    {
        Debug.Log("hit by " + dmg);
        health-=dmg;
        
        if(health < 0.1f){
            anim.SetTrigger("Death");
            //Destroy(gameObject, shrink.clip.length); d
            gameObject.GetComponent<SpitterAI>().Die();

        }
        else 
            anim.SetTrigger("Hit");
        
                
    }
}
