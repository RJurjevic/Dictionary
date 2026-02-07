using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Webster
{
    /// <summary>
    /// Represents a single dictionary entry (a headword line + one or more description blocks).
    ///
    /// Notes on naming (kept to match your original style):
    /// - Verbum: the raw headword line as read from the file (e.g., "HOME" or "FOO; BAR").
    /// - Sermo: a display/anchor form created by joining the split headword tokens with "; ".
    /// </summary>
    class Word
    {
        // Raw headword line from the dictionary file (e.g. "HOME").
        private readonly string _word;

        // Joined headword tokens used for display/HTML id (e.g. "FOO; BAR").
        private readonly string _sermo;

        // One or more text blocks making up the entry. Each block may contain embedded newlines.
        private readonly List<string> _descriptions;

        // Headword tokens (usually split by ';' and trimmed).
        private readonly string[] _words;

        public Word(string word, string[] words)
        {
            _word = word;
            _words = words;

            _descriptions = new List<string>();

            // Original code constructed this via a loop with String.Format and "; " between tokens.
            // string.Join produces the same output for non-null arrays.
            _sermo = string.Join("; ", words);
        }

        /// <summary>Add a full description block (already formatted with newlines where needed).</summary>
        public void AddDescription(string description)
        {
            _descriptions.Add(description);
        }

        /// <summary>Returns the raw headword line as read from the dictionary.</summary>
        public string GetVerbum()
        {
            return _word;
        }

        /// <summary>Returns the "joined" headword tokens (used for display and HTML anchors).</summary>
        public string GetWord()
        {
            return _sermo;
        }

        /// <summary>Returns the list of description blocks (original list instance).</summary>
        public List<string> GetDescriptions()
        {
            return _descriptions;
        }

        /// <summary>
        /// Returns the full entry text by concatenating all description blocks.
        /// (Behavior preserved: trailing newline trimming in the original was a no-op, so we do not trim.)
        /// </summary>
        public string GetDescription()
        {
            // More efficient than repeated string concatenation; output is identical.
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _descriptions.Count; i++)
            {
                sb.Append(_descriptions[i]);
            }

            // Original code attempted TrimEnd but did not assign the result; preserve that behavior.
            // sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());

            return sb.ToString();
        }

        /// <summary>
        /// Returns an HTML rendering of the entry.
        /// - Inserts an anchor: &lt;a id="SERMO"&gt;&lt;/a&gt;
        /// - For each description block: bolds the first line, then emits each line with "&lt;br&gt;".
        ///
        /// (Behavior preserved: no HTML escaping; TrimEnd in original was also a no-op.)
        /// </summary>
        public string GetDescriptionHTML()
        {
            StringBuilder sb = new StringBuilder();

            // Anchor for jump-links.
            sb.Append("<a id=\"").Append(_sermo).Append("\"></a>").Append(Environment.NewLine);

            for (int d = 0; d < _descriptions.Count; d++)
            {
                string desc = _descriptions[d];

                using (StringReader reader = new StringReader(desc))
                {
                    string line;
                    int lineIndex = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineIndex++;

                        if (lineIndex == 1)
                        {
                            sb.Append("<b>").Append(line).Append("</b><br>").Append(Environment.NewLine);
                        }
                        else
                        {
                            sb.Append(line).Append("<br>").Append(Environment.NewLine);
                        }
                    }
                }
            }

            // Original code attempted TrimEnd but did not assign the result; preserve that behavior.
            // sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());

            return sb.ToString();
        }

        /// <summary>Returns the headword tokens array (original array instance).</summary>
        public string[] GetWords()
        {
            return _words;
        }

        /// <summary>Returns the number of headword tokens (split by ';').</summary>
        public int GetCount()
        {
            return _words.Length;
        }
    }
}
