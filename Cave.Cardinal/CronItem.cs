using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cave.Collections;

namespace Cave.Cron
{
    /// <summary>
    /// Provides a wrapper for a full cron configuration line containing start date/time and command.
    /// </summary>
    public class CronItem
    {
        /// <summary>
        /// Parses a <see cref="CronItem"/> string.<br />
        /// Format1:<br />
        /// <para>
        /// .---------------- minute (0 - 59)<br />
        /// |   .------------- hour (0 - 23)<br />
        /// |   |   .---------- day of month (1 - 31)<br />
        /// |   |   |   .------- month (1 - 12) OR jan,feb,mar,apr ... <br />
        /// |   |   |   |  .----- day of week (0 - 6) (Sunday=0 or 7)  OR sun,mon,tue,wed,thu,fri,sat <br />
        /// |   |   |   |  |<br />
        /// *   *   *   *  *  command to be executed
        /// .
        /// </para>
        /// Format2:<br />
        /// <para>
        /// @yearly (or @annually): Run once a year  - equals: 0 0 1 1 *<br />
        /// @monthly:               Run once a month - equals: 0 0 1 * *<br />
        /// @weekly:                Run once a week  - equals: 0 0 * * 0<br />
        /// @daily (or @midnight):  Run once a day   - equals: 0 0 * * *<br />
        /// @hourly:                Run once an hour - equals: 0 * * * *<br />
        /// .
        /// </para>
        /// </summary>
        /// <param name="parts">Parts to parse.</param>
        /// <returns>Returns a new <see cref="CronItem"/> instance.</returns>
        public static CronItem Parse(string[] parts)
        {
            if (parts.Length > 1)
            {
                string firstItem = parts[0].ToLower();
                if (firstItem.StartsWith("@"))
                {
                    switch (firstItem)
                    {
                        case "@yearly":
                        case "@annually":
                            return Parse($"0 0 1 1 * {parts.Skip(1).Join(" ")}");

                        case "@monthly":
                            return Parse($"0 0 1 * * {parts.Skip(1).Join(" ")}");

                        case "@weekly":
                            return Parse($"0 0 * * 0 {parts.Skip(1).Join(" ")}");

                        case "@daily":
                        case "@midnight":
                            return Parse($"0 0 * * * {parts.Skip(1).Join(" ")}");

                        case "@hourly":
                            return Parse($"0 * * * * {parts.Skip(1).Join(" ")}");

                        case "@reboot":
                            throw new NotSupportedException(string.Format("RunOnce / @reboot is not supported!"));

                        default:
                            throw new FormatException(string.Format("Unknown cron repetition '{0}'!", firstItem));
                    }
                }
            }
            if (parts.Length < 5)
            {
                throw new Exception(string.Format("Invalid number of parts at cron entry! Expected '@repetition' or 'minute hour day month weekday', got '{0}'!", parts.Join(" ")));
            }

            CronItem result = new CronItem();
            if (parts.Length > 5)
            {
                result.Command = CronCommand.Parse(string.Join(" ", parts, 5, parts.Length - 5));
            }

            for (int i = 0; i < 5; i++)
            {
                string text = parts[i].Trim(' ', '\t').ToLower();
                switch (i)
                {
                    case 3:
                        text = text.Replace("jan", "1");
                        text = text.Replace("feb", "2");
                        text = text.Replace("mar", "3");
                        text = text.Replace("apr", "4");
                        text = text.Replace("may", "5");
                        text = text.Replace("jun", "6");
                        text = text.Replace("jul", "7");
                        text = text.Replace("aug", "8");
                        text = text.Replace("sep", "9");
                        text = text.Replace("oct", "10");
                        text = text.Replace("nov", "11");
                        text = text.Replace("dec", "12");
                        break;
                    case 4:
                        text = text.Replace("sun", "0");
                        text = text.Replace("mon", "1");
                        text = text.Replace("tue", "2");
                        text = text.Replace("wed", "3");
                        text = text.Replace("thu", "4");
                        text = text.Replace("fri", "5");
                        text = text.Replace("sat", "6");
                        text = text.Replace("7", "0");
                        break;
                }
                result.ranges[i].Parse(text);
            }

            return result;
        }

