using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{

    //public BanditData BTData;

    private bool isAlive = true;

    [SerializeField] private Transform playerDetector;

    [SerializeField] private LayerMask playerMask;

    private float playerDetectorRange = 5.0f;

    private float attackRange = 1.5f;

    public Slider healthBarValue;
    public GameObject healthBarObject;
    public GameObject canvas;

    private float maxHealth = 100;
    float currentHealth;

    float flashCooldown = 0.5f;

    public bool isAttacking = false;
    public bool canBeAttacked = true;
    public bool canBlock = true;

    public bool spottedPlayer = false;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBarValue.value = maxHealth;

        healthBarObject.SetActive(false);

        anim = GetComponent<Animator>();
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
    }

    public void TakeDamage(float damage)
    {
        if (canBeAttacked)
        {

            //anim.SetBool("isBlocking", false);
            anim.SetBool("hasJustTakenDamage", true);

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

    void Die()
    {
        //Debug.Log("Killed");
        isAlive = false;
        // disable enemy
        StopCoroutine(Flash());
        //GetComponent<SpriteRenderer>().enabled = false;
        //healthBarObject.SetActive(false);
        //GetComponent<Collider2D>().enabled = false;
        //enabled = false;
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
