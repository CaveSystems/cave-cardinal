using System.ComponentModel;
using System.ServiceProcess;

namespace Cave.Cardinal
{
    /// <summary>
    /// Provides the service installer.
    /// </summary>
    [RunInstaller(true)]
    public sealed class CardinalServiceInstaller : ServiceInstaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardinalServiceInstaller"/> class.
        /// </summary>
        public CardinalServiceInstaller()
        {
            Description = ProgramConfig.Service.Description;
            DisplayName = ProgramConfig.Service.Title;
            ServiceName = ProgramConfig.Service.Name;
            StartType = ProgramConfig.Service.StartMode;

#if !NET20 && !NET35
            DelayedAutoStart = ProgramConfig.Service.DelayedAutoStart;
#endif
        }
    }
}
