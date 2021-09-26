using System;
using System.Collections.Generic;
using System.Linq;

namespace LuaMerge.Core
{
    internal class Resolver
    {
        public IEnumerable<string> Resolve(Node node)
        {
            var resolved = new HashSet<Node>();
            Resolve(node, resolved, new HashSet<Node>());
            return resolved.Select(x => x.Path);
        }

        public void Resolve(Node node, HashSet<Node> resolved, HashSet<Node> seen)
        {
            seen.Add(node);
            foreach (Node item in node.Edges)
            {
                if (!resolved.Contains(item))
                {
                    if (seen.Contains(item))
                    {
                        throw new Exception();
                    }
                    Resolve(item, resolved, seen);
                }
            }

            resolved.Add(node);
        }
    }
}
