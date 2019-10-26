using System.Text;
using Cave.Collections;

namespace Cave.Cron
{
    /// <summary>
    /// Provides access to the time range a <see cref="CronItem"/> is run at.
    /// </summary>
    public class CronTimeRanges
    {
        readonly Range[] ranges;

        internal CronTimeRanges(Range[] ranges)
        {
            this.ranges = ranges;
        }

        /// <summary>
        /// Gets the specified time range.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Returns the time range.</returns>
        public Range this[CronItemTimeType type]
        {
            get
            {
                return ranges[(int)type];
            }
        }

        /// <summary>
        /// Gets the ranges as cron config string.
        /// </summary>
        /// <returns>A new config string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Range range in ranges)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(range.ToString());
            }
            return sb.ToString();
        }
    }
}
