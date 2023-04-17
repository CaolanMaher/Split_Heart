using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskBlockAttack : MyNode
{

    Transform _transform;
    EnemyCombat enemyCombat;
    //BanditData _data;
    Animator anim;
    //AnimatorStateInfo info;

    public BanditTaskBlockAttack(Transform transform)
    {
        _transform = transform;
        enemyCombat = transform.GetComponent<EnemyCombat>();
        //_data = data;
        anim = _transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        enemyCombat.canBeAttacked = false;
        //_data.canBeAttacked = false;
        //Debug.Log("Blocked Attack");

        anim.SetTrigger("block");
        //anim.SetBool("isBlocking", true);

        //anim.SetBool("isLightAttacking", false);
        //anim.SetBool("isRunning", false);

        /*
        info = anim.GetCurrentAnimatorStateInfo(0);

        if(info.IsName("Bandit_Block"))
        {
            //Debug.Log("Blocked Attack");
            _data.canBeAttacked = false;
        }
        else
        {
            _data.canBeAttacked = true;
        }
        */

        state = NodeState.RUNNING;
        return state;
    }
}
