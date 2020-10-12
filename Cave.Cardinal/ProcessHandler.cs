using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using Cave.Logging;

namespace Cave.Cardinal
{
    class ProcessHandler
    {
        Process process;
        readonly Logger log = new Logger();

        public ProcessHandler(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            log.SourceName = Name;
        }

        public Process Process => process;

        public TimeSpan Timeout { get; set; }

        public string WorkingDirectory { get; set; }

        public string Arguments { get; set; }

        public string FileName { get; set; }

        public string Name { get; }

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
                RedirectStandardInput = true,
            };
            try
            {
                using (ManualResetEvent outputWaitHandle = new ManualResetEvent(false))
                using (ManualResetEvent errorWaitHandle = new ManualResetEvent(false))
                using (process = Process.Start(startInfo))
                {
                    log.LogVerbose($"Start reading from process [<cyan>{process.Id}<default>] <cyan>{Name}");
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) { StdErr(e.Data); } else { errorWaitHandle.Set(); } };
                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) { StdOut(e.Data); } else { outputWaitHandle.Set(); } };
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    log.LogVerbose($"Wait for exit [<cyan>{process.Id}<default>] <cyan>{Name}");
                    var timeoutMilliseconds = (int)Timeout.TotalMilliseconds;
                    if (timeoutMilliseconds <= 0)
                    {
                        timeoutMilliseconds = -1;
                    }

                    IsRunning = true;
                    Started?.Invoke(this, new EventArgs());
                    var result = process.WaitForExit(timeoutMilliseconds);
                    result &= outputWaitHandle.WaitOne(timeoutMilliseconds);
                    result &= errorWaitHandle.WaitOne(timeoutMilliseconds);

                    if (result)
                    {
                        log.LogVerbose($"Process <cyan>{Name}<default> exited with code <cyan>{process.ExitCode}<default>.");
                        int exitCode = process.ExitCode;
                        process = null;
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
                process = null;
                IsRunning = false;
                Exited?.Invoke(this, new EventArgs());
            }
        }

        public void KillProcessAndChildren()
        {
            if (process != null)
            {
                KillProcessAndChildren(process);
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
