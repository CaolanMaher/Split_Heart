using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class ThugCheckGroundBeneath : MyNode
{
    private Transform _transform;

    private Transform _floorDetector;

    private static int wallLayerMask = 1 << 7;

    private float floorDetectorRange = 1.0f;

    //private EnemyCombat _enemyCombat;

    public ThugCheckGroundBeneath(Transform transform, Transform floorDetector)
    {
        _transform = transform;
        _floorDetector = floorDetector;
        //_enemyCombat = _transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        Vector2 origin = _floorDetector.position;
        Vector2 rayCastDirection = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(origin, rayCastDirection, floorDetectorRange, wallLayerMask);

        /*
        Transform target = (Transform)GetData("target");

        if (!(target.position.y > _transform.position.y + 0.5) && !(target.position.y < _transform.position.y - 0.5))
        {
            if (hit.collider)
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
        */

        if (hit.collider)
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
}
