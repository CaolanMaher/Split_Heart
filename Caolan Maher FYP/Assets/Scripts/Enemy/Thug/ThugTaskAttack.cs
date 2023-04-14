using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugTaskAttack : MyNode
{
    private static int _playerLayerMask = 1 << 3;

    private Animator anim;
    private AnimatorStateInfo info;
    private Transform _transform;
    private Transform _attackPoint;

    private float lightAttackRadius = 0.3f;

    public ThugTaskAttack(Transform transform, Transform attackPoint)
    {
        anim = transform.GetComponent<Animator>();
        _transform = transform;
        _attackPoint = attackPoint;
    }

    public override NodeState Evaluate()
    {
        info = anim.GetCurrentAnimatorStateInfo(0);

        if(!info.IsName("Thug_Attack"))
        {
            anim.SetTrigger("attack");
        }

        Transform target = (Transform)GetData("target");

        if (info.IsName("Thug_Attack") && info.normalizedTime % 1 > 0.8 && info.IsName("Thug_Attack") && info.normalizedTime % 1 < 0.85)
        {
            Collider2D player = Physics2D.OverlapCircle(_attackPoint.position, lightAttackRadius, _playerLayerMask);

            if (player != null)
            {
                player.GetComponent<Player>().TakeDamage(40);
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

}
