using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugCheckPlayerInAttackRange : MyNode
{

    private Transform _transform;
    private Animator anim;
    //private EnemyCombat enemyCombat;

    public ThugCheckPlayerInAttackRange(Transform transform)
    {
        _transform = transform;
        anim = transform.GetComponent<Animator>();
        //enemyCombat = transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if (t == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)t;

        if (Vector2.Distance(_transform.position, target.position) < ThugBT.attackRange)
        {
            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            //anim.SetBool("isAttacking", false);

            state = NodeState.FAILURE;
            return state;
        }
    }
}
