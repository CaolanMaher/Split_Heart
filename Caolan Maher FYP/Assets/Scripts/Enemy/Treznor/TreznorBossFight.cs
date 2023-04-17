using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreznorBossFight : MonoBehaviour
{

    public bool bossFightStarted = false;

    public GameObject player;

    public GameObject bullet;

    Animator anim;

    bool isFacingRight;

    FollowRoute followRoute;

    GameObject doors;

    // Shooting Down References
    public Transform attackPointDown;
    float shootingForce = 10f;

    // Shooting Normally References
    public Transform attackPoint;

    // Ground Pound References
    float groundPoundSpeed = 1.5f;

    // Health References
    public GameObject healthBarObject;
    public Slider healthBarValue;
    public GameObject bossName;
    private float maxHealth = 2000;
    private float currentHealth = 2000;
    private bool isAlive = true;
    float flashCooldown = 0.5f;

    // Attacks
    // Shoot 3 bullets in a spread pattern
    // Ground Pound
    // Jump while shooting

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        followRoute = GetComponent<FollowRoute>();

        healthBarObject = GameObject.FindGameObjectWithTag("Treznor_Health_Bar");
        healthBarValue = healthBarObject.GetComponent<Slider>();
        bossName = GameObject.FindGameObjectWithTag("Treznor_Name");

        doors = GameObject.FindGameObjectWithTag("Boss_Doors");
        doors.SetActive(false);

        healthBarObject.SetActive(false);
        bossName.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (bossFightStarted)
        {
            if (player.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(2f, 2f, 2f);
                isFacingRight = false;
            }
            else
            {
                transform.localScale = new Vector3(-2f, 2f, 2f);
                isFacingRight = true;
            }
        }
    }

    public void PrepareToShoot()
    {
        // decide how many times to shoot??



        anim.SetTrigger("shoot");
    }

    public void Shoot()
    {

        anim.ResetTrigger("shoot");

        // create bullet
        GameObject newBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        // make bullet travel and face direction of Treznor
        if (isFacingRight)
        {
            newBullet.transform.eulerAngles = new Vector3(0f, 180f, 0f);
            newBullet.GetComponentInChildren<Rigidbody2D>().AddForce(transform.right * shootingForce, ForceMode2D.Impulse);
        }
        else
        {
            //newBullet.transform.eulerAngles = new Vector3(0f, 180f, 0f);
            newBullet.GetComponentInChildren<Rigidbody2D>().AddForce(-(transform.right) * shootingForce, ForceMode2D.Impulse);
        }

        followRoute.ShouldJump();
    }

    public void ShootDown()
    {
        // create bullet
        GameObject newBullet = Instantiate(bullet, attackPointDown.position, Quaternion.identity);

        // make bullet travel and face down
        newBullet.transform.eulerAngles = new Vector3(0f, 0f, 90f);
        newBullet.GetComponentInChildren<Rigidbody2D>().AddForce(Vector2.down * shootingForce, ForceMode2D.Impulse);
    }

    //public void GroundPound()
    //{
    //    transform.Translate(Vector2.down * groundPoundSpeed * Time.deltaTime);
    //}

    public void StartBossFight()
    {
        if (!bossFightStarted)
        {

            bossFightStarted = true;

            player = GameObject.FindGameObjectWithTag("Player");

            healthBarObject.SetActive(true);
            bossName.SetActive(true);
            healthBarValue.maxValue = maxHealth;
            healthBarValue.minValue = 0;
            healthBarValue.value = maxHealth;
            currentHealth = maxHealth;

            //doors = GameObject.FindGameObjectWithTag("Boss_Doors");

            doors.SetActive(true);

        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        healthBarValue.value = currentHealth;

        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Flash());
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
            //anim.SetBool("hasJustTakenDamage", false);
        }
    }

    void Die()
    {

        healthBarObject.SetActive(false);
        bossName.SetActive(false);
        doors.SetActive(false);

        Destroy(gameObject);
    }
}
