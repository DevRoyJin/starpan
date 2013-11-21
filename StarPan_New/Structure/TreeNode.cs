using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StarPan.Structure
{
    public class TreeNode<T>
    {

        private IList<TreeNode<T>> _children;

        public TreeNode()
        {
            _children = new List<TreeNode<T>>();
        }

        public TreeNode(T data):this()
        {
            Data = data;
        }

        public T Data
        {
            get;
            private set;

        }

        public TreeNode<T> Parent
        {
            get;
            private set;

        }

        public int Layer
        {
            get
            {
                int i = 1;
                var parent = this.Parent;
                while(parent!=null)
                {
                    i++;
                    parent = parent.Parent;
                }
                return i;
            }
        }

        public bool HasChild
        {
            get
            {
                return _children.Any();
            }
        }

        public TreeNode<T> AddChild(T data)
        {
            var node = new TreeNode<T>(data);
            this._children.Add(node);
            node.Parent = this;
            return node;
        }

        public void RemoveChild(TreeNode<T> child)
        {
            child.Parent = null;
            _children.Remove(child);
        }

        public void RemoveSelf()
        {
            if (this.Parent != null)
            {
                this.Parent.RemoveChild(this);
                this.Parent = null;
            }
        }

        public TreeNode<T> GetChild(Func<T, bool> condition)
        {
            return _children.FirstOrDefault(node => condition(node.Data));
        }

        public T[] GetChildrenData()
        {
            return _children.Select(c => c.Data).ToArray();
        }

        public TreeNode<T>[] GetChildren()
        {
            return _children.ToArray();

        }
    }
}
