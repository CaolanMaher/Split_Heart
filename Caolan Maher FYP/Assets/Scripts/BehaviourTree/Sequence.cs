using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{

    // Sequence acts as an AND logic gate
    // it only succeeds if all child nodes succeed

    public class Sequence : MyNode
    {

        public Sequence() : base() { }
        public Sequence(List<MyNode> children) : base(children) { }

        public override NodeState Evaluate()
        {
            // iterate through the children and check their state
            // if any child fails then we stop and return the failed state
            // else, keep processing the children

            bool anyChildIsRunning = false;

            foreach(MyNode child in children)
            {
                switch(child.Evaluate())
                {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        anyChildIsRunning = true;
                        continue;
                    default:
                        state = NodeState.SUCCESS;
                        return state;
                }
            }

            state = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }
    }
}