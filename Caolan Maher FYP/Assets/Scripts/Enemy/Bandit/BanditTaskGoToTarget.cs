using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;

public class BanditTaskGoToTarget : MyNode
{

    private Transform banditTransform;
    private EnemyCombat enemyCombat;

    public BanditTaskGoToTarget(Transform transform)
    {
        banditTransform = transform;
        enemyCombat = transform.GetComponent<EnemyCombat>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if(Vector2.Distance(banditTransform.position, target.position) > 1f)
        {

            //Vector2 directionToTarget = (banditTransform.position - target.position).normalized;
            //banditTransform.Translate(directionToTarget * BanditBT.movementSpeed * Time.deltaTime);
            
            if(target.position.x > banditTransform.position.x)
            {
                banditTransform.localScale = new Vector3(-1.25f, 1.25f, 1.25f);
                banditTransform.Translate(-(Vector2.right) * BanditBT.movementSpeed * Time.deltaTime);
                //enemyCombat.Invoke("AllowBlock", 1f);
            }
            else
            {
                banditTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                banditTransform.Translate(Vector2.right * BanditBT.movementSpeed * Time.deltaTime);
            }

            //Vector2.MoveTowards(banditTransform.position, target.position, BanditBT.movementSpeed);
            //banditTransform.LookAt(target.position);
        }

        state = NodeState.RUNNING;
        return state;
    }

}
