using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugTaskGoToTarget : MyNode
{

    Transform _transform;
    private EnemyCombat enemyCombat;
    Animator _anim;

    private int wallLayerMask = 1 << 7;

    AnimatorStateInfo info;

    Vector2 directionToMove;

    public ThugTaskGoToTarget(Transform transform)
    {
        _transform = transform;
        _anim = _transform.GetComponent<Animator>();
        enemyCombat = transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        info = _anim.GetCurrentAnimatorStateInfo(0);

        if (Vector2.Distance(_transform.position, target.position) > 1f)
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
        }

        state = NodeState.RUNNING;
        return state;
    }

}
