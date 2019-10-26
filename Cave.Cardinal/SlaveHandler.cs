using System;
using System.Threading;
using Cave.Logging;

namespace Cave.Cardinal
{
    class SlaveHandler
    {
        public SlaveConfig Config { get; }

        readonly Logger log;
        Thread thread;
        ProcessHandler processHandler;
        volatile bool exit;

        public SlaveHandler(SlaveConfig config)
        {
            Config = config;
            log = new Logger(Config.Name);
        }

        public void Start()
        {
            log.LogDebug($"Slave <cyan>{Config.Name}<default> handling startup...");
            if (thread != null)
            {
                throw new InvalidOperationException($"Slave {Config.Name} already started!");
            }

            thread = new Thread(Worker);
            thread.Start();
        }

        public void Stop()
        {
            log.LogDebug($"Slave <cyan>{Config.Name}<default> handling shutdown...");
            exit = true;
            processHandler?.KillProcessAndChildren();
            thread.Join();
            log.LogNotice($"Slave <cyan>{Config.Name}<default> shutdown complete.");
        }

        void Worker(object state)
        {
            log.LogNotice($"Slave <cyan>{Config.Name}<default> handling started.");
            while (!exit)
            {
                log.LogDebug($"Slave <cyan>{Config.Name}<default> process startup in 5s...");
                for (var waitTill = DateTime.UtcNow.AddSeconds(5); DateTime.UtcNow < waitTill;)
                {
                    Thread.Sleep(100);
                    if (exit)
                    {
                        break;
                    }
                }
                try
                {
                    processHandler = new ProcessHandler(Config.Name)
                    {
                        FileName = Config.FileName,
                        Arguments = Config.Arguments,
                        Timeout = Config.Timeout,
                        WorkingDirectory = Config.WorkingDirectory,
                    };
                    log.LogInfo($"Slave <cyan>{Config.Name}<default> process starting...");
                    var exitCode = processHandler.RunRedirected();
                    log.LogInfo($"Slave <cyan>{Config.Name}<default> process exited with exit code <cyan>{exitCode}<default>.");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Slave <cyan>{Config.Name}<default> process caused exception.");
                }
                finally
                {
                    processHandler = null;
                }
            }
            log.LogDebug($"Slave <cyan>{Config.Name}<default> handling finished.");
        }
    }
}
