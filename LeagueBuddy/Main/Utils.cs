using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace LeagueBuddy.Main
{
    public class Utils
    {
        public static string? GetRiotClientPath()
        {
            // Find the RiotClientInstalls file.
            var installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Riot Games/RiotClientInstalls.json");
            if (!File.Exists(installPath))
                return null;

            try
            {
                // occasionally this deserialization may error, because the RC occasionally corrupts its own
                // configuration file (wtf riot?). we will return null in that case, which will cause a prompt
                // telling the user to launch a game normally once
                var data = JsonSerializer.Deserialize<JsonNode>(File.ReadAllText(installPath));
                var rcPaths = new List<string?> { data?["rc_default"]?.ToString(), data?["rc_live"]?.ToString(), data?["rc_beta"]?.ToString() };

                return rcPaths.FirstOrDefault(File.Exists);
            }
            catch
            {
                return null;
            }
        }
        public static Process? GetRiotClientUxProcess() => Process.GetProcessesByName("RiotClientUx").FirstOrDefault();
        public static Process? GetRiotClientServicesProcess() => Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
        public static Process? GetLeagueClientUxProcess() => Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
        public static Process? GetGameProcess => Process.GetProcessesByName("League of Legends (TM) Client").FirstOrDefault();
        public static void ListenToRiotClientExit(Process riotClientProcess)
        {
            riotClientProcess.EnableRaisingEvents = true;
            riotClientProcess.Exited += async (sender, e) =>
            {
                Trace.WriteLine("Detected Riot Client exit.");
                await Task.Delay(3000); // wait for a bit to ensure this is not a relaunch triggered by the RC

                var newProcess = GetRiotClientServicesProcess();
                if (newProcess is not null)
                {
                    Trace.WriteLine("A new Riot Client process spawned, monitoring that for exits.");
                    ListenToRiotClientExit(newProcess);
                }
                else
                {
                    Trace.WriteLine("No new clients spawned after waiting, killing ourselves.");
                    Environment.Exit(0);
                }
            };
        }

        public static void KillClientProcesses()
        {
            foreach (var process in GetClientProcesses())
            {
                process.Refresh();
                if (process.HasExited)
                    continue;
                process.Kill();
                process.WaitForExit();
            }
        }

        public static string CombineStringArrayAfter(string[] array, int elementsToSkip) {
            var skippedArray = array.Select(x => x).Skip(elementsToSkip).ToArray();
            return String.Join(" ", skippedArray);
        }

        public static IEnumerable<Process> GetClientProcesses()
        {
            var riotCandidates = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Where(process => process.Id != Process.GetCurrentProcess().Id).ToList();
            riotCandidates.AddRange(Process.GetProcessesByName("LeagueClient"));
            riotCandidates.AddRange(Process.GetProcessesByName("RiotClientServices"));
            return riotCandidates;
        }
    }
}
