using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Group of functions for handling screen formatting
    /// </summary>
    public static class Formatting {

        /// <summary>
        /// Turn an abstract coordinate into a real coordinate.
        /// This just means that we use positive values to represent offsets from left/top,
        /// and negative values to represent offsets from right/bottom.
        /// </summary>
        /// <param name="val">Coordinate value to convert</param>
        /// <param name="max">Maximum value for the coordinate, used to translate negative values</param>
        /// <returns>
        /// Position represented
        /// </returns>
        public static int ConvertCoord(int val, int max)
        {
            if (val >= 0) {
                return val;
            } else {
                return max + val - 1;
            }
        }

        /// <summary>
        /// Calculate the longest line length in a string when split on newlines
        /// </summary>
        /// <param name="msg">String to analyze</param>
        /// <returns>
        /// Length of longest line
        /// </returns>
        public static int MaxLineLength(string msg)
        {
            int len = 0;
            string[] hardLines = msg.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);
            foreach (string line in hardLines) {
                if (len < line.Length) {
                    len = line.Length;
                }
            }
            return len;
        }

        /// <summary>
        /// Word wrap a long string into separate lines
        /// </summary>
        /// <param name="msg">Long message to wrap</param>
        /// <param name="w">Allowed length of lines</param>
        /// <returns>
        /// List of strings, one per line
        /// </returns>
        public static List<string> WordWrap(string msg, int w)
        {
            List<string> messageLines = new List<string>();
            if (!string.IsNullOrEmpty(msg)) {
                // The string is allowed to contain line breaks.
                string[] hardLines = msg.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);
                foreach (string line in hardLines) {
                    if (string.IsNullOrEmpty(line)) {
                        messageLines.Add("");
                    } else if (line.Length <= w) {
                        messageLines.Add(line);
                    } else {
                        int used = 0;
                        while (used < line.Length) {
                            while (used < line.Length && line[used] == ' ') {
                                // Skip spaces so lines start with non-spaces
                                ++used;
                            }
                            if (used >= line.Length) {
                                // Ran off the end of the string with spaces, we're done
                                messageLines.Add("");
                                break;
                            }
                            int lineLen;
                            if (used + w >= line.Length) {
                                // We're at the end of the line, use the whole thing
                                lineLen = line.Length - used;
                            } else {
                                // Middle of the line, find a word wrappable chunk
                                for (lineLen = w; lineLen >= 0 && line[used + lineLen] != ' '; --lineLen) { }
                            }
                            if (lineLen < 1) {
                                // Word too long, truncate it
                                lineLen = w;
                            }
                            messageLines.Add(line.Substring(used, lineLen));
                            used += lineLen;
                        }
                    }
                }
            }
            return messageLines;
        }

    }

}
