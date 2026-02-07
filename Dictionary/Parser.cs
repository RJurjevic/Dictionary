using System;

namespace Webster
{
    /// <summary>
    /// Parses the Gutenberg Webster dictionary text file via <see cref="Reader"/>.
    ///
    /// This parser supports two usage patterns via <see cref="FindWord"/>:
    /// 1) Lookup mode: FindWord("HOME") returns the entry for the specified headword line.
    /// 2) Sequential mode: FindWord() returns the "current" word, and internally sets the next headword
    ///    so that repeated calls walk the dictionary from A .. ZYTHUM.
    ///
    /// Important implementation detail: this parser always rewinds and scans from the start of the file
    /// to find the current headword. This is correct but slow (O(n) per lookup / per step).
    /// We'll keep it as-is for now to preserve behavior.
    /// </summary>
    class Parser
    {
        private readonly Reader _reader;

        // Current target headword line to search for (must match the full line exactly).
        private string _word;

        public Parser()
        {
            _reader = new Reader();
        }

        /// <summary>
        /// Heuristic for "this line is a dictionary headword line".
        ///
        /// Current rule (kept exactly):
        /// - must be non-empty
        /// - must NOT contain: lowercase letters, '.', digits, (), [], '+', ','
        /// - must NOT be "--" and must NOT start with "--"
        ///
        /// If true, also returns <paramref name="words"/> as the line split by ';' and trimmed.
        /// </summary>
        private static bool IsWordLine(string line, out string[] words)
        {
            words = null;

            if (line == null || line.Length == 0)
                return false;

            // Disqualify lines that look like non-headword content.
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (char.IsLower(c) ||
                    c == '.' ||
                    char.IsNumber(c) ||
                    c == '(' || c == ')' ||
                    c == '[' || c == ']' ||
                    c == '+' ||
                    c == ',')
                {
                    return false;
                }
            }

            // Disqualify "--" lines and lines starting with "--".
            if (line == "--")
                return false;

            if (line.Length > 1 && line[0] == '-' && line[1] == '-')
                return false;

            // For headword lines, split by ';' (aliases) and trim.
            words = Array.ConvertAll(line.Split(';'), p => p.Trim());
            return true;
        }

        /// <summary>
        /// Determines whether the current candidate headword line matches our target.
        ///
        /// Behavior preserved: exact line match only.
        /// (There is a commented-out alternative in the original code that matched any alias token.)
        /// </summary>
        private bool IsMatch(string line, string wordArg, string[] words)
        {
            // Original (disabled) behavior:
            // return ((line == _word) || (wordArg != null && words.Any(s => s == _word)));

            return (line == _word);
        }

        /// <summary>
        /// Find an entry.
        ///
        /// If <paramref name="word"/> is provided:
        /// - sets the target headword line and returns its entry (lookup mode)
        ///
        /// If <paramref name="word"/> is null:
        /// - uses the current internal headword target (_word)
        /// - if _word is null, starts at "A"
        /// - returns the entry for _word, and sets _word to the next headword line (sequential mode)
        /// </summary>
        public Word FindWord(string word = null)
        {
            Word found = null;

            // In lookup mode, update the target headword line.
            if (word != null)
                _word = word;

            // In sequential mode, default starting point.
            if (_word == null)
                _word = "A";

            // Keep structure close to original: rewind and scan for the target headword line.
            _reader.Rewind();

            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                // Stop when we hit the Gutenberg footer marker.
                // Note: (w != null...) part is effectively unreachable here in practice,
                // but kept to preserve original logic.
                if (line.Contains("End of Project Gutenberg's Webster's Unabridged Dictionary, by Various") ||
                    (found != null && found.GetWord() == "ZYTHUM" && line.Contains("wheat. [Written also zythem.]")))
                {
                    break;
                }

                // Detect headword line.
                string[] headwordTokens;
                if (IsWordLine(line, out headwordTokens) && IsMatch(line, word, headwordTokens))
                {
                    // We found the headword line for the requested/current target.
                    string verbum = line;          // raw headword line
                    string[] verbis = headwordTokens; // tokens split by ';'

                    found = new Word(verbum, verbis);

                    // Build one description block at a time. Repeated headword lines create multiple blocks.
                    string description = line;

                    while ((line = _reader.ReadLine()) != null)
                    {
                        // Original formatting: always insert a newline before appending the next line.
                        description += Environment.NewLine;

                        string[] maybeTokens;

                        // If we see the SAME headword line again, close the current block and start a new one.
                        if (IsWordLine(line, out maybeTokens) && IsMatch(line, word, maybeTokens))
                        {
                            found.AddDescription(description);
                            description = "";
                        }

                        // If we see a DIFFERENT headword line, close the current block,
                        // set _word to that next headword (for sequential mode), and stop.
                        if (IsWordLine(line, out maybeTokens) && !IsMatch(line, word, maybeTokens))
                        {
                            found.AddDescription(description);
                            description = "";
                            _word = line; // next headword line
                            break;
                        }

                        // Normal content line: append to description.
                        description += line;

                        // Special-case end-of-file tail for ZYTHUM.
                        if (found.GetVerbum() == "ZYTHUM" && line.Contains("wheat. [Written also zythem.]"))
                        {
                            if (_word == "ZYTHUM")
                                found.AddDescription(description + Environment.NewLine + Environment.NewLine);
                            break;
                        }
                    }

                    break; // stop scanning after we parsed the target word
                }
            }

            return found;
        }
    }
}
