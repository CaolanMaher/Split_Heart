using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckIfShouldBlock : MyNode
{

    private Transform _transform;
    private Animator anim;
    private EnemyCombat enemyCombat;

    bool numberCheckDone = false;
    float numberCheckTimer = 0f;

    public CheckIfShouldBlock(Transform transform)
    {
        _transform = transform;
        anim = transform.GetComponent<Animator>();
        enemyCombat = transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if (t == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        if(numberCheckDone)
        {
            numberCheckTimer += Time.deltaTime;

            if(numberCheckTimer > 1f)
            {
                numberCheckTimer = 0f;
                numberCheckDone = false;
            }
        }

        Transform target = (Transform)t;

        if (!numberCheckDone)
        {
            int number = Random.Range(1, 11);
            numberCheckDone = true;

            if (number >= 4)
            {
                state = NodeState.SUCCESS;
                return state;
            }
            else
            {
                //enemyCombat.canBeAttacked = true;

                state = NodeState.FAILURE;
                return state;
            }
        }

        //enemyCombat.canBeAttacked = true;

        state = NodeState.FAILURE;
        return state;
    }
}
