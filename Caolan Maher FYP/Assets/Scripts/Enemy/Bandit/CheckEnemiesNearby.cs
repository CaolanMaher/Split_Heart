using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckEnemiesNearby : MyNode
{

    Transform _transform;
    LayerMask _enemyLayerMask;

    public CheckEnemiesNearby(Transform transform, LayerMask enemyLayerMask)
    {
        _transform = transform;
        _enemyLayerMask = enemyLayerMask;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(_transform.position, BanditBT.checkNearbyEnemiesDistance, _enemyLayerMask);

        if(nearbyEnemies.Length > 0)
        {

            Transform target = (Transform)GetData("target");

            foreach (Collider2D enemy in nearbyEnemies)
            {
                if (enemy.transform.root != _transform.root) {

                    // check if the player is closer than the nearest enemy
                    if (Vector2.Distance(_transform.position, target.position) > Vector2.Distance(_transform.position, enemy.transform.position) && enemy.transform.position.y <= _transform.position.y + 2.5)
                    {
                        parent.parent.SetData("nearbyEnemy", enemy.transform);

                        Debug.Log("Found Suitable Enemy at " + enemy.transform.position.x);
                        state = NodeState.SUCCESS;
                        return state;
                    }
                }
            }
            Debug.Log("No Enemies Suitable");
            state = NodeState.FAILURE;
            return state;
        }
        else
        {
            Debug.Log("Found No Enemy");
            state = NodeState.FAILURE;
            return state;
        }
    }
}
