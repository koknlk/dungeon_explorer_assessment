using DungeonExplorerBackend.Contracts;

namespace DungeonExplorerBackend.Services
    {
    public class InputSanitizer : IInputSanitizer
        {
        public string Sanitize(string input)
            {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(input, @"[<>""'&/()]", string.Empty).Trim();
            }
        }
    }