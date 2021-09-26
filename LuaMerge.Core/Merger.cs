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
            //GatherDeps(input);
            var sourceNodeLookup = GatherDependencies(input).Distinct().ToDictionary(key => key.Name);
            var resolver = new Resolver();
            var resolved = resolver.Resolve(sourceNodeLookup[input]).ToList();
            foreach (string item in resolved)
            {
                var sourceFile = new SourceFile(item);
                string fileData = sourceFile.Code;
                if (sourceNodeLookup.TryGetValue(item, out Node deps))
                {
                    foreach (Node dependency in deps.Edges)
                    {
                        string includeString = $"{Constants.INCLUDE_STRING} {dependency.Name}";

                        string sourceToInsert;
                        if (_alreadyResolved.Contains(dependency.Name))
                        {
                            sourceToInsert = string.Empty;
                        }
                        else
                        {
                            sourceToInsert = _resolvedFiles[dependency.Name];
                            _alreadyResolved.Add(dependency.Name);
                        }

                        fileData = fileData.Replace(includeString, sourceToInsert);
                    }

                    _resolvedFiles[item] = fileData;
                }
            }

            return _resolvedFiles[input];
        }

        private static IEnumerable<Node> GatherDependencies(string sourceFile)
        {
            static IEnumerable<string> GatherIncludes(string sourceFile)
            {
                using var reader = new StreamReader(new FileStream(sourceFile, FileMode.Open, FileAccess.Read));
                string line;
                while ((line = reader.ReadLine()?.Trim()) is { })
                {
                    if (line.StartsWith(Constants.INCLUDE_STRING))
                    {
                        string includeFile = line.Substring(Constants.INCLUDE_STRING.Length).Trim();

                        yield return includeFile;
                    }
                }
            }

            var stack = new Stack<Node>();
            stack.Push(new Node(sourceFile));

            while (stack.Count > 0)
            {
                Node node = stack.Pop();
                foreach (string includeFile in GatherIncludes(node.Name))
                {
                    var includeNode = new Node(includeFile);
                    node.AddNode(includeNode);
                    stack.Push(includeNode);
                }

                yield return node;
            }
        }
    }
}
