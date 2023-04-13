using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugTaskGoToTarget : MyNode
{

    Transform _transform;
    private EnemyCombat enemyCombat;

    //private Transform _wallDetector;
    //private Transform _higherWallDetector;

    private int wallLayerMask = 1 << 7;

    Vector2 directionToMove;
    //Vector2 directionForRays;

    //bool isLowerWallDetectorHittingWall = false;
    //bool isHigherWallDetectorHittingWall = false;

    //float wallCheckDistance = 1f;

    public ThugTaskGoToTarget(Transform transform)
    {
        _transform = transform;
        enemyCombat = transform.GetComponent<EnemyCombat>();

        //_wallDetector = wallDetector;
        //_higherWallDetector = higherWallDetector;

        //_floorLayerMask = floorLayerMask;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if (Vector2.Distance(_transform.position, target.position) > 1f)
        {

            //Debug.Log("GOING TO ENEMY");

            //Vector2 directionToEnemy = (_transform.position - nearbyEnemy.position).normalized;
            //_transform.Translate(directionToEnemy * BanditBT.movementSpeed * Time.deltaTime);

            if (target.position.x > _transform.position.x)
            {
                _transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
                //_transform.Translate(-(Vector2.right) * BanditBT.movementSpeed * Time.deltaTime);

                directionToMove = -(Vector2.right);
                //directionForRays = -_transform.right;

                //isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, -_transform.right, wallCheckDistance, _floorLayerMask);
                //isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, -_transform.right, wallCheckDistance, _floorLayerMask);

                //enemyCombat.Invoke("AllowBlock", 1f);

                enemyCombat.JustChangedDirection();
            }
            else if (target.position.x < _transform.position.x)
            {
                _transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                //_transform.Translate(Vector2.right * BanditBT.movementSpeed * Time.deltaTime);

                directionToMove = Vector2.right;
                //directionForRays = _transform.right;

                //isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, _transform.right, wallCheckDistance, _floorLayerMask);
                //isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, _transform.right, wallCheckDistance, _floorLayerMask);

                enemyCombat.JustChangedDirection();
            }

            _transform.Translate(directionToMove * ThugBT.movementSpeed * Time.deltaTime);

            //isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, directionForRays, wallCheckDistance, _floorLayerMask);
            //isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, directionForRays, wallCheckDistance, _floorLayerMask);

            //Vector2.MoveTowards(banditTransform.position, target.position, BanditBT.movementSpeed);
            //banditTransform.LookAt(target.position);

            // should be grounded, lower wall check should be true, high wall check should be false, target y is greater than enemy y
            //if (enemyCombat.isGrounded && isLowerWallDetectorHittingWall && !isHigherWallDetectorHittingWall && !enemyCombat.justJumped)
            //{
                // Should Jump
            //    enemyCombat.Jump();
            //}
        }

        state = NodeState.RUNNING;
        return state;
    }

}
