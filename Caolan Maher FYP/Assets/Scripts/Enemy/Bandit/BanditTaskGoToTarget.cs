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

    Animator anim;
    AnimatorStateInfo info;

    public BanditTaskGoToTarget(Transform transform, Transform wallDetector, Transform higherWallDetector, LayerMask floorLayerMask)
    {
        banditTransform = transform;
        enemyCombat = transform.GetComponent<EnemyCombat>();

        _wallDetector = wallDetector;
        _higherWallDetector = higherWallDetector;

        _floorLayerMask = floorLayerMask;

        anim = banditTransform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        info = anim.GetCurrentAnimatorStateInfo(0);

        if (Vector2.Distance(banditTransform.position, target.position) > 1f)
        {

            // Go Towards Target
            if(target.position.x > banditTransform.position.x && enemyCombat.canTurn)
            {
                if (!info.IsName("Bandit_Light_Attack"))
                {
                    banditTransform.localScale = new Vector3(-1.25f, 1.25f, 1.25f);

                    directionToMove = -(Vector2.right);
                    directionForRays = -banditTransform.right;

                    enemyCombat.JustChangedDirection();
                }
            }
            else if(target.position.x < banditTransform.position.x && enemyCombat.canTurn)
            {
                if (!info.IsName("Bandit_Light_Attack"))
                {
                    banditTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

                    directionToMove = Vector2.right;
                    directionForRays = banditTransform.right;

                    enemyCombat.JustChangedDirection();
                }
            }

            if (info.IsName("Bandit_Run"))
            {
                banditTransform.Translate(directionToMove * BanditBT.runningSpeed * Time.deltaTime);
                isLowerWallDetectorHittingWall = Physics2D.Raycast(_wallDetector.position, directionForRays, wallCheckDistance, _floorLayerMask);
                isHigherWallDetectorHittingWall = Physics2D.Raycast(_higherWallDetector.position, directionForRays, wallCheckDistance, _floorLayerMask);
            }

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
