using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskBlockAttack : MyNode
{

    Transform _transform;

    public BanditTaskBlockAttack(Transform transform)
    {
        _transform = transform;
    }

    public override NodeState Evaluate()
    {

        Debug.Log("Blocked Attack");

        state = NodeState.RUNNING;
        return state;
    }
}
