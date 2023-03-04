using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    // for prototype only
    public GameObject mainCamera;

    private bool isAlive = true;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;

    public Transform attackPoint;
    private float attackRange = 0.75f;
    public LayerMask enemyLayers;
    public int attackDamage = 50;

    private float attackRate = 2f;
    private float nextAttackTime = 0f;

    private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private float hurtCooldown = 2f;
    private float nextHurtTime = 0f;

    private int flashCooldown = 1;

    private float movementSpeed = 7.0f;

    private bool isFacingRight;

    // Wall Detection
    public Transform wallCheck;
    float wallCheckDistance = 0.5f;
    private bool isTouchingWall;

    [SerializeField] private LayerMask groundLayer;

    // Ledge Detection

    public Transform ledgeCheck;
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;

    private Vector2 ledgePositionBottom;
    private Vector2 ledgePosition1;
    private Vector2 ledgePosition2;

    public Vector2 ledgeClimbOffset1;
    public Vector2 ledgeClimbOffset2;

    /*
    public bool ledgeDetected;

    [SerializeField] private Vector2 offset1;
    [SerializeField] private Vector2 offset2;

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;
    private bool canClimb;
    */

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

        // Delete Later
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);

        Movement();

        CheckSurroundings();

        CheckLedgeClimb();

        //AnimatorControlleVariables();

        //CheckForLedge();

        //float attack = Input.GetAxisRaw("Fire1");

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate; // add half a second onto current time, means we can attack next in half a second
            }
        }
    }

    void CheckSurroundings()
    {
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, groundLayer);

        if(isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePositionBottom = wallCheck.position;
        }
    }

    // This method is called at the end of the climbing animation
    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePosition2;

        ledgeDetected = false;

        playerAnimator.SetBool("canClimb", canClimbLedge);
    }

    void CheckLedgeClimb()
    {
        if(ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if(isFacingRight)
            {
                ledgePosition1 = new Vector2(Mathf.Floor(ledgePositionBottom.x + wallCheckDistance) - ledgeClimbOffset1.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset1.y);
                ledgePosition2 = new Vector2(Mathf.Floor(ledgePositionBottom.x + wallCheckDistance) + ledgeClimbOffset2.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset2.y);
            }
            else
            {
                ledgePosition1 = new Vector2(Mathf.Ceil(ledgePositionBottom.x - wallCheckDistance) + ledgeClimbOffset1.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset1.y);
                ledgePosition2 = new Vector2(Mathf.Ceil(ledgePositionBottom.x - wallCheckDistance) - ledgeClimbOffset2.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset2.y);
            }

            playerAnimator.SetBool("canClimb", canClimbLedge);
        }

        if(canClimbLedge)
        {
            transform.position = ledgePosition1;
        }
    }

    /*
    void AnimatorControlleVariables()
    {
        playerAnimator.SetBool("canClimb", canClimb);
    }

    private void CheckForLedge()
    {
        if(ledgeDetected && canGrabLedge)
        {
            canGrabLedge = false;

            Vector2 ledgePosition = GetComponentInChildren<LedgeDetection>().transform.position;

            climbBegunPosition = ledgePosition + offset1;
            climbOverPosition = ledgePosition + offset2;

            canClimb = true;
        }

        if(canClimb)
        {
            transform.position = climbBegunPosition;
        }
    }

    // This method is called by a trigger at the end of the climb animation
    void LedgeClimbOver()
    {
        canClimb = false;
        transform.position = climbOverPosition;
        Invoke("AllowLedgeGrab", 0.1f);
    }

    void AllowLedgeGrab()
    {
        canGrabLedge = true;
    }
    */

    /*
    void FixedUpdate()
    {
        
    }
    */

    void Movement()
    {
        float dirX = Input.GetAxis("Horizontal");

        if (dirX < 0)
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                180,
                transform.eulerAngles.z
            );

            //if (playerRigidbody.velocity.x < 0)
            //{
                playerAnimator.SetBool("isRunning", true);
            //}

            isFacingRight = false;
        }
        else if (dirX > 0)
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                0,
                transform.eulerAngles.z
            );

            //if (playerRigidbody.velocity.x > 0)
            //{
                playerAnimator.SetBool("isRunning", true);
            //}

            isFacingRight = true;
        }
        else
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                transform.eulerAngles.z
            );

            playerAnimator.SetBool("isRunning", false);
        }

        playerRigidbody.velocity = new Vector2(dirX * movementSpeed, playerRigidbody.velocity.y);

        if (Input.GetButtonDown("Jump"))
        {
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 20f);
        }
    }

    void Attack()
    {
        // Play animation
        playerAnimator.SetTrigger("attack");

        // Check nearby enemies
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage enemies
        foreach(Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<EnemyCombat>().Attacked(attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (Time.time >= nextAttackTime)
        {
            currentHealth -= damage;
            nextHurtTime = Time.time + 1f / hurtCooldown;
        }

        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // flash enemy
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
        }
    }

    void Die()
    {
        isAlive = false;
        GetComponent<SpriteRenderer>().enabled = false;
        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
