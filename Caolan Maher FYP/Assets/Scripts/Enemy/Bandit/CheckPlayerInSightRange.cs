using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class CheckPlayerInSightRange : MyNode
{
    private Transform banditTransform;

    private static int playerLayerMask = 1 << 3;

    private EnemyCombat _enemyCombat;

    public CheckPlayerInSightRange(Transform transform)
    {
        banditTransform = transform;
        _enemyCombat = banditTransform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        object target = GetData("Target");
        if(target == null)
        {
            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(banditTransform.position, BanditBT.sightRange, playerLayerMask);

            if(playerColliders.Length > 0)
            {
                parent.parent.SetData("target", playerColliders[0].transform);
                _enemyCombat.spottedPlayer = true;
            }
            else if(_enemyCombat.spottedPlayer)
            {
                state = NodeState.SUCCESS;
                return state;
            }
            else
            {
                state = NodeState.FAILURE;
                return state;
            }
        }

        //Debug.Log("SUCCESS");

        state = NodeState.SUCCESS;
        return state;
    }
}
