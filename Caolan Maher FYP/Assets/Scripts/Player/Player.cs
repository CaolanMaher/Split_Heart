using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    // for prototype only
    //public GameObject mainCamera;

    // General References

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private AnimatorStateInfo playerStateInfo;

    // Health / Combat

    [SerializeField] private bool isAttacking = false;

    private float maxHealth = 100;
    [SerializeField] private float currentHealth;
    private bool canBeHit = true;

    private float hurtCooldown = 0.75f;
    private float hurtTimer = 0;

    private int flashCooldown = 1;

    // Attacking

    public Transform attackPoint;
    private float attackRange = 0.75f;
    public LayerMask enemyLayers;
    public float attackDamage = 50;

    private bool canAttack = true;

    private float attackRate = 2f;
    private float attackTimer = 0f;

    // Movement

    private float moveHorizontal;

    private float movementSpeed = 350;
    private float jumpForce = 15f;

    private bool isFacingRight;

    private bool isAlive = true;

    private bool justJumped;

    private bool canTurn = true;

    private bool canMove = true;

    // Ground Detection

    public Transform groundCheck_1;
    public Transform groundCheck_2;

    [SerializeField] private bool isGrounded;

    private float groundCheckDistance = 0.2f;

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
    private bool isGroundBeneath;

    private float detectGroundBeneathDistance = 1.5f;

    private Vector2 ledgePositionBottom;
    private Vector2 ledgePosition1;
    private Vector2 ledgePosition2;

    public Vector2 ledgeClimbOffset1;
    public Vector2 ledgeClimbOffset2;

    // Fury References

    bool isFurySide = false;
    bool canTransform = true;
    bool isTransforming = false;
    float transformCooldown = 10f;
    float transformCooldownTimer = 0f;
    float timeInFurySide = 0f;
    float furySideLength = 15f;

    // UI & HUD References

    public Slider healthBar;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        currentHealth = maxHealth;
        healthBar.value = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.minValue = 0;
    }

    // Update is called once per frame
    void Update()
    {

        // Delete Later
        //mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);

        MovementCalculations();

        CheckSurroundings();

        CheckInputs();

        CheckLedgeClimb();

        DoTimers();

        playerStateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        //AnimatorControlleVariables();

        //CheckForLedge();

        //float attack = Input.GetAxisRaw("Fire1");
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void DoTimers()
    {
        if(!canBeHit)
        {
            hurtTimer += Time.deltaTime;

            if(hurtTimer >= hurtCooldown)
            {
                hurtTimer = 0;
                canBeHit = true;
            }
        }

        if (!canAttack && !isTransforming)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer > 0.1)
            {
                attackTimer = 0;
                canAttack = true;
            }
        }

        if(isFurySide)
        {
            timeInFurySide += Time.deltaTime;

            if(timeInFurySide >= furySideLength)
            {
                timeInFurySide = 0;
                TransformToNormal();
            }
        }

        if(!isFurySide && !canTransform)
        {
            transformCooldownTimer += Time.deltaTime;

            if(transformCooldownTimer >= transformCooldown)
            {
                //print("CAN TRANSFORM");
                transformCooldownTimer = 0;
                canTransform = true;
            }
        }
    }

    void CheckInputs()
    {

        if(Input.GetKeyDown(KeyCode.Mouse0) && canAttack && !canClimbLedge)
        {
            canAttack = false;
            Attack();
        }

        if(Input.GetKeyDown(KeyCode.X) && canTransform && !canClimbLedge)
        {
            canTransform = false;
            TransformToFury();
        }
    }

    void CheckSurroundings()
    {
        isGrounded = Physics2D.Raycast(groundCheck_1.position, -transform.up, groundCheckDistance, groundLayer) 
            || Physics2D.Raycast(groundCheck_2.position, -transform.up, groundCheckDistance, groundLayer);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, groundLayer);

        isGroundBeneath = Physics2D.Raycast(wallCheck.position, -transform.up, detectGroundBeneathDistance, groundLayer);

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected && !isGrounded && !isGroundBeneath)
        {
            ledgeDetected = true;
            ledgePositionBottom = wallCheck.position;

            canTurn = false;
            canMove = false;
        }
    }

    // This method is called at the end of the climbing animation
    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePosition2;

        ledgeDetected = false;

        canTurn = true;
        canMove = true;

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

    void MovementCalculations()
    {

        if (canMove)
        {

            float dirX = Input.GetAxis("Horizontal");

            moveHorizontal = Input.GetAxisRaw("Horizontal");

            if (dirX < 0 && canTurn)
            {
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    180,
                    transform.eulerAngles.z
                );
                playerAnimator.SetBool("isRunning", true);

                isFacingRight = false;
            }
            else if (dirX > 0 && canTurn)
            {
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    0,
                    transform.eulerAngles.z
                );
                playerAnimator.SetBool("isRunning", true);

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

            if (Input.GetButtonDown("Jump") && !justJumped && isGrounded)
            {
                justJumped = true;
            }
        }
    }

    void MovePlayer()
    {
        if (canMove)
        {
            playerRigidbody.velocity = new Vector2(moveHorizontal * movementSpeed * Time.deltaTime, playerRigidbody.velocity.y);
        }
        else
        {
            playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);
        }

        if(justJumped)
        {
            justJumped = false;
            playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void Attack()
    {
        isAttacking = true;

        canMove = false;

        if ((playerStateInfo.IsName("Player_Attack_Move_1") || playerStateInfo.IsName("Player_Attack_Move_1_Fury")) && playerStateInfo.normalizedTime % 1 > 0.35)
        {
            // Play first move animation
            playerAnimator.SetTrigger("performComboMove");
        }
        else if((playerStateInfo.IsName("Player_Attack_Move_2") || playerStateInfo.IsName("Player_Attack_Move_2_Fury")) && playerStateInfo.normalizedTime % 1 > 0.35)
        {
            // Play first move animation
            playerAnimator.SetTrigger("attack");
        }
        else if (!playerStateInfo.IsName("Player_Attack_Move_1") && !playerStateInfo.IsName("Player_Attack_Move_1_Fury"))
        {
            // Play combo move animation
            playerAnimator.SetTrigger("attack");
        }

        // Check nearby enemies
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage enemies
        foreach(Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<EnemyCombat>().TakeDamage(attackDamage);
        }
    }

    // This is called at the end of the attack animation
    public void ResetAttackBool()
    {
        isAttacking = false;
        ResetMoveBool();
    }

    // This is called at the end of both transformation animations and when ResetAttackBool is called
    public void ResetMoveBool()
    {
        canMove = true;
    }

    // This is called at the end of both transformation animations
    public void ResetTransformBool()
    {
        isTransforming = false;
        ResetMoveBool();
    }

    public void TransformToFury()
    {
        isTransforming = true;

        playerAnimator.SetTrigger("transformToFury");
        playerAnimator.SetBool("isFury", true);

        isFurySide = true;

        canMove = false;
        canAttack = false;

        //canTransform = false;

        // change stats and timers
        timeInFurySide = 0;

        movementSpeed *= 1.25f;
        attackDamage *= 1.5f;
    }

    public void TransformToNormal()
    {
        isTransforming = true;

        //print("BACK TO NORMAL");
        playerAnimator.SetTrigger("transformToNormal");

        playerAnimator.SetBool("isFury", false);

        isFurySide = false;

        canMove = false;
        canAttack = false;

        // reset stats and timers
        timeInFurySide = 0;

        movementSpeed /= 1.25f;
        attackDamage /= 1.5f;
    }

    public void TakeDamage(int damage)
    {

        if(canBeHit)
        {
            canBeHit = false;

            currentHealth -= damage;

            healthBar.value = currentHealth;
        }

        if(currentHealth <= 0)
        {
            healthBar.value = 0;
            Die();
        }
        else
        {
            // flash player
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

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    private void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        //Gizmos.DrawWireSphere(transform.position, 6f);
    }
}
