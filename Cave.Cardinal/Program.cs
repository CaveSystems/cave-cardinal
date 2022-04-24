using System;
using System.IO;
using System.Threading.Tasks;
using Cave.Cron;
using Cave.Logging;
using Cave.Service;

namespace Cave.Cardinal
{
    class Program : ServiceProgram
    {
        public static bool TestFlagFile(string flagFile)
        {
            try
            {
                if (flagFile == null) return false;
                var fileName = Environment.ExpandEnvironmentVariables(flagFile);
                return File.Exists(fileName);
            }
            catch { return false; }
        }

        static void Main()
        {
            using var program = new Program();
            program.Run();
        }

        public Program()
        {
            try
            {
                ProgramConfig.Load();
            }
            catch(Exception ex)
            {
                throw new Exception("Could not load configuration.", ex);
            }
            ServiceName = ProgramConfig.Service.Name;
        }

        protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Escape: ServiceParameters.CommitShutdown(); break;
            }
        }

        protected override void Worker()
        {
            log = new Logger(ServiceName);
            log.LogInfo("Starting slaves...");
            Parallel.ForEach(ProgramConfig.Slaves, slave => slave.Start());
            log.LogInfo("Waiting for events...");

            lastCheck = (DateTime.Now.Ticks / TimeSpan.TicksPerMinute) - 1;
            while (!ServiceParameters.Shutdown)
            {
                CheckCronItems();
                ServiceParameters.WaitForShutdown(1000);
            }

            log.LogInfo("Shutting down slaves...");
            Parallel.ForEach(ProgramConfig.Slaves, slave => slave.Stop());
            log.LogInfo("Exit worker...");
        }

        void CheckCronItems()
        {
            now = DateTime.Now.Ticks / TimeSpan.TicksPerMinute;

            if (now != lastCheck)
            {
                for (var i = 0; i < ProgramConfig.Items.Count; i++)
                {
                    CheckCronItem(i);
                }
            }

            lastCheck = now;
        }

        void CheckCronItem(int itemNumber)
        {
            var item = ProgramConfig.Items[itemNumber];

            // check all minutes since last check
            for (var minute = lastCheck + 1; minute <= now; minute++)
            {
                var checkTime = new DateTime(minute * TimeSpan.TicksPerMinute);

                var runMonth = item.Ranges[CronItemTimeType.Month].Contains(checkTime.Month);
                var runDay = item.Ranges[CronItemTimeType.Day].Contains(checkTime.Day);
                var runWeekday = item.Ranges[CronItemTimeType.Weekday].Contains((int)checkTime.DayOfWeek);
                var runHour = item.Ranges[CronItemTimeType.Hour].Contains(checkTime.Hour);
                var runMinute = item.Ranges[CronItemTimeType.Minute].Contains(checkTime.Minute);

                if (runMonth && runDay && runWeekday && runHour && runMinute)
                {
                    var name = $"Cron#{itemNumber}";
                    try
                    {
                        var processHandler = new ProcessHandler(name)
                        {
                            FileName = item.Command.Command,
                            Arguments = item.Command.Parameters,
                        };
                        log.LogDebug($"Slave <cyan>{name}<default> process starting...");
                        var exitCode = processHandler.RunRedirected();
                        log.LogNotice($"Slave <cyan>{name}<default> process exited with exit code <cyan>{exitCode}<default>.");
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Slave <cyan>{name}<default> process caused exception.");
                    }
                }
            }
        }

        Logger log;
        long lastCheck;
        long now;
    }
}
