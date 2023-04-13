using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugCheckPlayerInSightRange : MyNode
{
    private Transform _transform;

    private static int playerLayerMask = 1 << 3;

    //private EnemyCombat _enemyCombat;

    public ThugCheckPlayerInSightRange(Transform transform)
    {
        _transform = transform;
        //_enemyCombat = _transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        object target = GetData("Target");
        if (target == null)
        {
            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(_transform.position, ThugBT.sightRange, playerLayerMask);

            if (playerColliders.Length > 0)
            {
                parent.parent.SetData("target", playerColliders[0].transform);
                //_enemyCombat.spottedPlayer = true;
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
