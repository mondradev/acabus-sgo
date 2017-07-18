using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Structures.Trees
{
    public class Tree<T> : ICollection<Tree<T>>
    {
        private Tree<T>[] _children;
        private int _level;
        private Tree<T> _parent;
        private Tree<T> _root;
        private T _value;

        public Tree(T value)
        {
            _value = value;
            _parent = null;
            _level = 0;
            _root = this;
        }

        public int Count => _children is null ? 0 : _children.Length;
        public bool IsReadOnly => false;
        public int Level => _level;
        public Tree<T> Parent => _parent;
        public Tree<T> Root => _root;
        public T Value => _value;
        public IEnumerable<Tree<T>> Children => _children;

        public IEnumerable<Tree<T>> Descendants => GetNodes(this).Skip(1);

        public void Add(Tree<T> item)
        {
            if (!IsReadOnly)
            {
                if (_children is null)
                    _children = new Tree<T>[1] { item };
                else
                {
                    Array.Resize(ref _children, _children.Length + 1);
                    _children[_children.Length - 1] = item;
                }
                item._parent = this;
                item._root = _root;
                item._level = _level + 1;
                UpdateNode(Root);
            }
        }

        public void Clear()
        {
        }

        public bool Contains(Tree<T> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Tree<T>[] array, int arrayIndex)
        {
        }

        public IEnumerator<Tree<T>> GetEnumerator()
        {
            foreach (var item in GetNodes(this))
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public bool Remove(Tree<T> item)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => String.Format("Value: {0}, Children: {1}, Parent: {2}, Root: {3}, Level: {4}",
                _value,
                Count,
                Parent is null ? default(T) : Parent.Value,
                Root is null ? default(T) : Root.Value,
                Level);

        private void CalculateLevel(Tree<T> tree, int level)
        {
            if (tree.Count > 0)
                foreach (var child in tree._children)
                {
                    child._level = level + 1;
                    CalculateLevel(child, child.Level);
                }
        }

        private IEnumerable<Tree<T>> GetNodes(Tree<T> root)
        {
            yield return root;
            if (root.Count > 0)
                foreach (var item in root._children)
                {
                    foreach (var child in GetNodes(item))
                        yield return child;
                }
        }

        private void UpdateNode(Tree<T> root)
        {
            foreach (var child in root)
            {
                child._root = root.Root;
            }
            CalculateLevel(root, root.Level);
        }
    }
}