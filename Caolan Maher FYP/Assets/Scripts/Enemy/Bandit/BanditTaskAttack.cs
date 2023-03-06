using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskAttack : MyNode
{

    private LayerMask _playerLayerMask;

    private Animator anim;
    private AnimatorStateInfo info;
    private Transform _transform;
    private Transform _lightAttackPoint;

    private float lightAttackRadius = 0.2f;

    private int attackDamage = 25;

    private float attackCooldown = 1f;
    private float attackTimer = 0;
    
    public BanditTaskAttack(Transform transform, Transform lightAttackPoint, LayerMask playerLayerMask)
    {
        anim = transform.GetComponent<Animator>();
        _transform = transform;
        _lightAttackPoint = lightAttackPoint;
        _playerLayerMask = playerLayerMask;
    }

    public override NodeState Evaluate()
    {
        info = anim.GetCurrentAnimatorStateInfo(0);

        Transform target = (Transform)GetData("target");

        attackTimer += Time.deltaTime;

        //if(attackTimer >= attackCooldown)
        if(info.IsName("Bandit_Light_Attack") && info.normalizedTime % 1 > 0.9)
        {
            attackTimer = 0;
            //Debug.Log("Attacking Player");

            Collider2D player = Physics2D.OverlapCircle(_lightAttackPoint.position, lightAttackRadius, _playerLayerMask);

            if(player != null)
            {
                player.GetComponent<PlayerMovement>().TakeDamage(25);
            }

            /*

            RaycastHit2D player = Physics2D.Raycast(_transform.position, _transform.right * BanditBT.direction, 0.5f, _playerLayerMask);

            if(player.collider != null)
            {
                player.collider.GetComponent<PlayerMovement>().TakeDamage(25);
            }
            */
        }

        state = NodeState.RUNNING;
        return state;
    }

}
