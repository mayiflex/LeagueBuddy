using LeagueBuddy.League.DTOs;
using LeagueBuddy.Main;
using Newtonsoft.Json;
using Swan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LeagueBuddy.League
{
    internal class LcuHandler
    {
        private static LCU? riotLcu = null;
        private static LCU? leagueLcu = null;
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static bool inChampSelect = false;
        private static bool inGame = false;
        private static bool inEndOfGameLobby = false;
        private static bool hasInit = false;
        private static Task? updatePlayerStatusTask = null;
        private static string lastMultisearch = "";
        public static void Init(LCU lcu)
        {
            if (updatePlayerStatusTask != null)
            {
                cts.Cancel();
                updatePlayerStatusTask.Wait();
            }
            cts = new CancellationTokenSource();
            if (lcu.origin == LCU.Hook.riotClientUx)
            {
                riotLcu = lcu;
                leagueLcu = null;
                AttachToLeagueClientOnStartup();
            }
            updatePlayerStatusTask = runPlayerStateTask();
        }
        public static void RemoveInitStatus()
        {
            cts.Cancel();
            riotLcu = null;
            leagueLcu = null;
            hasInit = false;
            inChampSelect = false;
            inGame = false;
            hasInit = false;
        }
        public static Task AttachToLeagueClientOnStartup()
        {
            return Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(250);
                } while (Utils.GetLeagueClientUxProcess() == null);
                //await Task.Delay(10000);
                leagueLcu = new LCU(LCU.Hook.leagueClientUx);
                riotLcu.UpdateRiotAuthWithLeagueOrigin(leagueLcu);
                Console.WriteLine("\n\n\n\n\n\n");
                Console.WriteLine(leagueLcu.IsConnected);
                hasInit = true;
            });
        }

        public static async Task<bool> DodgeAsync()
        {
            if (!hasInit) return false;
            if (!inChampSelect || leagueLcu == null) return false;
            await leagueLcu.Request(LCU.RequestMethod.POST, "/lol-login/v1/session/invoke?destination=lcdsServiceProxy&method=call&args=[\"\",\"teambuilder-draft\",\"quitV2\",\"\"]", "");
            return true;
        }
        public static async Task<string> GetMultisearchAsync()
        {
            var players = await GetLobbyParticpantsAsync();
            if (players.Length == 0) return "";
            var sb = new StringBuilder(Settings.current.MultisearchPrefix);
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                var playerName = player.name == "" ? player.game_name : player.name;
                sb.Append(HttpUtility.UrlEncode(playerName));
                if (i < players.Length - 1)
                    sb.Append(',');
            }
            sb.Append(Settings.current.MultisearchSuffix);
            return sb.ToString();

        }
        public static async Task<bool> ReportLobbyAsync()
        {
            var friends = await GetFriendsAsync();
            var endOfGame = await GetEndOfGameAsync();
            if (friends.Length == 0) return false;
            if (endOfGame == null) return false;
            foreach (var team in endOfGame.teams)
            {
                foreach (var player in team.players)
                {
                    if (player.summonerId == endOfGame.localPlayer.summonerId) continue;
                    if (friends.Where(x => player.summonerId == x.summonerId).Count() > 0) continue;

                    var reportAnswer = await SendPlayerReportAsync(player.summonerId, endOfGame.gameId);
                }
            }
            return true;
        }

        public static async Task<bool> IsCurrentSummonerBlacklistedAsync() {
            var self = await GetSelfSummonerAsync();
            if (!Settings.current.ContainsBlacklistedSummoner(self.name)) return false;
            cts.Cancel();
            MainController.enqueueMessage("You are on a blacklisted account. The commands multisearch, autoaccept, dodge and report aren't available.");
            return true;
        }

        private async static void replacePlayerStateTaskWithKeepAlive() {
            await Task.Delay(5000);
            cts = new CancellationTokenSource();
            updatePlayerStatusTask = Task.Run(() => {
                while (!cts.IsCancellationRequested) {
                    Thread.Sleep(3600000);
                }
            });
        }

        private static Task runPlayerStateTask()
        {
            return Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    if (!hasInit)
                    {
                        Thread.Sleep(2500);
                        continue;
                    }
                    if (!IsCurrentSummonerBlacklistedAsync().Result) break;
                    //replacePlayerStateTaskWithKeepAlive();
                    return;
                }

                var multisearchTries = 0;
                while (!cts.IsCancellationRequested)
                {
                    //in game no need to check for queue
                    var gameClient = Utils.GetGameProcess;
                    if (gameClient != null)
                    {
                        inChampSelect = false;
                        inGame = true;
                        Thread.Sleep(10000);
                        continue;
                    }
                    else if (inGame)
                    {
                        //gameclient closed but didnt still has inGame status => aftergame lobby
                        inGame = false;
                        inEndOfGameLobby = true;
                        if (Settings.current.IsAutoReportEnabled)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                Thread.Sleep(750);
                                if (GetEndOfGameAsync() != null)
                                {
                                    await ReportLobbyAsync();
                                    break;
                                }
                            }
                        }
                    }
                    inGame = false;

                    //check if match found
                    if (!getSearchStateAsync().Result.Contains("Found"))
                    {
                        inChampSelect = false;
                        Thread.Sleep(1000);
                        continue;
                    }

                    //game found but not in champ select = ready check
                    if (getChampSelectAsync().Result.Contains("RPC_ERROR"))
                    {
                        inChampSelect = false;
                        if (Settings.current.IsAutoAcceptEnabled) await acceptReadyCheckAsync();
                        Thread.Sleep(1174);
                        continue;
                    }
                    else
                    { //in champ select
                        inChampSelect = true;
                        if (Settings.current.IsAutoLobbyRevealEnabled)
                        {
                            var multisearch = await GetMultisearchAsync();
                            if (multisearch.Split(',').Length >= 5 || multisearchTries >= 7) {
                                if (multisearch != lastMultisearch && multisearch != "") {
                                    MainController.enqueueMessage(multisearch);
                                }
                                lastMultisearch = multisearch;
                                multisearchTries = 0;
                            } else {
                                multisearchTries++;
                                Thread.Sleep(1000);
                                continue;
                            }
                        }
                        Thread.Sleep(5000);
                    }
                }
            });
        }

        private static async Task<string> getChampSelectAsync()
        {
            if (!hasInit) return "";
            return await leagueLcu.Request(LCU.RequestMethod.GET, "/lol-champ-select/v1/session");
        }
        private static async Task<string> getSearchStateAsync()
        {
            if (!hasInit) return "";
            return await leagueLcu.Request(LCU.RequestMethod.GET, "/lol-lobby/v2/lobby/matchmaking/search-state");
        }
        private static async Task<string> acceptReadyCheckAsync()
        {
            return await leagueLcu.Request(LCU.RequestMethod.POST, "/lol-matchmaking/v1/ready-check/accept", "");
        }
        private static async Task<SummonerDTO[]> GetFriendsAsync()
        {
            try
            {
                var rawJson = await leagueLcu.Request(LCU.RequestMethod.GET, "/lol-chat/v1/friends");
                return JsonConvert.DeserializeObject<SummonerDTO[]>(rawJson);
            }
            catch
            {
                return Array.Empty<SummonerDTO>();
            }
        }
        private static async Task<EndOfGameDTO?> GetEndOfGameAsync()
        {
            try
            {
                var rawJson = await leagueLcu.Request(LCU.RequestMethod.GET, "/lol-end-of-game/v1/eog-stats-block");
                return JsonConvert.DeserializeObject<EndOfGameDTO>(rawJson);
            }
            catch
            {
                return null;
            }
        }
        private static async Task<ParticipantDTO[]> GetLobbyParticpantsAsync()
        {
            if (!inChampSelect || !hasInit) return Array.Empty<ParticipantDTO>();
            try
            {
                var rawJson = await riotLcu.Request(LCU.RequestMethod.GET, "/chat/v5/participants/champ-select");
                rawJson = rawJson.Substring("{\"participants\":".Length, rawJson.Length - "{\"participants\":".Length - 1);
                return JsonConvert.DeserializeObject<ParticipantDTO[]>(rawJson);
            }
            catch
            {
                return Array.Empty<ParticipantDTO>();
            }
        }
        private static async Task<string> SendPlayerReportAsync(long playerId, long gameId)
        {
            var payload = $"{{\"comment\": \"{Settings.current.ReportMessage}\", \"gameId\": {gameId.ToString()}, \"offenses\": \"Negative Attitude, Verbal Abuse\", \"reportedSummonerId\": {playerId.ToString()}}}";
            return await leagueLcu.Request(LCU.RequestMethod.POST, "/lol-end-of-game/v2/player-complaints", payload);
        }
        private static async Task<SummonerDTO?> GetSelfSummonerAsync()
        {
            try
            {
                var rawJson = await leagueLcu.Request(LCU.RequestMethod.GET, "/lol-chat/v1/me");
                return JsonConvert.DeserializeObject<SummonerDTO?>(rawJson);
            }
            catch
            {
                return null;
            }
        }
    }
}
