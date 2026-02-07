using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Webster
{
    class Verbis
    {
        // Gutenberg file is ISO-8859-1 (Latin-1). Keep this explicit everywhere.
        private static readonly Encoding WebsterEncoding = Encoding.GetEncoding("ISO-8859-1");

        /// <summary>
        /// Sequentially parses the entire dictionary (via Parser.FindWord() with no args),
        /// performing basic integrity checks and optionally writing all headwords to Webster.txt.
        ///
        /// This mode is triggered by /c.
        /// </summary>
        private static void Webster(bool write = true)
        {
            // Used only to detect duplicate headword lines.
            // (Original code stored Word objects; a HashSet is sufficient for the same behavior.)
            HashSet<string> seenHeadwords = new HashSet<string>();

            Parser parser = new Parser();
            Word word = null;

            // Left as-is: original code tracked max alias count but never used it.
            int max = 0;

            bool ok = true;

            StreamWriter file = null;
            if (write)
            {
                // Writes a plain list of headwords encountered during parsing.
                FileStream filestream = new FileStream(@"Webster.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                file = new StreamWriter(filestream, WebsterEncoding);
            }

            Console.Write(Environment.NewLine + "checking dictionary...");

            while ((word = parser.FindWord()) != null)
            {
                if (file != null)
                {
                    file.Write("{0}", Environment.NewLine + word.GetVerbum());
                }

                if (word.GetCount() > max)
                {
                    max = word.GetCount();
                }

                // Integrity check: the stored "verbum" (raw headword line) must equal the joined alias string.
                if (word.GetVerbum() != word.GetWord())
                {
                    Console.Write("{0} not equal to {1}", Environment.NewLine + word.GetVerbum(), word.GetWord());
                    ok = false;
                    break;
                }

                // Duplicate headword detection.
                if (seenHeadwords.Contains(word.GetVerbum()))
                {
                    // Original message said "not in sequence" but this really means duplicate.
                    Console.Write("{0} not in sequence", Environment.NewLine + word.GetVerbum());
                    ok = false;
                    break;
                }

                seenHeadwords.Add(word.GetVerbum());

                // Original behavior: stop when ZYTHUM is reached.
                if (word.GetVerbum() == "ZYTHUM")
                    break;
            }

            if (ok)
                Console.Write("{0} words found", Environment.NewLine + seenHeadwords.Count);

            Console.Write("{0} {1}", Environment.NewLine + "dictionary", (ok ? "OK" : "not OK"));

            if (file != null)
                file.Close();
        }

        static void Main(string[] args)
        {
            ConsoleWriter.InitializeIso88591();
            ConsoleWriter.DescriptionColor = ConsoleColor.Green;

            if (args == null || args.Length == 0)
                return;

            List<string> words = new List<string>();

            bool html = false; // /w
            bool readFromFile = false; // /f
            int htmlWordCount = 0;

            // Switches (kept exactly the same as original):
            // /c => run dictionary check
            // /w => write HTML output (words.html)
            // /f => read lookup words from words.txt
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "/c") { Webster(); }
                else if (args[i] == "/w") { html = true; }
                else if (args[i] == "/f") { readFromFile = true; }
            }

            // Build word list.
            if (!readFromFile)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string w = args[i];
                    if (!w.StartsWith("/"))
                        words.Add(w);
                }
            }
            else
            {
                // Original behavior: when /f is present, ignore non-switch args and read words.txt.
                using (FileStream filestreamin = new FileStream(@"words.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader filein = new StreamReader(filestreamin, WebsterEncoding, false, 1024))
                {
                    string w;
                    while ((w = filein.ReadLine()) != null)
                    {
                        words.Add(w);
                    }
                }
            }

            Parser parser = new Parser();

            StreamWriter htmlFile = null;

            if (html)
            {
                Console.Write(Environment.NewLine + "writing header...");

                string title = "Webster's Unabridged Dictionary";
                string description =
                    "HTML version of Gutenberg's Webster's Unabridged Dictionary 29765-8.txt with character set encoding ISO-8859-1";
                string details =
                    "Title: Gutenberg's Webster's Unabridged Dictionary; Author: Various; Release date: August 22, 2009 [EBook #29765]; Last update date: December 24, 2018; Language: English; Character set encoding: ISO-8859-1; Produced by Graham Lawrence; HTML by Robert Jurjevic";

                FileStream filestream = new FileStream(@"words.html", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                htmlFile = new StreamWriter(filestream, WebsterEncoding);

                // HTML header (kept identical in spirit/content).
                htmlFile.Write("<!DOCTYPE html>" + Environment.NewLine);
                htmlFile.Write("<html>" + Environment.NewLine);
                htmlFile.Write("<head>" + Environment.NewLine);
                htmlFile.Write("<meta charset=\"ISO-8859-1\">" + Environment.NewLine);
                htmlFile.Write("<title>Webster's Unabridged Dictionary</title>" + Environment.NewLine);
                htmlFile.Write("<style>" + Environment.NewLine);
                htmlFile.Write("a:link {color: green; background-color: transparent; text-decoration: none;}" + Environment.NewLine);
                htmlFile.Write("a:visited {color: pink; background-color: transparent; text-decoration: none;}" + Environment.NewLine);
                htmlFile.Write("a:hover {color: red; background-color: transparent; text-decoration: underline;}" + Environment.NewLine);
                htmlFile.Write("a:active {color: yellow; background-color: transparent; text-decoration: underline;}" + Environment.NewLine);
                htmlFile.Write("</style>" + Environment.NewLine);
                htmlFile.Write("</head>" + Environment.NewLine);
                htmlFile.Write("<body>");
                htmlFile.Write("<h2>" + title + "</h2>");
                htmlFile.Write("<a href=\" \" title=\"" + details +
                               "\" style=\"background-color:#FFFFFF;color:#000000;text-decoration:none\">" +
                               description + "</a>" + "<br>" + Environment.NewLine + "<br><br>");

                // Build jump-links section.
                // Note: this may be slow because it re-scans the dictionary per word (same as original).
                StringBuilder header = new StringBuilder();
                for (int i = 0; i < words.Count; i++)
                {
                    string w = words[i];
                    Word found = parser.FindWord(w.ToUpper());
                    if (found != null)
                    {
                        header.Append("<a href=\"#")
                              .Append(found.GetWord())
                              .Append("\">")
                              .Append(found.GetWord())
                              .Append("</a><br>")
                              .Append(Environment.NewLine);
                    }
                }

                htmlFile.Write(header.ToString());
                htmlFile.Write("<br>");
            }

            if (html)
                Console.Write(Environment.NewLine + "writing words...");

            // Write each found entry.
            for (int i = 0; i < words.Count; i++)
            {
                string w = words[i];
                Word found = parser.FindWord(w.ToUpper());
                if (found != null)
                {
                    if (html)
                    {
                        htmlFile.Write("{0}", found.GetDescriptionHTML());
                        htmlWordCount++;
                    }
                    else
                    {
                        ConsoleWriter.WriteEntryWithHeadwordHighlight(found.GetDescription());
                    }
                }
            }

            if (html)
            {
                Console.Write(Environment.NewLine + "finishing...");

                htmlFile.Write("</body>" + Environment.NewLine);
                htmlFile.Write("</html>");
                htmlFile.Close();

                Console.Write("{0} html words added", Environment.NewLine + htmlWordCount);
            }
        }
    }
}
