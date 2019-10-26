using System.ComponentModel;
using System.ServiceProcess;

namespace Cave.Cardinal
{
    /// <summary>
    /// Provides the server installer properties.
    /// </summary>
    [RunInstaller(true)]
    public sealed class CardinalServiceInstallerProcess : ServiceProcessInstaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardinalServiceInstallerProcess"/> class.
        /// </summary>
        public CardinalServiceInstallerProcess()
        {
            Account = ProgramConfig.Service.Account;
            if (ProgramConfig.Service.Account == ServiceAccount.User)
            {
                Username = ProgramConfig.Service.Username;
                Password = ProgramConfig.Service.Password;
            }
        }
    }
}
