using System;
using System.IO;
using System.Text;
using RFReborn.Hashing;

namespace LuaMerge.Core
{
    internal class SourceFile
    {
        private readonly string _rawFile;
        private readonly string _path;

        public SourceFile(string path)
        {
            _path = path;
            _rawFile = File.ReadAllText(path);
            _convertedFile = new(ConvertToFunctionCall);
        }

        private readonly Lazy<string> _convertedFile;

        public string Code => _convertedFile.Value;

        private string ConvertToFunctionCall() => ConvertToFunctionCall(ConvertFilePathToFunctionName(_path), _rawFile);

        private static string ConvertFilePathToFunctionName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return string.Create(fileName.Length, fileName, (span, value) =>
            {
                value.AsSpan().CopyTo(span);
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (char.IsLetterOrDigit(c) && char.IsAscii(c))
                    {
                        continue;
                    }

                    span[i] = '_';
                }
            });
        }

        private static string ConvertToFunctionCall(string name, string rawText)
        {
            string hash = HashFactory.Hash("SHA256", rawText);
            var builder = new StringBuilder();
            builder
                .Append("local function ")
                .Append(name)
                .Append('_')
                .Append(hash)
                .AppendLine("()")
                .AppendLine(rawText)
                .AppendLine("end")
                .Append(name)
                .Append('_')
                .Append(hash)
                .AppendLine("()");

            return builder.ToString();
        }
    }
}
