using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskPatrol : MyNode
{

    private Rigidbody2D enemyRigidBody;

    private Transform transform;
    private int direction = -1;
    private bool isMovingRight;
    private float movementSpeed = 2.0f;
    private bool canChangeDirection = true;
    private float directionChangeCooldown = 0.5f;
    private float directionChangeTimer;

    private Transform wallDetector;
    private Transform floorDetector;

    private float wallDetectorRange = 1.0f;
    private float floorDetectorRange = 1.0f;
    LayerMask wallLayerMask;

    public BanditTaskPatrol(Transform banditTransform, Rigidbody2D banditRigidBody, Transform banditWallDetector, Transform banditFloorDetector, LayerMask banditWallMask, GameObject banditHealthBar)
    {
        transform = banditTransform;
        enemyRigidBody = banditRigidBody;
        wallDetector = banditWallDetector;
        floorDetector = banditFloorDetector;
        wallLayerMask = banditWallMask;
    }

    public override NodeState Evaluate()
    {

        Movement();
        DetectFloorBeneath();
        DetectWallCollision();

        if(!canChangeDirection)
        {
            directionChangeTimer += Time.deltaTime;
            if(directionChangeTimer >= directionChangeCooldown)
            {
                directionChangeTimer = 0;
                canChangeDirection = true;
            }
        }

        state = NodeState.RUNNING;
        return state;

        //return base.Evaluate();
    }

    void Movement()
    {

        //enemyRigidBody.velocity = new Vector2(direction * BanditBT.movementSpeed, enemyRigidBody.velocity.y);
        //Vector2.MoveTowards(transform.position, transform.forward, BanditBT.movementSpeed);

        //transform.Translate(transform.right * BanditBT.movementSpeed);

        if (direction == 1)
        {
            isMovingRight = true;
            transform.localScale = new Vector3(-1.25f, 1.25f, 1.25f);
            transform.Translate(transform.right * BanditBT.movementSpeed * Time.deltaTime);
        }
        else
        {
            isMovingRight = false;
            transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            transform.Translate((-transform.right) * BanditBT.movementSpeed * Time.deltaTime);
        }

        //if (!transform.GetComponent<EnemyCombat>().GetIsAttacking())
        //{
        //    enemyRigidBody.velocity = new Vector2(direction * movementSpeed, enemyRigidBody.velocity.y);
        //}
    }

    void DetectWallCollision()
    {
        if (isMovingRight)
        {
            Vector2 origin = wallDetector.position;
            Vector2 rayCastDirection = Vector2.right;
            Debug.DrawRay(origin, rayCastDirection);
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, wallDetectorRange, wallLayerMask);
            if (hit.collider && canChangeDirection)
            {
                direction *= -1;
                canChangeDirection = false;
                // change enemy's direction
                //transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                // stop healthbar from flipping
                //RectTransform rectTransform = healthBarObject.GetComponent<RectTransform>();
                //rectTransform.localScale = new Vector3(rectTransform.localScale.x * -1, rectTransform.localScale.y, rectTransform.localScale.z);
                //StartCoroutine(StartChangeDirectionCooldown());
            }
        }
        else if (!isMovingRight)
        {
            Vector2 origin = wallDetector.position;
            Vector2 rayCastDirection = -Vector2.right;
            Debug.DrawRay(origin, rayCastDirection);
            RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, wallDetectorRange, wallLayerMask);
            if (hit.collider && canChangeDirection)
            {
                direction *= -1;
                canChangeDirection = false;
                //transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                //RectTransform rectTransform = healthBarObject.GetComponent<RectTransform>();
                //rectTransform.localScale = new Vector3(rectTransform.localScale.x * -1, rectTransform.localScale.y, rectTransform.localScale.z);
                //StartCoroutine(StartChangeDirectionCooldown());
            }
        }
    }

    void DetectFloorBeneath()
    {
        Vector2 origin = floorDetector.position;
        Vector2 rayCastDirection = Vector2.down;
        Debug.DrawRay(origin, rayCastDirection);
        RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, floorDetectorRange, wallLayerMask);
        if ((!hit.collider) && canChangeDirection)
        {
            direction *= -1;
            canChangeDirection = false;
            //transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            // stop healthbar from flipping
            //RectTransform rectTransform = healthBarObject.GetComponent<RectTransform>();
            //rectTransform.localScale = new Vector3(rectTransform.localScale.x * -1, rectTransform.localScale.y, rectTransform.localScale.z);
            //StartCoroutine(StartChangeDirectionCooldown());
        }
    }

}
