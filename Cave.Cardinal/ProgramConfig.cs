using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using Cave.Cron;
using Cave.Logging;

namespace Cave.Cardinal
{
    static class ProgramConfig
    {
        public static void Load()
        {
            var log = new Logger("Cardinal");
            log.LogInfo("Loading configuration...");

            var configFileName = Path.ChangeExtension(typeof(ProgramConfig).Assembly.GetAssemblyFilePath(), ".ini");
            var iniSettings = IniProperties.Default;
            iniSettings.DisableEscaping = true;
            var config = IniReader.FromFile(configFileName, iniSettings);
            var service = config.ReadStructFields<ServiceSettings>("Service");
            var i = AssemblyVersionInfo.Program;
            service.Description ??= i.Description;
            service.Title ??= i.Title;
            service.Name ??= i.Product;
            service.StartMode = service.StartMode == 0 ? ServiceStartMode.Automatic : service.StartMode;
            Service = service;

            var slaves = new List<SlaveHandler>();
            foreach (var slaveName in config.ReadSection("slaves"))
            {
                var slaveSection = $"slave:{slaveName}";
                if (config.HasSection(slaveSection))
                {
                    var slaveConfig = config.ReadStructFields<SlaveConfig>(slaveSection, false);
                    slaveConfig.Name = slaveName;
                    slaves.Add(new SlaveHandler(slaveConfig));
                }
            }
            log.LogInfo($"{slaves.Count} slaves configured.");
            Slaves = slaves.AsReadOnly();

            var items = new List<CronItem>();
            foreach (var entry in config.ReadSection("cron", false))
            {
                var line = entry.Trim(new char[] { '\t', ' ' });
                if (line.Contains("#"))
                {
                    line = line.Substring(0, line.IndexOf('#')).Trim(new char[] { '\t', ' ' });
                }
                if (line.Length == 0)
                {
                    continue;
                }

                try
                {
                    items.Add(CronItem.Parse(line));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, string.Format("Error while loading crontab line #{0} {1}", items.Count, line));
                }
                finally { }
            }
            log.LogInfo($"{items.Count} cron tab entries configured.");
            Items = items.AsReadOnly();
        }

        public static ServiceSettings Service { get; private set; }

        public static IList<SlaveHandler> Slaves { get; private set; }

        public static IList<CronItem> Items { get; private set; }
    }
}