        /// <summary>
        /// Parses a <see cref="CronItem"/> string.<br />
        /// Format1:<br />
        /// <para>
        /// .---------------- minute (0 - 59)<br />
        /// |   .------------- hour (0 - 23)<br />
        /// |   |   .---------- day of month (1 - 31)<br />
        /// |   |   |   .------- month (1 - 12) OR jan,feb,mar,apr ... <br />
        /// |   |   |   |  .----- day of week (0 - 6) (Sunday=0 or 7)  OR sun,mon,tue,wed,thu,fri,sat <br />
        /// |   |   |   |  |<br />
        /// *   *   *   *  *  command to be executed
        /// .
        /// </para>
        /// Format2:<br />
        /// <para>
        /// @yearly (or @annually): Run once a year  - equals: 0 0 1 1 *<br />
        /// @monthly:               Run once a month - equals: 0 0 1 * *<br />
        /// @weekly:                Run once a week  - equals: 0 0 * * 0<br />
        /// @daily (or @midnight):  Run once a day   - equals: 0 0 * * *<br />
        /// @hourly:                Run once an hour - equals: 0 * * * *<br />
        /// .
        /// </para>
        /// </summary>
        /// <param name="line">Configurationline to parse.</param>
        /// <returns>Returns a new <see cref="CronItem"/> instance.</returns>
        public static CronItem Parse(string line)
        {
            return Parse(line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        readonly Range[] ranges = new Range[5]
        {
            new Range(0, 59),
            new Range(0, 59),
            new Range(1, 31),
            new Range(1, 12),
            new Range(0, 6),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CronItem"/> class.
        /// </summary>
        public CronItem()
        {
        }

        /// <summary>
        /// Gets or sets the command to execute.
        /// </summary>
        public CronCommand Command { get; set; }

        /// <summary>
        /// Gets the <see cref="CronTimeRanges"/>.
        /// </summary>
        public CronTimeRanges Ranges
        {
            get
            {
                return new CronTimeRanges(ranges);
            }
        }

        /// <summary>
        /// Gets the <see cref="CronTimeStrings"/>.
        /// </summary>
        public CronTimeStrings Strings
        {
            get
            {
                return new CronTimeStrings(ranges);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item is run monthly or not.
        /// </summary>
        public bool Monthly
        {
            get
            {
                return Strings[CronItemTimeType.Month] == "*";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item is run weekly or not.
        /// </summary>
        public bool Weekly
        {
            get
            {
                return (Weekdays.Length == 1) && Daily && Monthly;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item is run daily or not.
        /// </summary>
        public bool Daily
        {
            get
            {
                return (Strings[CronItemTimeType.Day] == "*") && Monthly && (Strings[CronItemTimeType.Weekday] == "*");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item is run hourly or not.
        /// </summary>
        public bool Hourly
        {
            get
            {
                return (Strings[CronItemTimeType.Hour] == "*") && Daily;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this item is run minutely or not.
        /// </summary>
        public bool Minutely
        {
            get
            {
                return (Strings[CronItemTimeType.Minute] == "*") && Hourly;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DayOfWeek"/>s this <see cref="CronItem"/> is run at.
        /// </summary>
        public DayOfWeek[] Weekdays
        {
            get
            {
                List<DayOfWeek> result = new List<DayOfWeek>();
                foreach (int value in Ranges[CronItemTimeType.Weekday])
                {
                    DayOfWeek dayOfWeek = (DayOfWeek)value;
                    if (!result.Contains(dayOfWeek))
                    {
                        result.Add(dayOfWeek);
                    }
                }
                result.Sort();
                return result.ToArray();
            }
            set
            {
                bool[] days = new bool[7];
                foreach (DayOfWeek dayOfWeek in value)
                {
                    days[(int)dayOfWeek] = true;
                }
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < 7; i++)
                {
                    if (result.Length > 0)
                    {
                        result.Append(',');
                    }

                    result.Append(i);
                }
                Strings[CronItemTimeType.Weekday] = result.ToString();
            }
        }

        /// <summary>
        /// Gets the cron config string.
        /// </summary>
        /// <returns>A new config string.</returns>
        public override string ToString() => $"{Strings} {Command}";
    }
}
