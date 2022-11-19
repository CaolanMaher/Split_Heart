using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{

    private Animator animator;

    private bool isAlive = true;

    [SerializeField] private Transform playerDetector;

    [SerializeField] private LayerMask playerMask;

    private float playerDetectorRange = 5.0f;

    private float attackRange = 1.5f;

    //private bool canSeePlayer = false;

    private bool canAttack = true;

    private float attackCooldown = 2.0f;

    public Slider healthBar;
    public GameObject healthBarObject;
    public GameObject canvas;

    private int maxHealth = 100;
    int currentHealth;

    int attackDamage = 50;

    int flashCooldown = 1;

    private bool isAttacking;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.value = maxHealth;

        canvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (GetComponent<EnemyMovement>().GetIsMovingRight())
        {
            Vector3 origin = playerDetector.position;
            Vector3 rayCastDirection = Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, playerDetectorRange, playerMask);
            if (hit.collider && canAttack)
            {
                print("Found Player");
                Attack();
            }
        }
        else
        {
            Vector3 origin = playerDetector.position;
            Vector3 rayCastDirection = -Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, playerDetectorRange, playerMask);
            if (hit.collider && canAttack)
            {
                print("Found Player");
                Attack();
            }
        }
        */

        Attack();
    }

    public void Attacked(int damage)
    {
        if(Random.Range(0, 10) >= 5) // 1 in 2 chance to block attack
        {
            // attack blocked
            print("Blockked");
        }
        else
        {
            canvas.SetActive(true);
            // take damage
            currentHealth -= damage;
            healthBar.value = currentHealth;

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
        }
    }

    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            collision.gameObject.GetComponent<PlayerMovement>().TakeDamage(attackDamage);
        }
    }
    */

    IEnumerator StartAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void Attack()
    {
        //print("Attacking Player");
        isAttacking = true;
        // Stop moving
        GetComponent<EnemyMovement>().StopMoving();

        if (GetComponent<EnemyMovement>().GetIsMovingRight())
        {
            Vector3 origin = playerDetector.position;
            Vector3 rayCastDirection = Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, attackRange, playerMask);
            if (hit.collider)
            {
                //print("Hit Player");
                hit.collider.gameObject.GetComponent<PlayerMovement>().TakeDamage(attackDamage);
            }
            StartCoroutine(StartAttackCooldown());
        }
        else
        {
            Vector3 origin = playerDetector.position;
            Vector3 rayCastDirection = -Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, attackRange, playerMask);
            if (hit.collider)
            {
                //print("Hit Player");
                hit.collider.gameObject.GetComponent<PlayerMovement>().TakeDamage(attackDamage);
            }
            StartCoroutine(StartAttackCooldown());
        }
        isAttacking = false;
        GetComponent<EnemyMovement>().StartMoving();
    }

    void Die()
    {
        isAlive = false;
        // disable enemy
        StopCoroutine(Flash());
        GetComponent<SpriteRenderer>().enabled = false;
        healthBarObject.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        enabled = false;
    }

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerDetector == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(playerDetector.position, playerDetectorRange);
        Gizmos.DrawWireSphere(playerDetector.position, attackRange);
    }
}
