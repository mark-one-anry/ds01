using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int MaxHealth = 10;
    public float movePower = 2f;
    public float chasePower = 7f;
    public float PATROL_RANGE = 2500f; // distance to patrol
    public float SEE_RANGE = 250f;
    public float SPELL_RANGE = 2.5f;
    public float SPELL_DELAY = 6f;

    public Transform homePoint; 
    public Transform ShootingPosition;

    protected int activeState; // current state 0 - idle 1 - patrol, 2 - see target, 3 - within attack range, 4 - bite, -1 - death
    
    protected float health; 
    protected float spellCastLastTime;

    public ManaBarScript manaSlider;

    private Animator objectAnimator; 
    // Start is called before the first frame update
    protected void Start()
    {
        health = MaxHealth;
        objectAnimator = GetComponent<Animator>();

        // if mana bar is defined - initialize it 
        if (manaSlider)
        {
            manaSlider.SetMaxMana(MaxHealth);
        }
    }

    public void hit(int dmg)
    {
        Debug.Log("hit by " + dmg);
        health-=dmg;
        activeState = 0;

        // update mana bar
        manaSlider.SetMana(health);

        if (health < 0.1f){
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
    private bool facingRight; // смотрит ли наш объект направо?

    private float nextWaypoint; // next point to patrol
    private float lastStateTime;

    public GameObject SlowBall;
    public float BITE_RANGE = 1;    // расстояние, на котором можно делать кусь 
    public float BITE_DELAY = 2;    // как часто кусаем (в секундах)
    public float BITE_POWER = 2;    // сила укуса

    //private Transform target; // chase and attack target
    private GameObject target; // chase and attack target

    private BottomWallCheckScript bottomWallCheck;
    private BottomWallCheckScript topWallCheck;

    protected GameObject biteTarget;    // цель для Кусь
    protected float lastBiteTime;       // время последнего Кусь

    private bool isBiting; // уведомили класс игрока, что мы кусаемся

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();

        startingPosition = transform.position;
        activeState = 0;
        // lastDirection = 0;
        facingRight = false; 

        nextWaypoint = GetNextPosition();
        lastStateTime = Time.time;
        spellCastLastTime = 0;

        bottomWallCheck = transform.Find("BottomWallCheck").GetComponent<BottomWallCheckScript>();
        topWallCheck = transform.Find("TopWallCheck").GetComponent<BottomWallCheckScript>();

        lastBiteTime = 0;
        isBiting = false;
    }

    // Get next idle action: stand still or go 
    float GetNextPosition(int dir = 0)
    {        
        int nextAction = 0; // 0 - stay still, -1 go left, 1 go right        
        
        // Если направление уже задано
        if(dir != 0)        
        {
            nextAction = dir;
        }
        // Should I stay or should I go?        
        else if(Random.value > 0.01f)
        {
            // if(lastDirection == 0)
            // Будем двигаться. В какую сторону?
            if(Random.value > 0.5f)            
                nextAction = 1; //вправо
            else {
                nextAction = -1; //влево
            }
        }
        else 
        {
            nextAction = 0; // стоим на месте
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
        //float oldDir = lastDirection;
        bool oldFacingRight = facingRight; //сохраним направление, чтобы флипнуть слизня если надо
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
                        //lastDirection = 1;
                        facingRight = true;
                        activeState = 1;                                            
                    }
                    else if (nextWaypoint < transform.position.x)
                    {
                        //lastDirection = -1;
                        facingRight = false;                    
                        activeState = 1;
                        
                    }
                    else 
                    {
                        // lastDirection = 0;
                        activeState = 0;
                    }
                    // если направление поменялось - повернться
                    if(facingRight != oldFacingRight)
                        transform.Rotate(0f,180f,0f);
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
                    //if(lastDirection == 1)
                    
                    
                    if(facingRight)
                        moveVelocity = Vector3.right;
                    //else if(lastDirection == -1)
                    else
                        moveVelocity = Vector3.left;
                    
                    //moveVelocity = Vector3.right;
                    // transform.localScale = new Vector3(lastDirection*-1, transform.localScale.y, transform.localScale.z);
                    
                    // Перед нами стена?
                    if(bottomWallCheck.isTouching && topWallCheck.isTouching){
                        // развернуться
                        facingRight=!facingRight;
                        transform.Rotate(0f,180f,0f);                    
                        nextWaypoint = GetNextPosition(facingRight ? -1 : 1);
                    }
                    else 
                    {
                        // move
                        transform.position += moveVelocity * movePower * Time.deltaTime;
                    }
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
                // nextWaypoint = target.position.x; 
                nextWaypoint = target.transform.position.x; 

                // check if target with spell range 
                //float dist = Vector3.Distance(transform.position, target.position);
                float dist = Vector3.Distance(transform.position, target.transform.position);
                // Debug.Log("Distance " + dist);

                // Близко для куся?
                if (dist <= BITE_RANGE )
                {
                    Debug.Log("Ready to bite target");
                    activeState = 4;
                    biteTarget = target;
                }
                    

                if (dist <= SPELL_RANGE && (Time.time-spellCastLastTime)>SPELL_DELAY)
                {
                    
                    castSlow();
                    spellCastLastTime = Time.time;
                }

            break;

            // режим кусь
            case 4:
                // уведомить класс игрока, если еще этого не сделали 
                if(!isBiting)
                {
                    target.SendMessage("addSucker", gameObject);
                    isBiting = true;
                }
                // когда последний раз кусали?
                if(Time.time - lastBiteTime >= BITE_DELAY)
                {
                    target.SendMessage("hit", BITE_POWER);
                    if (health < MaxHealth)
                    {
                        health += (float)BITE_POWER;
                    }
                    health = health > MaxHealth ? MaxHealth : health;   // не превышать максимального запаса

                    lastBiteTime = Time.time;

                    // update mana bar
                    manaSlider.SetMana(health);
                }
               
                //transform.position = target.transform.position;
                Vector3 pos = target.transform.position;
                pos.x -= (float)0.5;
                transform.position = pos;
                break;
        }

        // anim.SetBool("isRun", true);
            

    }
    
    void castSlow()
    {        
        GameObject obj = Instantiate(SlowBall, ShootingPosition.transform.position, transform.rotation);
        obj.SendMessage("setTarget", target);       
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
            //target = obj.transform; // set target
            target = obj.gameObject;
            nextWaypoint = obj.transform.position.x;// set waypoint to player 
        }
        // Debug.Log("Chasing player at " + nextWaypoint);     
    }

    public void ObjectLost(Collider2D obj)
    {
        if (activeState == 1 || activeState == 2)
        {
            activeState = 1;// switch to patrol mode, leaving next waypoint untouched (last seen position)
            target = null;
        }
    }

    public void hit(int dmg)
    {
        if (isBiting)
        {
            target.SendMessage("removeSucker", gameObject);
            isBiting = false;
        }
        base.hit(dmg);
    }
}
