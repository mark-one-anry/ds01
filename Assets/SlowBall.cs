using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowBall : MonoBehaviour
{
    public float speed = 6f;
    public float slowEffect = 3f;
    public float slowTime=3f;

    // protected Vector3 slowBalltarget;
    protected GameObject slowBalltarget;
    private Rigidbody2D rb;
    private Rigidbody2D targetRB;
    private SimplePlayerController playerController; 

    protected bool refreshTarget;
    private float sourceMass;
    private float sourceMovePower;
    private bool slowInProgress;
    private float slowStartTime;

    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();          
        // Просто направление в сторону цели
        //Vector2 fireDirection = (this.slowBalltarget.x < transform.position.x ? Vector3.left : Vector3.right);        
        //Vector2 fireDirection = (this.slowBalltarget.transform.position.x < transform.position.x ? Vector3.left : Vector3.right);        
        // Направление в центр масс цели
        // 
        targetRB = slowBalltarget.GetComponent<Rigidbody2D>();
        Vector2 fireDirection = ((new Vector2(slowBalltarget.transform.position.x,slowBalltarget.transform.position.y) + targetRB.centerOfMass) - new Vector2(transform.position.x,transform.position.y)).normalized; 
        //Debug.Log("Casting slowball to " + fireDirection + ", target x = " + slowBalltarget.x + ", spitter position = " + transform.position.x);
        //Debug.Break();
        rb.velocity = fireDirection * speed;
        //rb.velocity = transform.right * speed;
        slowInProgress = false;        
    }

    // Update is called once per frame
    void Update()
    {        
        
    }

    //public void setTarget(Vector3 targetNew)
    public void setTarget(GameObject targetNew)
    {        
        //this.target = targetNew;
        //this.slowBalltarget = new Vector3(targetNew.x,targetNew.y,targetNew.z);
        this.slowBalltarget = targetNew;
        //refreshTarget = true;        
        //Debug.Log("Slowball got new target:" + targetNew.x + ", target.x = " + this.slowBalltarget.x);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {   
        //Debug.Log("Slowball OnTriggerEnter2D");
        LayerMask plm = LayerMask.GetMask("Player");
        LayerMask wlm = LayerMask.GetMask("Wall");

        if(plm == (plm | (1 << other.gameObject.layer)) && !slowInProgress)
        {
            // slow player
            Debug.Log("Slowball hits player");
            //other.gameObject.GetComponent<SimplePlayerController>().hit(5f);
            targetRB = other.gameObject.GetComponent<Rigidbody2D>();
            playerController = other.gameObject.GetComponent<SimplePlayerController>();
            /*
            slowStartTime = Time.time;
            slowInProgress = true;
            sourceMass = targetRB.mass;
            targetRB.mass = sourceMass * slowEffect;
            sourceMovePower = playerController.movePower;
            playerController.movePower = sourceMovePower / 3;
            */
            playerController.slowEffect(slowEffect, slowEffect, slowTime);
            Destroy(gameObject);
            
            // gameObject.GetComponent<SpriteRenderer>().enabled=false;
            // Destroy(gameObject);
        }
        else  if (wlm == (wlm | (1 << other.gameObject.layer)))
        {
            // Hit wall 
            Destroy(gameObject);
        }
    }
}
