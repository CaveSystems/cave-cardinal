using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using Cave.Logging;

namespace Cave.Cardinal
{
    class ProcessHandler
    {
        readonly Logger log = new();

        public ProcessHandler(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            log.SourceName = Name;
        }

        public Process Process { get; private set; }

        public TimeSpan Timeout { get; set; }

        public string WorkingDirectory { get; set; }

        public string Arguments { get; set; }

        public string FileName { get; set; }

        public string Name { get; }

        public string FlagFile { get; set; }

        public bool IsRunning { get; private set; }

        public event EventHandler<EventArgs> Started;

        public event EventHandler<EventArgs> Exited;

        public int RunRedirected()
        {
            log.LogDebug($"Start process {Name} '<cyan>{FileName}<default>' '<cyan>{Arguments}<default>'");
            var startInfo = new ProcessStartInfo()
            {
                WorkingDirectory = Environment.ExpandEnvironmentVariables(WorkingDirectory ?? string.Empty),
                FileName = Environment.ExpandEnvironmentVariables(FileName ?? string.Empty),
                Arguments = Environment.ExpandEnvironmentVariables(Arguments ?? string.Empty),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                ErrorDialog = false,
            };
            try
            {
                using (var outputWaitHandle = new ManualResetEvent(false))
                using (var errorWaitHandle = new ManualResetEvent(false))
                using (Process = Process.Start(startInfo))
                {
                    log.LogVerbose($"Start reading from process [<cyan>{Process.Id}<default>] <cyan>{Name}");
                    Process.ErrorDataReceived += (sender, e) => { if (e.Data != null) { StdErr(e.Data); } else { errorWaitHandle.Set(); } };
                    Process.OutputDataReceived += (sender, e) => { if (e.Data != null) { StdOut(e.Data); } else { outputWaitHandle.Set(); } };
                    Process.BeginErrorReadLine();
                    Process.BeginOutputReadLine();
                    log.LogVerbose($"Wait for exit [<cyan>{Process.Id}<default>] <cyan>{Name}");

                    IsRunning = true;
                    Started?.Invoke(this, new EventArgs());

                    var watch = StopWatch.StartNew();
                    while (!Process.HasExited)
                    {
                        if (Timeout.Ticks > 0 && watch.Elapsed > Timeout)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                        if (Program.TestFlagFile(FlagFile))
                        {
                            throw new Exception($"Process stop requested by flagfile {FlagFile}!");
                        }
                    }

                    var result = Process.HasExited && outputWaitHandle.WaitOne() && errorWaitHandle.WaitOne();
                    if (result)
                    {
                        log.LogVerbose($"Process <cyan>{Name}<default> exited with code <cyan>{Process.ExitCode}<default>.");
                        var exitCode = Process.ExitCode;
                        Process = null;
                        return exitCode;
                    }
                    else
                    {
                        throw new TimeoutException($"Process <red>{Name}<default> timed out.");
                    }
                }
            }
            finally
            {
                KillProcessAndChildren();
                Process = null;
                IsRunning = false;
                Exited?.Invoke(this, new EventArgs());
            }
        }

        public void KillProcessAndChildren()
        {
            if (Process != null)
            {
                KillProcessAndChildren(Process);
            }
        }

        void StdOut(string data)
        {
            if (!string.IsNullOrEmpty(data?.Trim()))
            {
                log.LogInfo($"StdOut: <green>{data}");
            }
        }

        void StdErr(string data)
        {
            if (!string.IsNullOrEmpty(data?.Trim()))
            {
                log.LogInfo($"StdErr: <red>{data}");
            }
        }

        void KillProcessAndChildren(Process process)
        {
            using var search = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");
            var children = search.Get();
            if (children != null)
            {
                foreach (var sub in children)
                {
                    try
                    {
                        KillProcessAndChildren(Process.GetProcessById(Convert.ToInt32(sub["ProcessID"])));
                    }
                    catch (ArgumentException)
                    {
                        // Process already exited.
                    }
                }
            }
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (InvalidOperationException)
            {
                // Process already exited.
            }
        }
    }
}
