using System.Collections.Generic;
using BehaviourTree;
using UnityEngine;

public class ThugBT : MyTree
{

    //public BanditData BTData;

    //private Rigidbody2D rigidbody;
    public Rigidbody2D enemyRigidBody;
    public Transform wallDetector;
    //public Transform higherWallDetector;
    public Transform floorDetector;
    //public Transform playerDetector;
    public Transform attackPoint;
    public LayerMask wallLayerMask;
    public LayerMask playerLayerMask;
    //public LayerMask enemyLayerMask;
    public GameObject healthBarObject;

    //public int direction = -1;

    //public bool isAttacking = false;

    public static float movementSpeed = 1.5f;

    //public static float runningSpeed = 3.5f;

    public static float sightRange = 4f;

    public static float attackRange = 1.5f;

    //public static float checkNearbyEnemiesDistance = 10f;

    //public static float checkIfInGroupRange = 1f;

    protected override MyNode SetupTree()
    {
        //MyNode root = new BanditTaskPatrol(transform, enemyRigidBody, wallDetector, floorDetector, wallLayerMask, healthBarObject);
        MyNode root = new Selector(new List<MyNode>
        {
            /*
            new Sequence(new List<MyNode>
            {
                new CheckPlayerInAttackRange(transform),

                new Selector(new List<MyNode>
                {
                    new Sequence(new List<MyNode>
                    {
                        new CheckPlayerIsAttacking(transform),
                        new BanditTaskBlockAttack(transform)
                    }),

                    new BanditTaskAttack(transform, lightAttackPoint, playerLayerMask)
                })
            }),

            new Sequence(new List<MyNode>
            {
                new CheckPlayerInSightRange(transform),

                new Selector(new List<MyNode>
                {
                    new Sequence(new List<MyNode>
                    {
                        new CheckIfNotInGroup(transform, enemyLayerMask),
                        new CheckEnemiesNearby(transform, enemyLayerMask),
                        new BanditTaskGoToNearbyEnemy(transform, wallDetector, higherWallDetector, wallLayerMask)
                    }),

                    new BanditTaskGoToTarget(transform, wallDetector, higherWallDetector, wallLayerMask)
                })
            }),
            */

            new Sequence(new List<MyNode>
            {
                new ThugCheckPlayerInSightRange(transform),
                new ThugCheckGroundBeneath(transform, floorDetector),
                new ThugTaskGoToTarget(transform)
            }),

            new ThugTaskPatrol(transform, enemyRigidBody, wallDetector, floorDetector, wallLayerMask, healthBarObject)
        });

        return root;
    }
}