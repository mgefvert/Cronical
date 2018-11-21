using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cronical.Misc
{
    /// <summary>
    /// Class that operates on a string and extract values from it
    /// </summary>
    public class StringParser
    {
        private string _source;
        private char[] _commentChars;
        private char[] _whitespace;

        /// <summary>Whether escape characters (\n, \r etc) are parsed in input strings</summary>
        public bool ParseEscape { get; set; }

        /// <summary>Whether strings surrounded in quotes ("") are handled</summary>
        public bool ParseQuotes { get; set; }

        /// <summary>Whether to leave quotes ("") in the extracted strings</summary>
        public bool LeaveQuotes { get; set; }

        /// <summary>List of characters used for comments (e.g. # or ; )</summary>
        public char[] CommentChars
        {
            get => _commentChars;
            set => _commentChars = value ?? new char[0];
        }

        /// <summary>Whitespace characters to be used for separating words</summary>
        public char[] Whitespace
        {
            get => _whitespace;
            set => _whitespace = value ?? new char[0];
        }

        /// <summary>Source string to operate on</summary>
        public string Source
        {
            get => _source;
            set => _source = value ?? "";
        }

        public StringParser(string source = null)
        {
            _commentChars = new char[0];
            _whitespace = new[] { ' ', '\t', '\r', '\n' };
            _source = source ?? "";
        }

        /// <summary>
        /// Static method for extracting a word from a string
        /// </summary>
        /// <param name="source">Source string to operate on; it will be modified by ExtractWord</param>
        /// <returns>The next word in the string</returns>
        public static string ExtractWord(ref string source)
        {
            var sp = new StringParser(source);
            var word = sp.ExtractWord();
            source = sp.Source;
            return word;
        }

        /// <summary>
        /// Extract the next word from Source, which will be modified by the operation
        /// </summary>
        /// <returns>The next word in the source string</returns>
        public string ExtractWord()
        {
            _source = (_source ?? "").Trim(Whitespace);
            if (string.IsNullOrEmpty(_source))
                return "";

            if (CommentChars.Contains(_source.First()))
            {
                _source = "";
                return "";
            }

            var word = Tokenize(ref _source, Whitespace, CommentChars, ParseEscape, ParseQuotes);
            _source = (_source ?? "").Trim(Whitespace);

            return ParseQuotes && !LeaveQuotes && word.Length >= 2 && word.First() == '"' && word.Last() == '"'
              ? word.Substring(1, word.Length - 2)
              : word;
        }

        /// <summary>
        /// Extract all words from the source string.
        /// </summary>
        /// <returns>A list of words found in the string</returns>
        public List<string> ExtractWords()
        {
            var result = new List<string>();

            while (!string.IsNullOrEmpty(_source))
            {
                var word = ExtractWord();
                if (!string.IsNullOrEmpty(word))
                    result.Add(word);
            }

            return result;
        }

        public static string Tokenize(ref string source, char[] separators, char[] commentchars, bool escape, bool quote)
        {
            if (source == null)
                return null;

            var result = new StringBuilder();
            var max = source.Length - 1;
            var inQuote = false;

            int i;
            for (i = 0; i <= max; i++)
            {
                var last = i == max;
                var c = source[i];

                if (c == '"' && quote)
                {
                    if (!inQuote)
                        inQuote = true;                                 // Not currently in quote, found quote, start handling quoted strings
                    else
                    {                                                 // Currently in quote
                        if (!last && source[i + 1] == '"')
                            i++;                                          // Double-quote, skip one
                        else
                            inQuote = false;                              // Just a normal quote, turn off quote processing
                    }
                }
                else if (c == '\\' && !last && escape)
                {
                    c = source[++i];                                  // Found escape character, step forward one
                    if (c == 'x')                                     // Found hex sequence
                    {
                        if (i > max - 2)                                // Not enough room, exit
                            break;

                        var hex = source[i + 1].ToString() + source[i + 2]; // Decode hex
                        c = (char)Convert.ToByte(hex, 16);
                        i += 2;
                    }
                    else if (c == '0')
                        c = '\0';                                       // Found NULL byte
                    else if (c == 'n')
                        c = '\n';                                       // Found New Line
                    else if (c == 'r')
                        c = '\r';                                       // Found Carriage Return
                    else if (c == 't')
                        c = '\t';                                       // Found Tab
                }
                else if (!inQuote && separators.Contains(c))
                {
                    source = source.SafeSubstring(i + 1);             // Found separator outside of quote, cut and return
                    return result.ToString();
                }
                else if (!inQuote && commentchars.Contains(c))
                    break;                                            // Found comment, break

                // Normal character, copy
                result.Append(c);
            }

            // This is the end... beautiful friend, the end
            source = null;

            return result.ToString();
        }
    }
}
