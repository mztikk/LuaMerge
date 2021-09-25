using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LuaMerge.Core
{
    public class Merger
    {
        private readonly ConcurrentDictionary<string, Node> _s = new();
        private readonly HashSet<string> _r = new();
        private readonly Dictionary<string, string> _resolvedFiles = new();

        public string Merge(string input)
        {
            GatherDeps(input);
            var r = new Resolver();
            var resolved = r.Resolve(_s[input]).ToList();
            foreach (string item in resolved)
            {
                var sourceFile = new SourceFile(item);
                string fileData = sourceFile.Code;
                if (_s.TryGetValue(item, out Node deps))
                {
                    foreach (Node d in deps.Edges)
                    {
                        string findstr = $"{Constants.INCLUDE_STRING} {d.Name}";
                        string replaceWith;
                        if (_r.Contains(d.Name))
                        {
                            replaceWith = string.Empty;
                        }
                        else
                        {
                            replaceWith = _resolvedFiles[d.Name];
                            _r.Add(d.Name);
                        }
                        fileData = fileData.Replace(findstr, replaceWith);
                    }

                    _resolvedFiles[item] = fileData;
                }
            }

            return _resolvedFiles[input];
        }

        private void GatherDeps(string input)
        {
            Node n = _s.GetOrAdd(input, new Node(input));

            using var reader = new StreamReader(new FileStream(input, FileMode.Open, FileAccess.Read));
            string line;
            var deps = new HashSet<string>();
            while ((line = reader.ReadLine()?.Trim()) is { })
            {
                if (line.StartsWith(Constants.INCLUDE_STRING))
                {
                    string includeFile = line.Substring(Constants.INCLUDE_STRING.Length).Trim();
                    n.AddNode(_s.GetOrAdd(includeFile, new Node(includeFile)));
                    deps.Add(includeFile);
                }
            }

            foreach (string dep in deps)
            {
                //if (_r.Contains(dep))
                //{
                //    continue;
                //}

                //_r.Add(dep);
                GatherDeps(dep);
            }
        }
    }
}
