using System.ComponentModel;
using System.ServiceProcess;

namespace Cave.Cardinal
{
    /// <summary>
    /// Provides all settings for the service.
    /// </summary>
    public struct ServiceSettings
    {
        /// <summary>
        /// Indicates the name used by the system to identify this service.
        /// </summary>
        public string Name;

        /// <summary>
        /// Indicates the friendly name that identifies the service to the user.
        /// </summary>
        public string Title;

        /// <summary>
        /// The description of the service.
        /// </summary>
        public string Description;

        /// <summary>
        /// Indicates the start mode of the service.
        /// </summary>
        public ServiceStartMode StartMode;

        /// <summary>
        /// Specifies a service's security context, which defines its logon type.
        /// </summary>
        public ServiceAccount Account;

        /// <summary>
        /// Indicates whether the service should be delayed from starting until other automatically started services are running.
        /// </summary>
        public bool DelayedAutoStart;

        /// <summary>
        /// User account under which the service application will run.
        /// </summary>
        public string Username;

        /// <summary>
        /// Password associated with the user account under which the service application runs.
        /// </summary>
        public string Password;
    }
}
