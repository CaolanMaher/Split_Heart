using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckPlayerInAttackRange : MyNode
{
    private static int playerMaskLayer = 1 << 3;

    private Transform _transform;
    private Animator anim;

    public CheckPlayerInAttackRange(Transform transform)
    {
        _transform = transform;
        anim = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if(t == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)t;

        if(Vector2.Distance(_transform.position, target.position) < BanditBT.attackRange)
        {
            // play attack animation
            anim.SetBool("isLightAttacking", true);
            anim.SetBool("isRunning", false);

            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            anim.SetBool("isLightAttacking", false);
            anim.SetBool("isRunning", true);

            state = NodeState.FAILURE;
            return state;
        }
    }
}
