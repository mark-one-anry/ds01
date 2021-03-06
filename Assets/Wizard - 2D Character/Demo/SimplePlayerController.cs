using UnityEngine;
using UnityEngine.SceneManagement;

// Для Dictionary
using System.Collections;
using System.Collections.Generic;



public class SimplePlayerController : MonoBehaviour
{
    public float movePower = 10f;
    public float jumpPower = 15f; //Set Gravity Scale in Rigidbody2D Component to 5

    private Rigidbody2D rb;
    private Animator anim;
    Vector3 movement;
    private int direction = 1;
    bool isJumping = false;
    private bool alive = true;

    public float maxMana = 30;
        
    private float currentMana;
    public ManaBarScript manaSlider;

    public Transform shootingPosition; // точка стрельбы
    
    // Блок FIREBALL
    public GameObject FireBall;
    public float fireBallCost = 2f;
    public float FIREBALL_POWER = 3f;

    private bool facingDirectionRight;
    // few constants
    private int ATTACK_COST = 1;
    private int HURT_COST = 3;
    private bool crystalnear = false;
    private float lastStateTime;

    private float sourceMass;
    private float sourceSpeed;
    private bool slowActive;
    private float slowEndTime;

    //protected GameObject[] suckers; // массив сосущих слизней на игроке
    private int suckersNum; // количество сосущих
    Dictionary<int, GameObject> suckers = new Dictionary<int, GameObject>();
    

    // Start is called before the first frame update
    void Start()
    {
            
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        lastStateTime = Time.time;
        sourceMass = rb.mass;
        sourceSpeed = movePower;
        slowActive = false;

        // if mana bar is defined - initialize it 
        currentMana = maxMana;
        if(manaSlider)
        {
            manaSlider.SetMaxMana(maxMana);
        }
        facingDirectionRight = true;

        suckersNum = 0;

    }

