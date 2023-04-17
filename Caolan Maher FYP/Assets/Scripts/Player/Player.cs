using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    // for prototype only
    //public GameObject mainCamera;

    public PlayManager playManager;

    // General References

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private AnimatorStateInfo playerStateInfo;

    // Health / Combat

    [SerializeField] private bool isAttacking = false;

    private float maxHealth = 150;
    [SerializeField] private float currentHealth;
    private bool canBeHit = true;

    private float hurtCooldown = 0.75f;
    private float hurtTimer = 0;

    private int flashCooldown = 1;

    public float totalDamageTaken;

    // Attacking

    public Transform attackPoint;
    private float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public LayerMask bossLayer;
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

    private bool takeUserMoveInput = true;

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
    bool canTransform = false;
    bool isTransforming = false;
    //float transformCooldown = 10f;
    //float transformCooldownTimer = 0f;
    //float timeInFurySide = 0f;
    //float furySideLength = 15f;

    float maxFuryCharge = 15f;
    float currentFuryCharge = 0f;

    // UI & HUD References

    public Slider healthBar;
    public Slider furyBar;

    // Music & SFX

    GameObject backgroundMusic;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        backgroundMusic = GameObject.FindGameObjectWithTag("Background_Music");

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Level_2")
        {
            maxHealth = PlayerPrefs.GetFloat("MaxHealth");
            currentHealth = PlayerPrefs.GetFloat("CurrentHealth");
            currentFuryCharge = PlayerPrefs.GetFloat("CurrentFuryCharge");

            healthBar.maxValue = maxHealth;
            healthBar.minValue = 0;
            healthBar.value = currentHealth;

            furyBar.maxValue = maxFuryCharge;
            furyBar.minValue = 0;
            furyBar.value = currentFuryCharge;
        }
        else
        {
            currentHealth = maxHealth;
            healthBar.maxValue = maxHealth;
            healthBar.minValue = 0;
            healthBar.value = maxHealth;

            currentFuryCharge = 0f;
            furyBar.maxValue = maxFuryCharge;
            furyBar.minValue = 0;
            furyBar.value = currentFuryCharge;
        }
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
            currentFuryCharge -= Time.deltaTime;
            furyBar.value = currentFuryCharge;

            if(furyBar.value <= 0 && !canClimbLedge && isGrounded)
            {
                currentFuryCharge = 0;
                furyBar.value = currentFuryCharge;
                TransformToNormal();
            }
        }

        if(!isFurySide && !canTransform)
        {
            currentFuryCharge += Time.deltaTime * 0.1f;
            furyBar.value = currentFuryCharge;

            if (currentFuryCharge >= furyBar.maxValue)
            {
                currentFuryCharge = maxFuryCharge;
                furyBar.value = currentFuryCharge;
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

        // only if fury bar is full
        if (Input.GetKeyDown(KeyCode.X) && canTransform && !canClimbLedge && isGrounded && furyBar.value >= furyBar.maxValue)
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

        if (canMove && takeUserMoveInput)
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

        //if(isAttacking && playerAnimator.GetBool("isInAir"))
        //{
        //    takeUserMoveInput = true;
        //}

        if (canMove)
        {
            playerRigidbody.velocity = new Vector2(moveHorizontal * movementSpeed * Time.deltaTime, playerRigidbody.velocity.y);
        }
        else if (!canMove && !takeUserMoveInput)
        {
            playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);
        }

        if(justJumped)
        {
            justJumped = false;
            playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // player is in air
        if(!isGrounded)
        {
            playerAnimator.SetBool("isInAir", true);
        }
        else
        {
            playerAnimator.SetBool("isInAir", false);
        }
    }

    void Attack()
    {
        isAttacking = true;

        //canMove = false;

        takeUserMoveInput = false;

        if (isGrounded)
        {
            canMove = false;
        }
        else
        {
            // cut current momentum by half
            //playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x / 2, playerRigidbody.velocity.y / 2);
            takeUserMoveInput = true;
        }

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

        /*
        // Check nearby enemies
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage enemies
        foreach(Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<EnemyCombat>().TakeDamage(attackDamage);
        }
        */
    }

    // This is called during each attack animation
    public void AttackEnemies()
    {
        // Check nearby enemies
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage enemies
        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<EnemyCombat>().TakeDamage(attackDamage);
        }

        // Check boss hit
        Collider2D[] bossHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, bossLayer);

        // Damage enemies
        foreach (Collider2D boss in bossHit)
        {
            boss.GetComponent<TreznorBossFight>().TakeDamage(attackDamage);
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
        takeUserMoveInput = true;
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
        takeUserMoveInput = false;
        canAttack = false;

        //canTransform = false;

        // change stats and timers
        //timeInFurySide = 0;

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
        takeUserMoveInput = false;
        canAttack = false;

        // reset stats and timers
        //timeInFurySide = 0;

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

            if (currentHealth <= 0)
            {
                healthBar.value = 0;
                Die();
            }
            else
            {
                // flash player
                StartCoroutine(Flash());
            }

            totalDamageTaken += damage;
        }
    }

    public void AddHealth(int health)
    {
        currentHealth += health;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthBar.value = currentHealth;
    }

    public void LaunchBack(Vector2 directionFromEnemyToPlayer)
    {

        takeUserMoveInput = true;

        playerRigidbody.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        playerRigidbody.AddForce(directionFromEnemyToPlayer * 10, ForceMode2D.Impulse);
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

    public void KilledEnemy()
    {
        
        if(currentFuryCharge < maxFuryCharge)
        {
            currentFuryCharge += maxFuryCharge / 10;
            furyBar.value = currentFuryCharge;
        }
    }

    void Die()
    {
        isAlive = false;
        GetComponent<SpriteRenderer>().enabled = false;
        enabled = false;

        backgroundMusic.GetComponent<BackgroundMusicFade>().FadeOut();

        playManager.PlayerDied();
    }

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    // Getters

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentFuryCharge()
    {
        return currentFuryCharge;
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
