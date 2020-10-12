using System.Text;
using Cave.Collections;

namespace Cave.Cron
{
    /// <summary>
    /// Provides access to the time range a <see cref="CronItem"/> is run at.
    /// </summary>
    public class CronTimeStrings
    {
        readonly Range[] ranges;

        internal CronTimeStrings(Range[] ranges) => this.ranges = ranges;

        /// <summary>
        /// Gets the string for the specified time range.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Returns the time range.</returns>
        public string this[CronItemTimeType type]
        {
            get => ranges[(int)type].ToString();
            set => ranges[(int)type].Parse(value);
        }

        /// <summary>
        /// Gets the ranges as cron config string.
        /// </summary>
        /// <returns>A new config string.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var range in ranges)
            {
                if (result.Length > 0)
                {
                    result.Append(" ");
                }

                result.Append(range);
            }
            return result.ToString();
        }
    }
}