    private void Update()
    {
        Restart();
        if (alive)
        {
            Hurt();
            Die();
            Attack();
            Jump();
            Run();
            CheckSpellEffects();
            SpellCasting(); // творение заклинаний

        }
        float timeIdle = (Time.time - lastStateTime);
        if (crystalnear && timeIdle > 1f) 
        {
            lastStateTime = Time.time;
            manarevive();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Wizard: OnTriggerEnter2D");
        anim.SetBool("isJump", false);
        LayerMask lm = LayerMask.GetMask("Exit");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
            //exit from level
            SceneManager.LoadScene("SceneVictory");
            //SceneManager.LoadScene("Assets/Scenes/SceneVictory",LoadSceneMode.Single);  
        }

        lm = LayerMask.GetMask("Crystal");
        if(lm == (lm | (1 << other.gameObject.layer)))
        {
                crystalnear = true;
                SpriteRenderer CrystalLight = other.GetComponentInChildren<SpriteRenderer>();
                Color CrystalLightColor = CrystalLight.color;
                CrystalLightColor.g = 100;
                CrystalLightColor.b = 0;
                CrystalLight.color = CrystalLightColor;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LayerMask lmExit = LayerMask.GetMask("Crystal");
        if(lmExit == (lmExit | (1 << other.gameObject.layer)))
        {
                crystalnear = false;
                SpriteRenderer CrystalLight = other.GetComponentInChildren<SpriteRenderer>();
                Color CrystalLightColor = CrystalLight.color;
                CrystalLightColor.g = 255;
                CrystalLightColor.b = 255;
                CrystalLight.color = CrystalLightColor;
        }

    }

    void Run()
    {
        Vector3 moveVelocity = Vector3.zero;
        anim.SetBool("isRun", false);


        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            direction = -1;
            moveVelocity = Vector3.left;

 
            //transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            if (!anim.GetBool("isJump"))
                anim.SetBool("isRun", true);

        }
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            direction = 1;
            moveVelocity = Vector3.right;

            //transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x) , transform.localScale.y, transform.localScale.z);
            if (!anim.GetBool("isJump"))
                anim.SetBool("isRun", true);

        }
        // check if we need to flip 
        float dir = Input.GetAxisRaw("Horizontal");
        if(dir !=0 )
        {
            if((facingDirectionRight && dir<0) || (!facingDirectionRight && dir>0))
            {
                Flip();
            }
        }
        transform.position += moveVelocity * movePower * Time.deltaTime;
    }

    void Flip()
    {
        facingDirectionRight = !facingDirectionRight;
        transform.Rotate(0f,180f,0f);
    }
    void Jump()
    {
        if ((Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0)
        && !anim.GetBool("isJump"))
        {
            isJumping = true;
            anim.SetBool("isJump", true);
        }
        if (!isJumping)
        {
            return;
        }

        rb.velocity = Vector2.zero;

        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

        isJumping = false;
    }
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            anim.SetTrigger("attack");

            // subtract mana for attack 
            currentMana-=ATTACK_COST;

            // update mana bar
            manaSlider.SetMana(currentMana);
        }
    }
    void Hurt()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anim.SetTrigger("hurt");
            if (direction == 1)
                rb.AddForce(new Vector2(-5f, 1f), ForceMode2D.Impulse);
            else
                rb.AddForce(new Vector2(5f, 1f), ForceMode2D.Impulse);

            // subtract mana for attack 
            currentMana-=HURT_COST;

            // update mana bar
            manaSlider.SetMana(currentMana);
        }
    }
    void Die()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) || currentMana<1)
        {
            anim.SetTrigger("die");
            alive = false;
            SceneManager.LoadScene("SceneDead");  
        }
    }
    void Restart()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            anim.SetTrigger("idle");
            alive = true;

            currentMana = maxMana;
            manaSlider.SetMaxMana(maxMana);
        }
    }

    public void hit(float dmg)
    {
        currentMana-=(float)dmg;
        manaSlider.SetMana(currentMana);
        anim.SetTrigger("hurt");
    }


    public void cast(float spellcost)
    {
        currentMana-=(float)spellcost;
        manaSlider.SetMana(currentMana);
        anim.SetTrigger("attack");
    }

    public void manarevive()
    {
        if (currentMana <= maxMana) 
        {
            currentMana+=1;
            manaSlider.SetMana(currentMana);
        }
    }

    public void slowEffect(float massEffect, float speedEffect, float time)
    {
        rb.mass = rb.mass * massEffect;
        movePower = movePower / speedEffect;
        slowActive = true;
        slowEndTime = Time.time + time;
    }

    private void CheckSpellEffects()
    {
        if(slowActive && Time.time >= slowEndTime)
        {
            rb.mass = sourceMass;
            movePower = sourceSpeed;
            slowActive = false;
        }
    }

    public bool isInvisible()
    {
        invisibilitySpell invSpell = GetComponent<invisibilitySpell>();
        if (invSpell != null)
        {
            return invSpell.isInvisible();
        }
        else
            return false;
    }

    protected void SpellCasting() // Вызов заклинаний
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // а не висит ли на нас сосущий?
            if(suckersNum > 0)
            {                
                // выбрать первого
                int key = 0;
                foreach (int value in suckers.Keys)
                {
                    key = value;
                    break;
                }
                // пнуть его
                suckers[key].SendMessage("hit", FIREBALL_POWER);
                // убрать его
                suckers.Remove(key);

            }
            Instantiate(FireBall, shootingPosition.transform.position, transform.rotation);
            //GetComponent<SimplePlayerController>().cast(fireBallCost);
            cast(fireBallCost);
        }
	}
    public bool addSucker(GameObject newSucker) // Добавить сосущего
    {
        // есть ли место для нового сосущего?
        if (suckersNum >= 10)
            return false;

        suckers.Add(newSucker.GetHashCode(), newSucker);
        suckersNum++;
        //suckers[suckersNum]
        return true;
        //suckers.push()
    }

    public void removeSucker(GameObject newSucker) // Убрать сосущего
    {
        suckers.Remove(newSucker.GetHashCode());
        suckersNum--;        
    }

    public void CastFireball(){
        // а не висит ли на нас сосущий?
        if(suckersNum > 0)
        {                
            // выбрать первого
            int key = 0;
            foreach (int value in suckers.Keys)
            {
                key = value;
                break;
            }
            // пнуть его
            suckers[key].SendMessage("hit", FIREBALL_POWER);
            // убрать его
            suckers.Remove(key);

        }
        Instantiate(FireBall, shootingPosition.transform.position, transform.rotation);
        //GetComponent<SimplePlayerController>().cast(fireBallCost);
        cast(fireBallCost);
    }

    public void CastInisibility(){
        gameObject.GetComponent<invisibilitySpell>().MakeMeInvisible();
    }
}
