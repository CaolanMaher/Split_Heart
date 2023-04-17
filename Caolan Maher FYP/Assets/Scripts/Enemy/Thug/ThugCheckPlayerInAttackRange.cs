using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugCheckPlayerInAttackRange : MyNode
{

    private Transform _transform;
    private Animator anim;
    AnimatorStateInfo info;
    private EnemyCombat enemyCombat;

    Vector2 directionToMove;

    public ThugCheckPlayerInAttackRange(Transform transform)
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

        Transform target = (Transform)t;

        info = anim.GetCurrentAnimatorStateInfo(0);

        if (Vector2.Distance(_transform.position, target.position) < ThugBT.attackRange)
        {

            if (target.position.x > _transform.position.x && enemyCombat.canTurn)
            {
                if (!info.IsName("Thug_Attack"))
                {

                    _transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);

                    directionToMove = -(Vector2.right);

                    enemyCombat.JustChangedDirection();
                }
            }
            else if (target.position.x < _transform.position.x && enemyCombat.canTurn)
            {
                if (!info.IsName("Thug_Attack"))
                {

                    _transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                    directionToMove = Vector2.right;

                    enemyCombat.JustChangedDirection();
                }
            }

            if (!info.IsName("Thug_Attack"))
            {
                _transform.Translate(directionToMove * ThugBT.movementSpeed * Time.deltaTime);
            }

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
