using System;
using System.Collections.Generic;

namespace LuaMerge.Core
{
    internal class Node : IEquatable<Node>, IEqualityComparer<Node>
    {
        public readonly string Name;
        public readonly HashSet<Node> Edges;

        public Node(string name)
        {
            Name = name;
            Edges = new HashSet<Node>();
        }

        public void AddNode(Node node) => Edges.Add(node);
        public bool Equals(Node x, Node y) => x.Name.Equals(y.Name);
        public bool Equals(Node other) => Equals(this, other);
        public int GetHashCode(Node obj) => HashCode.Combine(Name);

        public override bool Equals(object obj) => Equals(obj as Node);

        public override int GetHashCode() => Name.GetHashCode();
    }
}
