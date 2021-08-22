using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpell : MonoBehaviour
{
    public float speed;
    public float Damage = 3f;
    Rigidbody2D rb;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // uncomment to filter touching objects 
        // LayerMask lm = LayerMask.GetMask("Player", "Guard");
        //if(lm == (lm | (1 << other.gameObject.layer)))
        //{
        
        if(other.gameObject.layer == LayerMask.GetMask("Player"))
        {
            // Hit player
            other.gameObject.GetComponent<SimplePlayerController>().hit(Damage);
            Destroy(gameObject);
        }
        
        // if(other.gameObject.layer == LayerMask.GetMask("Enemy"))
        LayerMask lm = LayerMask.GetMask("Enemy");
        LayerMask wlm = LayerMask.GetMask("Wall");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            // Hit enemy
            Debug.Log("pre hit enemy: " + other.gameObject);

            other.gameObject.GetComponent<EnemyBase>().hit((int)Damage);
            Destroy(gameObject);
        }
        else  if (wlm == (wlm | (1 << other.gameObject.layer)))
        {
            // Hit wall 
            Destroy(gameObject);
        }


        

        
        
    }
}
