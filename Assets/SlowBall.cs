using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowBall : MonoBehaviour
{
    public float speed = 1f;
    public float slowEffect = 3f;
    public float slowTime=3f;
    private Rigidbody2D rb;
    private Rigidbody2D targetRB;
    private SimplePlayerController playerController; 

    private float sourceMass;
    private float sourceMovePower;
    private bool slowInProgress;
    private float slowStartTime;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        slowInProgress = false;
    }

    // Update is called once per frame
    void Update()
    {
        // finish slow
        if(slowInProgress && (Time.time - slowStartTime)>=slowTime)
        {
            slowInProgress = false; 
            targetRB.mass = sourceMass;
            playerController.movePower = sourceMovePower;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {        
        LayerMask plm = LayerMask.GetMask("Player");
        LayerMask wlm = LayerMask.GetMask("Wall");

        if(plm == (plm | (1 << other.gameObject.layer)) && !slowInProgress)
        {
            // slow player
            //other.gameObject.GetComponent<SimplePlayerController>().hit(5f);
            targetRB = other.gameObject.GetComponent<Rigidbody2D>();
            playerController = other.gameObject.GetComponent<SimplePlayerController>();
            slowStartTime = Time.time;
            slowInProgress = true;
            sourceMass = targetRB.mass;
            targetRB.mass = sourceMass * slowEffect;
            sourceMovePower = playerController.movePower;
            playerController.movePower = sourceMovePower / 3;
            
            gameObject.GetComponent<SpriteRenderer>().enabled=false;
            // Destroy(gameObject);
        }
        else  if (wlm == (wlm | (1 << other.gameObject.layer)))
        {
            // Hit wall 
            Destroy(gameObject);
        }
    }
}
