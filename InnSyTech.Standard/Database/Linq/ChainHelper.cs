using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace InnSyTech.Standard.Database.Linq
{
    internal sealed class ChainHelper : IEnumerable, IEnumerable<ChainNode>
    {
        private List<ChainNode> _nodes = new List<ChainNode>();

        public int Count => _nodes.Count;

        public ChainNode this[int index] => _nodes[index];

        public void Add(MemberInfo member)
                            => _nodes.Insert(0, new ChainNode(member));

        public void Add(IEnumerable<ChainNode> chain)
            => _nodes.AddRange(chain);

        public void AddLast(MemberInfo member)
                    => _nodes.Add(new ChainNode(member));

        IEnumerator IEnumerable.GetEnumerator()
        => _nodes.GetEnumerator();

        public IEnumerator<ChainNode> GetEnumerator()
            => _nodes.GetEnumerator();

        public ChainHelper GetEqualsNodes(ChainHelper chain)
        {
            List<ChainNode> members = new List<ChainNode>();

            for (int i = 0; i < Count && i < chain.Count; i++)
            {
                if (this[i].Member == chain[i].Member)
                {
                    members.Add(this[i]);
                    chain._nodes[i] = this[i];
                }
                else
                    break;
            }
            return new ChainHelper() { _nodes = members };
        }
    }

    internal sealed class ChainNode
    {
        private MemberInfo _member;

        public ChainNode(MemberInfo member)
            => _member = member;

        public String Alias { get; set; }

        public MemberInfo Member => _member;

        public static bool operator !=(ChainNode node, ChainNode anotherNode)
        {
            if (node is null && anotherNode is null)
                return false;

            if (node is null || anotherNode is null)
                return true;

            return node.Alias != anotherNode.Alias || node.Member != anotherNode.Member;
        }

        public static bool operator ==(ChainNode node, ChainNode anotherNode)
        {
            if (node is null && anotherNode is null)
                return true;

            if (node is null || anotherNode is null)
                return false;

            return node.Alias == anotherNode.Alias && node.Member == anotherNode.Member;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;

            if (!(obj is ChainNode))
                return false;

            return this == (obj as ChainNode);
        }

        public override int GetHashCode()
            => base.GetHashCode();
    }
}