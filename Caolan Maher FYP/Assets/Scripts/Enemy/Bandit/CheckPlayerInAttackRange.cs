using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckPlayerInAttackRange : MyNode
{
    private static int playerMaskLayer = 1 << 3;

    private Transform _transform;
    private Animator anim;
    private BanditData _data;

    public CheckPlayerInAttackRange(Transform transform, BanditData data)
    {
        _transform = transform;
        anim = transform.GetComponent<Animator>();
        _data = data;
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

            _data.isAttacking = true;

            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            anim.SetBool("isLightAttacking", false);
            anim.SetBool("isRunning", true);

            _data.isAttacking = false;

            state = NodeState.FAILURE;
            return state;
        }
    }
}
