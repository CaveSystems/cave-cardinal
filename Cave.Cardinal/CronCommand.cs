using System;
using System.Collections.Generic;

namespace Cave.Cron
{
    /// <summary>
    /// Provides a CronCommand.
    /// </summary>
    public struct CronCommand
    {
        /// <summary>
        /// Parses a cron command.
        /// </summary>
        /// <param name="text">Command and parameter string.</param>
        /// <returns>Returns a new <see cref="CronCommand"/> instance.</returns>
        public static CronCommand Parse(string text)
        {
            var result = default(CronCommand);
            var i = 0;
            var depth = new Stack<char>();
            for (; i < text.Length; i++)
            {
                var c = text[i];
                switch (c)
                {
                    case ' ':
                        if (depth.Count == 0)
                        {
                            result.Command = text.UnboxText(false).Substring(0, i);
                            result.Parameters = text.UnboxText(false).Substring(i + 1);
                            return result;
                        }
                        break;
                    case '\'':
                    case '"':
                        var push = true;
                        if (depth.Count > 0)
                        {
                            if (depth.Peek() == c)
                            {
                                depth.Pop();
                                push = false;
                            }
                        }
                        if (push)
                        {
                            depth.Push(c);
                        }
                        break;
                }
            }
            if (depth.Count > 0)
            {
                throw new FormatException(string.Format("Error parsing CronCommand '{0}'!", text));
            }

            result.Command = text.UnboxText();
            result.Parameters = string.Empty;
            return result;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public string Command;

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public string Parameters;

        /// <summary>
        /// Gets the cron config string.
        /// </summary>
        /// <returns>A new config string.</returns>
        public override string ToString() => $"'{Command}' {Parameters}";
    }
}
