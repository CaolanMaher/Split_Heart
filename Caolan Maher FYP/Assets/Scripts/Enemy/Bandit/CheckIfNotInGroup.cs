using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckIfNotInGroup : MyNode
{
    Transform _transform;
    LayerMask _enemyLayerMask;
    EnemyCombat _enemyCombat;

    public CheckIfNotInGroup(Transform transform, LayerMask enemyLayerMask)
    {
        _transform = transform;
        _enemyLayerMask = enemyLayerMask;
        _enemyCombat = _transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(_transform.position, BanditBT.checkIfInGroupRange, _enemyLayerMask);

        if(nearbyEnemies.Length > 0)
        {
            foreach(Collider2D enemy in nearbyEnemies)
            {
                if(enemy.transform.root != _transform.root)
                {
                    Debug.Log("In Group");

                    _enemyCombat.spottedPlayer = false;

                    state = NodeState.FAILURE;
                    return state;
                }
            }
            Debug.Log("Only Found Self");
            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            Debug.Log("Not In Group");
            state = NodeState.SUCCESS;
            return state;
        }
    }
}
