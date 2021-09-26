using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LuaMerge.Core
{
    public class Merger
    {
        // to avoid duplicate inserting if multiple files include same file
        private readonly HashSet<string> _alreadyResolved = new();

        // lookup from sourcefile paths to sourcecode
        private readonly Dictionary<string, string> _resolvedFiles = new();

        public string Merge(string input)
        {
            var sourceNodeLookup = GatherDependencies(input).Distinct().ToDictionary(key => key.Path);
            var resolver = new Resolver();
            var resolved = resolver.Resolve(sourceNodeLookup[input]).ToList();
            foreach (string item in resolved)
            {
                Node node = sourceNodeLookup[item];
                var sourceFile = new SourceFile(item, node.Alias);
                string fileData = sourceFile.Code;
                if (sourceNodeLookup.TryGetValue(item, out Node deps))
                {
                    foreach (Node dependency in deps.Edges)
                    {
                        //string includeString = $"{Constants.INCLUDE_STRING} {dependency.Path}";
                        int includePos = fileData.IndexOf(Constants.INCLUDE_STRING);
                        int endPos = fileData.IndexOf(';', includePos);
                        string sourceToInsert;
                        if (_alreadyResolved.Contains(dependency.Path))
                        {
                            sourceToInsert = string.Empty;
                        }
                        else
                        {
                            sourceToInsert = _resolvedFiles[dependency.Path];
                            _alreadyResolved.Add(dependency.Path);
                        }

                        fileData = fileData.Remove(includePos, endPos + 1 - includePos).Insert(includePos, sourceToInsert);
                        //fileData = fileData.Replace(includeString, sourceToInsert);
                    }

                    _resolvedFiles[item] = fileData;
                }
            }

            return _resolvedFiles[input];
        }

        private static IEnumerable<Node> GatherDependencies(string sourceFile)
        {
            static IEnumerable<Node> GatherIncludes(string sourceFile)
            {
                using var reader = new StreamReader(new FileStream(sourceFile, FileMode.Open, FileAccess.Read));
                string line;
                while ((line = reader.ReadLine()?.Trim()) is { })
                {
                    if (line.StartsWith(Constants.INCLUDE_STRING))
                    {
                        //string includeFile = line.Substring(Constants.INCLUDE_STRING.Length).Trim();
                        int pos1 = line.IndexOf('"', Constants.INCLUDE_STRING.Length);
                        int pos2 = line.IndexOf('"', pos1 + 1);
                        string includeFile = line[(pos1 + 1)..pos2];
                        string rest = line[(pos1 + 1)..];
                        int endPos = rest.IndexOf(';');

                        string? alias = null;
                        int aliasPos = rest.IndexOf(Constants.ALIAS);
                        if (aliasPos != -1)
                        {
                            alias = rest[(aliasPos + Constants.ALIAS.Length)..endPos].Trim();
                        }

                        yield return new(includeFile, alias);
                    }
                }
            }

            var stack = new Stack<Node>();
            stack.Push(new Node(sourceFile));

            while (stack.Count > 0)
            {
                Node node = stack.Pop();
                foreach (Node includeNode in GatherIncludes(node.Path))
                {
                    node.AddNode(includeNode);
                    stack.Push(includeNode);
                }

                yield return node;
            }
        }
    }
}
