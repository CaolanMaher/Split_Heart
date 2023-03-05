using System.Collections.Generic;
using BehaviourTree;
using UnityEngine;

public class BanditBT : MyTree
{

    //private Rigidbody2D rigidbody;
    public Rigidbody2D enemyRigidBody;
    public Transform wallDetector;
    public Transform floorDetector;
    public LayerMask wallLayerMask;
    public GameObject healthBarObject;

    public static float movementSpeed = 2f;

    public static float sightRange = 6f;

    public static float attackRange = 1f;

    protected override MyNode SetupTree()
    {
        //MyNode root = new BanditTaskPatrol(transform, enemyRigidBody, wallDetector, floorDetector, wallLayerMask, healthBarObject);
        MyNode root = new Selector(new List<MyNode>
        {
            new Sequence(new List<MyNode>
            {
                new CheckPlayerInAttackRange(transform),
                new BanditTaskAttack(transform)
            }),
            new Sequence(new List<MyNode>
            {
                new CheckPlayerInSightRange(transform),
                new BanditTaskGoToTarget(transform)
            }),
            new BanditTaskPatrol(transform, enemyRigidBody, wallDetector, floorDetector, wallLayerMask, healthBarObject)
        });

        return root;
    }
}
