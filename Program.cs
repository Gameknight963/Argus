using System;
using System.Diagnostics;
using System.Text.Json;
using System.ServiceProcess;

namespace  Argus
{
    public class ArgusService : ServiceBase
    {
        private Thread? _watchThread;
        private bool _running = false;

        // Config
        private int _restartThreshold = 3;      // how many restarts...
        private int _restartWindowSeconds = 10; // ...within this many seconds
        private List<string> _killList = new List<string> { "Windhawk" };

        private Queue<DateTime> _dwmRestarts = new Queue<DateTime>();

        static void Main(string[] args)
        {
            Run(new ArgusService());
        }

        protected override void OnStart(string[] args)
        {
            LoadConfig();
            _running = true;
            _watchThread = new Thread(Watch);
            _watchThread.Start();
        }

        protected override void OnStop()
        {
            _running = false;
            _watchThread?.Join();
        }

        private void Watch()
        {
            Process? dwmProcess = null;

            while (_running)
            {
                // Find DWM if we don't have it
                if (dwmProcess == null || dwmProcess.HasExited)
                {
                    if (dwmProcess != null)
                    {
                        // DWM just died — record it
                        Log("DWM restart detected");
                        _dwmRestarts.Enqueue(DateTime.Now);
                        PruneOldRestarts();

                        if (_dwmRestarts.Count >= _restartThreshold)
                        {
                            Log($"DWM restarted {_dwmRestarts.Count} times in {_restartWindowSeconds}s — killing suspects");
                            KillSuspects();
                            _dwmRestarts.Clear();
                        }
                    }

                    // Wait for DWM to come back up
                    dwmProcess = null;
                    while (_running && dwmProcess == null)
                    {
                        Process[] procs = Process.GetProcessesByName("dwm");
                        if (procs.Length > 0)
                            dwmProcess = procs[0];
                        else
                            Thread.Sleep(500);
                    }
                }
                Thread.Sleep(200);
            }
        }

        private void PruneOldRestarts()
        {
            DateTime cutoff = DateTime.Now.AddSeconds(-_restartWindowSeconds);
            while (_dwmRestarts.Count > 0 && _dwmRestarts.Peek() < cutoff)
                _dwmRestarts.Dequeue();
        }

        private void KillSuspects()
        {
            foreach (string name in _killList)
            {
                Process[] procs = Process.GetProcessesByName(name);
                foreach (Process p in procs)
                {
                    try
                    {
                        Log($"Killing {name} (PID {p.Id})");
                        p.Kill();
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to kill {name}: {ex.Message}");
                    }
                }
            }
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "argus.json");
            if (!File.Exists(configPath)) return;

            try
            {
                string json = File.ReadAllText(configPath);
                ArgusConfig? config = JsonSerializer.Deserialize<ArgusConfig>(json);
                _restartThreshold = config!.RestartThreshold;
                _restartWindowSeconds = config.RestartWindowSeconds;
                _killList = config.KillList;
            }
            catch (Exception ex)
            {
                Log($"Failed to load config: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(line);
            File.AppendAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "argus.log"),
                line + Environment.NewLine
            );
        }
    }
}