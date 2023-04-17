using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugTaskPatrol : MyNode
{

    private Rigidbody2D enemyRigidBody;
    Animator anim;
    AnimatorStateInfo info;

    private Transform transform;
    private int direction = -1;
    private bool isMovingRight;
    //private float movementSpeed = 2.0f;
    private bool canChangeDirection = true;
    private float directionChangeCooldown = 0.5f;
    private float directionChangeTimer;

    private Transform wallDetector;
    private Transform floorDetector;

    private float wallDetectorRange = 1.0f;
    private float floorDetectorRange = 1.0f;
    LayerMask wallLayerMask;

    EnemyCombat enemyCombat;

    public ThugTaskPatrol(Transform banditTransform, Rigidbody2D banditRigidBody, Transform banditWallDetector, Transform banditFloorDetector, LayerMask banditWallMask, GameObject banditHealthBar)
    {
        transform = banditTransform;
        enemyRigidBody = banditRigidBody;
        wallDetector = banditWallDetector;
        floorDetector = banditFloorDetector;
        wallLayerMask = banditWallMask;
        enemyCombat = transform.GetComponent<EnemyCombat>();

        anim = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {

        Movement();
        DetectFloorBeneath();
        DetectWallCollision();

        if (!canChangeDirection)
        {
            directionChangeTimer += Time.deltaTime;
            if (directionChangeTimer >= directionChangeCooldown)
            {
                directionChangeTimer = 0;
                canChangeDirection = true;
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

    void Movement()
    {

        info = anim.GetCurrentAnimatorStateInfo(0);

        if (!info.IsName("Thug_Attack"))
        {

            if (direction == 1)
            {
                isMovingRight = true;
                transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
                transform.Translate(transform.right * ThugBT.movementSpeed * Time.deltaTime);
            }
            else
            {
                isMovingRight = false;
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                transform.Translate((-transform.right) * ThugBT.movementSpeed * Time.deltaTime);
            }
        }
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

                enemyCombat.JustChangedDirection();
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

                enemyCombat.JustChangedDirection();
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

            enemyCombat.JustChangedDirection();
        }
    }

}
