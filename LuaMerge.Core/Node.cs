using System;
using System.Collections.Generic;

namespace LuaMerge.Core
{
    internal class Node : IEquatable<Node>, IEqualityComparer<Node>
    {
        public readonly string Path;
        public readonly string? Alias;
        public readonly HashSet<Node> Edges;

        public Node(string path, string? alias = null)
        {
            Path = path;
            Alias = alias;
            Edges = new HashSet<Node>();
        }

        public void AddNode(Node node) => Edges.Add(node);
        public bool Equals(Node x, Node y) => x.Path.Equals(y.Path);
        public bool Equals(Node other) => Equals(this, other);
        public int GetHashCode(Node obj) => HashCode.Combine(Path);

        public override bool Equals(object obj) => Equals(obj as Node);

        public override int GetHashCode() => Path.GetHashCode();
    }
}
