using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{

    // Selector acts as an OR logic gate
    // it succeeds if any child succeeds

    public class Selector : MyNode
    {
        public Selector() : base() { }
        public Selector(List<MyNode> children) : base(children) { }

        public override NodeState Evaluate()
        {
            // iterate through the children and check their state
            // if any child succeeds then we stop and return the success state
            // else, keep processing the children

            foreach (MyNode child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        return state;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.SUCCESS;
            return state;
        }
    }
}