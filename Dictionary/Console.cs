using System;
using System.Linq;
using System.Text;

namespace Webster
{
    internal static class ConsoleWriter
    {
        private static bool _initialized;

        // Tweak these anytime (or set them once in Main).
        public static ConsoleColor HeadwordColor { get; set; } = ConsoleColor.White;
        public static ConsoleColor DescriptionColor { get; set; } = ConsoleColor.Green;

        public static void InitializeIso88591()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                // On .NET Framework this is usually enough.
                var enc = Encoding.GetEncoding("ISO-8859-1");
                Console.OutputEncoding = enc;
                Console.InputEncoding = enc;
            }
            catch
            {
                // If it fails, we just keep console defaults (don’t crash).
            }
        }

        public static void WriteEntryWithHeadwordHighlight(string entryText)
        {
            if (string.IsNullOrEmpty(entryText))
                return;

            // Normalize line endings, then write line-by-line with colors.
            entryText = entryText.Replace("\r\n", "\n").Replace("\r", "\n");

            var oldColor = Console.ForegroundColor;

            try
            {
                var lines = entryText.Split('\n');
                foreach (var raw in lines)
                {
                    var line = raw ?? string.Empty;

                    if (IsHeadwordLine(line))
                    {
                        Console.ForegroundColor = HeadwordColor;
                        Console.WriteLine(line);
                    }
                    else
                    {
                        Console.ForegroundColor = DescriptionColor;
                        Console.WriteLine(line);
                    }
                }
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

        // “Headword line” = has letters, has NO lowercase letters.
        // Works for: HOME / HONOR / COOPERATE / etc.
        private static bool IsHeadwordLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            bool hasLetter = false;

            foreach (char c in line)
            {
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                    if (char.IsLower(c))
                        return false;
                }
            }

            return hasLetter;
        }
    }
}
