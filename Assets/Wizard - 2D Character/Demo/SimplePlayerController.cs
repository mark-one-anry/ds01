using UnityEngine;


namespace ClearSky
{
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

        public int maxMana = 10;
        
        private int currentMana;
        public ManaBarScript manaSlider;

        private bool facingDirectionRight;
        // few constants
        private int ATTACK_COST = 1;
        private int HURT_COST = 3;


        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

            // if mana bar is defined - initialize it 
            currentMana = maxMana;
            if(manaSlider)
            {
                manaSlider.SetMaxMana(maxMana);
            }
            facingDirectionRight = true;
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

            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Wizard: OnTriggerEnter2D");
            anim.SetBool("isJump", false);
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
    }
}