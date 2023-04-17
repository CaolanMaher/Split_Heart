using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{

    //public BanditData BTData;

    GameObject player;

    private bool isAlive = true;

    [SerializeField] private Transform isGroundedChecker_1;
    [SerializeField] private Transform isGroundedChecker_2;

    private float groundCheckDistance = 0.2f;

    [SerializeField] private LayerMask groundLayer;

    Rigidbody2D rb;

    private float jumpForce = 55f;

    public bool justJumped = false;

    //[SerializeField] private Transform playerDetector;

    [SerializeField] private LayerMask playerMask;

    private float playerDetectorRange = 5.0f;

    private float attackRange = 1.5f;

    public Slider healthBarValue;
    public GameObject healthBarObject;
    //public GameObject canvas;

    public float maxHealth = 250;
    float currentHealth;

    float flashCooldown = 0.5f;

    public bool isAttacking = false;
    public bool canBeAttacked = true;
    public bool canBlock = true;
    public bool canTurn = true;

    public bool spottedPlayer = false;

    public bool isGrounded = false;

    private Animator anim;
    AnimatorStateInfo info;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBarValue.maxValue = maxHealth;
        healthBarValue.minValue = 0;
        healthBarValue.value = healthBarValue.maxValue;
        //currentHealth = healthBarValue.value;

        healthBarObject.SetActive(false);

        anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(transform.localScale.x < 0)
        {
            healthBarObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            healthBarObject.transform.localScale = new Vector3(1, 1, 1);
        }

        CheckIsGrounded();
    }

    private void FixedUpdate()
    {
        if (justJumped)
        {
            justJumped = false;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void CheckIsGrounded()
    {

        if (isGroundedChecker_1 && isGroundedChecker_2)
        {
            isGrounded = Physics2D.Raycast(isGroundedChecker_1.position, -transform.up, groundCheckDistance, groundLayer)
                || Physics2D.Raycast(isGroundedChecker_2.position, -transform.up, groundCheckDistance, groundLayer);
        }
    }

    public void Jump()
    {
        justJumped = true;
    }

    public void TakeDamage(float damage)
    {

        info = anim.GetCurrentAnimatorStateInfo(0);

        //if (canBeAttacked)
        if(!info.IsName("Bandit_Block"))
        {
            //anim.SetBool("isBlocking", false);
            //anim.SetBool("hasJustTakenDamage", true);

            //Debug.Log("ATTACKED");

            // show health bar
            healthBarObject.SetActive(true);

            // take damage
            currentHealth -= damage;
            healthBarValue.value = currentHealth;

            // check if enemy should die
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // flash enemy
                StartCoroutine(Flash());
            }
        }
        else
        {
            //print("Launched Player");

            if(player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            // direction from enemy to player
            Vector2 directionFromEnemyToPlayer = (player.transform.position - transform.position).normalized;

            // launch player back
            player.GetComponent<Player>().LaunchBack(directionFromEnemyToPlayer);
        }
    }

    IEnumerator Flash()
    {
        if (isAlive)
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }

        float whenToStopFlashing = Time.time + flashCooldown;
        while (Time.time < whenToStopFlashing && isAlive)
        {
            GetComponent<SpriteRenderer>().enabled = !GetComponent<SpriteRenderer>().enabled;
            yield return new WaitForSeconds(0.15f);
        }
        if (isAlive)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            anim.SetBool("hasJustTakenDamage", false);
        }
    }

    public void AllowBlock()
    {
        canBlock = true;
    }

    public void JustChangedDirection()
    {
        canTurn = false;
        Invoke("AllowTurn", 0.25f);
    }

    public void AllowTurn()
    {
        canTurn = true;
    }

    // This is called at the end of the block animation
    public void StopBlock()
    {
        //canBeAttacked = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            print("PLAYER");
            Vector2 directionToPlayer = (transform.position - collision.transform.position).normalized;
            collision.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
            collision.GetComponent<Rigidbody2D>().AddForce(directionToPlayer * 10f, ForceMode2D.Impulse);
        }
    }

    void Die()
    {
        isAlive = false;
        // disable enemy
        StopCoroutine(Flash());

        //player.KilledEnemy();
        GameObject.FindWithTag("Player").GetComponent<Player>().KilledEnemy();

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        /*
        if (playerDetector == null)
        {
            return;
        }
        */
        //Gizmos.DrawWireSphere(playerDetector.position, playerDetectorRange);
        //Gizmos.DrawWireSphere(playerDetector.position, attackRange);

        Gizmos.DrawWireSphere(transform.position, 6f);
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
