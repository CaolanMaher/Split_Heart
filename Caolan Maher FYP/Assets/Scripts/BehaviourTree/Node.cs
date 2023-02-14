using System.Collections;
using System.Collections.Generic;


namespace BehaviourTree
{

    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class Node
    {
        // protected allows inherited objects (children) to access and modify
        protected NodeState state;

        public Node parent;
        protected List<Node> children = new List<Node>();

        // string is used to mark the name of the data
        // using object as the value allows for any type of object to be stored
        private Dictionary<string, object> dataContext = new Dictionary<string, object>();

        public Node()
        {
            parent = null;
        }

        public Node(List<Node> children)
        {
            foreach(Node child in children)
            {
                Attach(child);
            }
        }

        private void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        // can be overridden so each node has its own evaluation function
        public virtual NodeState Evaluate() => NodeState.FAILURE;

        public void SetData(string key, object value)
        {
            dataContext[key] = value;
        }

        public object Getdata(string key)
        {
            object value = null;
            if (dataContext.TryGetValue(key, out value))
            {
                return value;
            }

            // continue working up the branch until we have found the value
            // or until we get to the root of the tree

            Node node = parent;
            while(node != null)
            {
                value = node.Getdata(key);
                if(value != null)
                {
                    return value;
                }
                node = node.parent;
            }
            return null;
        }

        public bool Cleardata(string key)
        {
            if (dataContext.ContainsKey(key))
            {
                dataContext.Remove(key);
                return true;
            }

            // continue working up the branch until we have found the key
            // or until we get to the root of the tree

            Node node = parent;
            while (node != null)
            {
                bool cleared = node.Cleardata(key);
                if (cleared)
                {
                    return true;
                }
                node = node.parent;
            }
            return false;
        }
    }

}