using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskGoToTarget : MyNode
{

    private Transform banditTransform;
    private EnemyCombat enemyCombat;

    private Transform _wallDetector;
    private Transform _higherWallDetector;

    private LayerMask _floorLayerMask;

    Vector2 directionToMove;
    Vector2 directionForRays;

    bool isLowerWallDetectorHittingWall = false;
    bool isHigherWallDetectorHittingWall = false;

    float wallCheckDistance = 1f;

    public BanditTaskGoToTarget(Transform transform, Transform wallDetector, Transform higherWallDetector, LayerMask floorLayerMask)
    {
        banditTransform = transform;
        enemyCombat = transform.GetComponent<EnemyCombat>();

        _wallDetector = wallDetector;
        _higherWallDetector = higherWallDetector;

        _floorLayerMask = floorLayerMask;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if(Vector2.Distance(banditTransform.position, target.position) > 1f)
        {

            // Go Towards Target
            if(target.position.x > banditTransform.position.x && enemyCombat.canTurn)
            {
                banditTransform.localScale = new Vector3(-1.25f, 1.25f, 1.25f);
                //banditTransform.Translate(-(Vector2.right) * BanditBT.movementSpeed * Time.deltaTime);
                //enemyCombat.Invoke("AllowBlock", 1f);

                directionToMove = -(Vector2.right);
                directionForRays = -banditTransform.right;

                //isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, -banditTransform.right, wallCheckDistance, _floorLayerMask);
                //isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, -banditTransform.right, wallCheckDistance, _floorLayerMask);

                enemyCombat.JustChangedDirection();
            }
            else if(target.position.x < banditTransform.position.x && enemyCombat.canTurn)
            {
                banditTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                //banditTransform.Translate(Vector2.right * BanditBT.movementSpeed * Time.deltaTime);

                directionToMove = Vector2.right;
                directionForRays = banditTransform.right;

                //isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, banditTransform.right, wallCheckDistance, _floorLayerMask);
                //isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, banditTransform.right, wallCheckDistance, _floorLayerMask);

                enemyCombat.JustChangedDirection();
            }

            banditTransform.Translate(directionToMove * BanditBT.runningSpeed * Time.deltaTime);
            isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, directionForRays, wallCheckDistance, _floorLayerMask);
            isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, directionForRays, wallCheckDistance, _floorLayerMask);

            // Check If Should Jump

            //isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, banditTransform.right, wallCheckDistance, _floorLayerMask);
            //isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, banditTransform.right, wallCheckDistance, _floorLayerMask);

            //Debug.Log("LOWER: " + isLowerWallDetectorHittingWall + " HIGHER: " + isHigherWallDetectorHittingWall);

            // should be grounded, lower wall check should be true, high wall check should be false, target y is greater than enemy y
            if (enemyCombat.isGrounded && isLowerWallDetectorHittingWall && !isHigherWallDetectorHittingWall && !enemyCombat.justJumped)
            {
                // Should Jump
                enemyCombat.Jump();
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

}
