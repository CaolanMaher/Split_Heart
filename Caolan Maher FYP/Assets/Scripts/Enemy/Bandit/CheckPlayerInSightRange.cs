using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

using BehaviourTree;

public class CheckPlayerInSightRange : MyNode
{
    private Transform banditTransform;

    private static int playerLayerMask = 1 << 3;

    public CheckPlayerInSightRange(Transform transform)
    {
        banditTransform = transform;
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
            }
            else
            {
                state = NodeState.FAILURE;
                return state;
            }
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
