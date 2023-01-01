using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

    private Rigidbody2D enemyRigidBody;

    private float movementSpeed = 2.0f;

    [SerializeField] private int direction = -1;

    private float changeDirectionCooldown = 0.5f;

    [SerializeField] private bool isMovingRight;

    private bool canChangeDirection = true;

    public Transform wallDetector;

    private float wallDetectorRange = 1.0f;

    [SerializeField] LayerMask wallLayerMask;

    public GameObject healthBarObject;

    // Start is called before the first frame update
    void Start()
    {
        enemyRigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        DetectWallCollision();
    }

    void Movement()
    {
        if(direction == 1)
        {
            isMovingRight = true;
        }
        else
        {
            isMovingRight = false;
        }

        if (!GetComponent<EnemyCombat>().GetIsAttacking())
        {
            enemyRigidBody.velocity = new Vector2(direction * movementSpeed, enemyRigidBody.velocity.y);
        }
    }

    void DetectWallCollision()
    {
        if(isMovingRight)
        {
            Vector3 origin = wallDetector.position;
            Vector3 rayCastDirection = Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, wallDetectorRange, wallLayerMask);
            if(hit.collider && canChangeDirection)
            {
                direction *= -1;
                // change enemy's direction
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                // stop healthbar from flipping
                //RectTransform rectTransform = healthBarObject.GetComponent<RectTransform>();
                //rectTransform.localScale = new Vector3(rectTransform.localScale.x * -1, rectTransform.localScale.y, rectTransform.localScale.z);
                StartCoroutine(StartChangeDirectionCooldown());
            }
        }
        else if(!isMovingRight)
        {
            Vector3 origin = wallDetector.position;
            Vector3 rayCastDirection = -Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, wallDetectorRange, wallLayerMask);
            if (hit.collider && canChangeDirection)
            {
                direction *= -1;
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                //RectTransform rectTransform = healthBarObject.GetComponent<RectTransform>();
                //rectTransform.localScale = new Vector3(rectTransform.localScale.x * -1, rectTransform.localScale.y, rectTransform.localScale.z);
                StartCoroutine(StartChangeDirectionCooldown());
            }
        }
    }

    
    private IEnumerator StartChangeDirectionCooldown()
    {
        canChangeDirection = false;
        yield return new WaitForSeconds(changeDirectionCooldown);
        canChangeDirection = true;
    }
    

    private void OnTriggerExit2D(Collider2D collision)
    {
        //print("TURN ENEMY " + collision.tag);
        if (collision.tag == "Tilemap" && canChangeDirection)
        {
            // when enemy gets to edge, flip enemy around
            direction *= -1;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            //canChangeDirection = false;
            StartCoroutine(StartChangeDirectionCooldown());
            //print("ENEMY TURNED");
        }
    }

    public void StopMoving()
    {
        enemyRigidBody.velocity = new Vector2(0, 0);
    }

    public void StartMoving()
    {
        enemyRigidBody.velocity = new Vector2(direction * movementSpeed, enemyRigidBody.velocity.y);
    }

    public bool GetIsMovingRight()
    {
        return isMovingRight;
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Tilemap" && canChangeDirection)
        {
            // when enemy gets to edge, flip enemy around
            direction *= -1;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            //canChangeDirection = false;
            StartCoroutine(StartChangeDirectionCooldown());
        }
    }
    */

    private void OnDrawGizmosSelected()
    {
        if (wallDetector == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(wallDetector.position, wallDetectorRange);
    }
}
