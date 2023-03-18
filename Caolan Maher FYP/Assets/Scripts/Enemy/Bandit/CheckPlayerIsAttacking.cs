using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckPlayerIsAttacking : MyNode
{
    private Transform _transform;
    private Animator anim;
    private EnemyCombat enemyCombat;
    //private BanditData _data;

    bool numberCheckDone = false;
    //bool attackCheckDone = false;

    float boolTimer = 0;

    public CheckPlayerIsAttacking(Transform transform)
    {
        _transform = transform;
        anim = transform.GetComponent<Animator>();
        enemyCombat = transform.GetComponent<EnemyCombat>();
        //_data = data;
        //Debug.Log("TESTING");
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

        int number = Random.Range(1, 11);

        if(number >= 4 && !numberCheckDone)
        {
            Debug.Log("NUMBER CHECKED");
            enemyCombat.canBeAttacked = true;
            numberCheckDone = true;
        }
        else if(number < 4 && !numberCheckDone)
        {
            Debug.Log("NUMBER CHECKED");
            enemyCombat.canBeAttacked = false;
            numberCheckDone = true;
        }

        if(numberCheckDone)
        {
            boolTimer += Time.deltaTime;

            if(boolTimer > 0.2f)
            {
                //attackCheckDone = false;
                numberCheckDone = false;
            }
        }

        if (target.GetComponent<PlayerMovement>().GetIsAttacking() && !enemyCombat.canBeAttacked)
        {
            Debug.Log("Blocking");

            //anim.SetBool("isBlocking", true);
            //anim.SetBool("isLightAttacking", false);
            //anim.SetBool("isRunning", false);

            //_data.canBeAttacked = false;
            enemyCombat.isAttacking = false;
            //enemyCombat.canBeAttacked = false;

            //attackCheckDone = true;

            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            Debug.Log("Testing");

            //anim.SetBool("isBlocking", false);
            //anim.SetBool("isLightAttacking", true);
            //anim.SetBool("isRunning", false);

            enemyCombat.isAttacking = true;

            state = NodeState.FAILURE;
            return state;
        }
    }
}
