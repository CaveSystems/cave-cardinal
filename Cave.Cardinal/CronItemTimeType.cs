namespace Cave.Cron
{
    /// <summary>
    /// Provides available cron time types.
    /// </summary>
    public enum CronItemTimeType : int
    {
        /// <summary>
        /// The minute part of the cron time entry
        /// </summary>
        Minute = 0,

        /// <summary>
        /// The hour part of the cron time entry
        /// </summary>
        Hour = 1,

        /// <summary>
        /// The day part of the cron time entry
        /// </summary>
        Day = 2,

        /// <summary>
        /// The month part of the cron time entry
        /// </summary>
        Month = 3,

        /// <summary>
        /// The weekday part of the cron time entry
        /// </summary>
        Weekday = 4,
    }
}
