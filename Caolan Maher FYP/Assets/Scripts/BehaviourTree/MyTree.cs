using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class MyTree : MonoBehaviour
    {
        private MyNode root = null;

        protected void Start()
        {
            root = SetupTree();
        }

        private void Update()
        {
            if(root != null)
            {
                root.Evaluate();
            }
        }

        protected abstract MyNode SetupTree();
    }

}