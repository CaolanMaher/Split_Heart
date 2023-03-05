using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskAttack : MyNode
{

    private Animator anim;

    private float attackCooldown = 1f;
    private float attackTimer = 0;
    
    public BanditTaskAttack(Transform transform)
    {
        anim = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        attackTimer += Time.deltaTime;

        if(attackTimer >= attackCooldown)
        {
            attackTimer = 0;
            Debug.Log("Attacking Player");
        }

        state = NodeState.RUNNING;
        return state;
    }

}
