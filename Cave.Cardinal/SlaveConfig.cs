using System;

namespace Cave.Cardinal
{
    /// <summary>
    /// Slave process configuration.
    /// </summary>
    public struct SlaveConfig
    {
        /// <summary>
        /// Name of the slave (used in configuration and logging).
        /// </summary>
        public string Name;

        /// <summary>
        /// Filename to start for the slave process.
        /// </summary>
        public string FileName;

        /// <summary>
        /// Commandline arguments for the slave process.
        /// </summary>
        public string Arguments;

        /// <summary>
        /// Working directory the slave is started at.
        /// </summary>
        public string WorkingDirectory;

        /// <summary>
        /// Timeout when waiting for slave exit or output (stderr, stdout). After exceeding the timeout the slave will be killed.
        /// </summary>
        public TimeSpan Timeout;

        /// <summary>
        /// Logfile produced by the process.
        /// </summary>
        public string LogFile;
    }
}
