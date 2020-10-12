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
        static ProgramConfig()
        {
            var log = new Logger("Cardinal");
            log.LogInfo("Loading configuration...");

            var configFileName = Path.ChangeExtension(typeof(ProgramConfig).Assembly.GetAssemblyFilePath(), ".ini");
            var config = IniReader.FromFile(configFileName);
            var service = config.ReadStructFields<ServiceSettings>("Service");
            AssemblyVersionInfo i = AssemblyVersionInfo.Program;
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
                    var slaveConfig = config.ReadStruct<SlaveConfig>(slaveSection);
                    slaveConfig.Name = slaveName;
                    slaves.Add(new SlaveHandler(slaveConfig));
                }
            }
            log.LogInfo($"{slaves.Count} slaves configured.");
            Slaves = slaves.AsReadOnly();

            var items = new List<CronItem>();
            foreach (var entry in config.ReadSection("cron", false))
            {
                string line = entry.Trim(new char[] { '\t', ' ' });
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

        public static ServiceSettings Service { get; }

        public static IList<SlaveHandler> Slaves { get; }

        public static IList<CronItem> Items { get; }
    }
}
