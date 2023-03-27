using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskGoToNearbyEnemy : MyNode
{

    Transform _transform;

    public BanditTaskGoToNearbyEnemy(Transform transform)
    {
        _transform = transform;
    }

    public override NodeState Evaluate()
    {
        Transform nearbyEnemy = (Transform)GetData("nearbyEnemy");

        if (Vector2.Distance(_transform.position, nearbyEnemy.position) > 1f)
        {

            Debug.Log("GOING TO ENEMY");

            //Vector2 directionToEnemy = (_transform.position - nearbyEnemy.position).normalized;
            //_transform.Translate(directionToEnemy * BanditBT.movementSpeed * Time.deltaTime);

            if (nearbyEnemy.position.x > _transform.position.x)
            {
                _transform.localScale = new Vector3(-1.25f, 1.25f, 1.25f);
                _transform.Translate(-(Vector2.right) * BanditBT.movementSpeed * Time.deltaTime);
                //enemyCombat.Invoke("AllowBlock", 1f);
            }
            else
            {
                _transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                _transform.Translate(Vector2.right * BanditBT.movementSpeed * Time.deltaTime);
            }

            //Vector2.MoveTowards(banditTransform.position, target.position, BanditBT.movementSpeed);
            //banditTransform.LookAt(target.position);
        }

        state = NodeState.RUNNING;
        return state;
    }

}
