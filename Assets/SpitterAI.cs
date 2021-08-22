using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int MaxHealth = 10;
    public float movePower = 4f;
    public float chasePower = 7f;
    public float PATROL_RANGE = 2500f; // distance to patrol
    public float SEE_RANGE = 250f;
    public float SPELL_RANGE = 2.5f;
    public float SPELL_DELAY = 6f;

    public Transform homePoint; 
    public Transform ShootingPosition;

    protected int activeState; // current state 0 - idle 1 - patrol, 2 - see target, 3 - within attack range, 4 - within suck range, -1 - death
    
    protected int health; 
    protected float spellCastLastTime;

    

    private Animator objectAnimator; 
    // Start is called before the first frame update
    protected void Start()
    {
        health = MaxHealth;
        objectAnimator = GetComponent<Animator>();
    }

    public void hit(int dmg)
    {
        Debug.Log("hit by " + dmg);
        health-=dmg;
        
        if(health < 0.1f){
            Die();
        }
        else 
            objectAnimator.SetTrigger("Hit");       
    }

    public void Die()
    {
        activeState = -1;
        objectAnimator.SetTrigger("Death");
        Destroy(gameObject,0.58f);
    }
}

// public class SpitterAI : MonoBehaviour
public class SpitterAI : EnemyBase
{

    private Vector3 startingPosition;

    private Animator anim;

    
    private int lastDirection; // last move direction
    private float nextWaypoint; // next point to patrol
    private float lastStateTime;

    public GameObject SlowBall; 

    private Transform target; // chase and attack target

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();

        startingPosition = transform.position;
        activeState = 0;
        lastDirection = 0;
        nextWaypoint = GetNextPosition();
        lastStateTime = Time.time;
    }

    // Get next idle action: stand still or go 
    float GetNextPosition()
    {        
        int nextAction = 0; // 0 - stay still, -1 go left, 1 go right        
        // Should I stay or should I go?
        if(Random.value > 0.01f)
        {
            if(lastDirection == 0)
            {
                nextAction = 1;
            }
            else {
                nextAction = lastDirection * -1;
            }
        }
        else 
        {
            nextAction = 0;
        }

        if(nextAction !=0)
        {
            float newX = PATROL_RANGE * nextAction + PATROL_RANGE * Random.value * nextAction + transform.position.x;
            //Debug.Log("Spitter: GetNextPosition decided to <" + nextAction + ">. New coordinate " + (newX));
            return newX;
        }
        else 
        {
            //Debug.Log("Spitter: GetNextPosition decided to <" + nextAction + ">");
            return transform.position.x;
        }

    }
    
    void Move()
    {
        
        Vector3 moveVelocity = Vector3.zero;
        switch(activeState)
        {
            // idle
            case 0:
                anim.SetBool("isRun", false);
                // how long are we alredy staying?
                float timeIdle = (Time.time - lastStateTime);
                if(timeIdle > 0.2f)
                {
                    // Get next action 
                    nextWaypoint = GetNextPosition();
                    // Refresh time
                    lastStateTime = Time.time;
                    // Set direction         
                    if (nextWaypoint > transform.position.x)
                    {
                        lastDirection = 1;
                        activeState = 1;                    
                    }
                    else if (nextWaypoint < transform.position.x)
                    {
                        lastDirection = -1;                    
                        activeState = 1;
                    }
                    else 
                    {
                        lastDirection = 0;
                        activeState = 0;
                    }
                }
            break;

            // move only in certain states (patrol, see range)
            case 1:
                anim.SetBool("isRun", true);
                // has we reached next waypoint?
                if((Mathf.Abs(transform.position.x - nextWaypoint) < 0.5f)) 
                {
                    //Debug.Log("waypoint reached. x= " + transform.position.x + ", waypoint = " + nextWaypoint);
                    activeState = 0;
                }
                else 
                {
                    // set moving direction vector                     
                    if(lastDirection == 1)
                        moveVelocity = Vector3.right;
                    else if(lastDirection == -1)
                        moveVelocity = Vector3.left;

                    // turn around 
                    transform.localScale = new Vector3(lastDirection*-1, transform.localScale.y, transform.localScale.z);

                    // move
                    transform.position += moveVelocity * movePower * Time.deltaTime;
                } 
                break;
            
            // chasing mode 
            case 2:
                anim.SetBool("isRun", true);
                // check if we reach casting range 
                // check if we reach bite range 
                // move to waypoint 
                if(nextWaypoint > transform.position.x)
                    moveVelocity = Vector3.right;
                else 
                    moveVelocity = Vector3.left;

                transform.position += moveVelocity * chasePower * Time.deltaTime;
                nextWaypoint = target.position.x; 

                // check if target with spell range 
                float dist = Vector3.Distance(transform.position, target.position);
                if(dist <= SPELL_RANGE && (Time.time-spellCastLastTime)>SPELL_DELAY)
                {
                    
                    castSlow();
                    spellCastLastTime = Time.time;
                }

            break;
        }

        // anim.SetBool("isRun", true);
            

    }
    
    void castSlow()
    {
        Debug.Log("Casting slow ");
        Instantiate(SlowBall, ShootingPosition.transform.position, transform.rotation);
    }

    // check if there are any enemies withing seeing range
    void CheckForEnemies()
    {        
        //RaycastHit2D vis = CircleCast(transform.position, SEE_RANGE)
        // cast circle to check if there is somebody
        // loop through hits. Raycast to every hit to check if it is visible
    }

    // Update is called once per frame
    void Update()
    {
        Move();    
    }

    public void ObjectDetected(Collider2D obj)
    {
        if(activeState == 0 || activeState == 1 || activeState == 2)
        {            
            activeState = 2; // switch to chase state 
            target = obj.transform; // set target
            nextWaypoint = obj.transform.position.x;// set waypoint to player 
        }
        // Debug.Log("Chasing player at " + nextWaypoint);     
    }

    public void ObjectLost(Collider2D obj)
    {        
        activeState = 1;// switch to patrol mode, leaving next waypoint untouched (last seen position)
        target = null;
    }
}
