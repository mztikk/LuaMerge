using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LuaMerge.Core;

namespace LuaMerge
{
    internal class Program
    {
        public const string LuaMergeConfig = ".luamconfig";

        private static async Task Main(string[] args)
        {
            string currentDir = Environment.CurrentDirectory;
            var stream = new FileStream(LuaMergeConfig, FileMode.Open, FileAccess.Read);
            LuaMergeOptions? cfg = await JsonSerializer.DeserializeAsync<LuaMergeOptions>(stream);
            if (cfg is null)
            {
                throw new Exception();
            }

            if (!Directory.Exists(cfg.OutDir))
            {
                Directory.CreateDirectory(cfg.OutDir);
            }

            foreach (string sourceFile in cfg.SourceFiles)
            {
                var file = new FileInfo(sourceFile);
                string newPath = Path.Join(cfg.OutDir, file.Name);
                var m = new Merger();
                File.WriteAllText(newPath, m.Merge(sourceFile));
            }
        }
    }
}
