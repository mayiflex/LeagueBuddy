using LeagueBuddy.League;
using LeagueBuddy.Main;
using LeagueBuddy.Main.DataCsses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy
{
    internal class Settings {
        [JsonIgnore]
        internal static readonly string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueBuddy");
        [JsonIgnore]
        public static Settings current;
        [JsonIgnore]
        private static readonly string settingsPath = Path.Combine(Settings.DataDir, "settings");
        [JsonProperty]
        private bool isAutoAcceptEnabled;
        [JsonIgnore]
        public bool IsAutoAcceptEnabled {
            get => isAutoAcceptEnabled;
            set {
                isAutoAcceptEnabled = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private bool isAutoLobbyRevealEnabled;
        [JsonIgnore]
        public bool IsAutoLobbyRevealEnabled {
            get => isAutoLobbyRevealEnabled;
            set {
                isAutoLobbyRevealEnabled = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private bool isAutoReportEnabled;
        [JsonIgnore]
        public bool IsAutoReportEnabled {
            get => isAutoReportEnabled;
            set {
                isAutoReportEnabled = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private string chatName;
        [JsonIgnore]
        public string ChatName {
            get => chatName;
            set {
                chatName = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private string mainAccountKey;
        [JsonIgnore]
        public string MainAccountKey {
            get => mainAccountKey;
            set {
                if (value == "" || (Launcher.LoginsContainsKey(value) && Launcher.GetLoginKeys().Contains(value))) {
                    mainAccountKey = value;
                    SaveSettings();
                }
            }
        }
        [JsonProperty]
        private string reportMessage;
        [JsonIgnore]
        public string ReportMessage {
            get => reportMessage;
            set {
                reportMessage = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private string multisearchPrefix;
        [JsonIgnore]
        public string MultisearchPrefix {
            get => multisearchPrefix;
            set {
                multisearchPrefix = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private string multisearchSuffix;
        [JsonIgnore]
        public string MultisearchSuffix {
            get => multisearchSuffix;
            set {
                multisearchSuffix = value;
                SaveSettings();
            }
        }
        [JsonProperty]
        private HashSet<string> blacklistedSummoners;
        [JsonProperty]
        private HashSet<KnownInter> intlist;

        [JsonConstructor]
        private Settings(bool isAutoAcceptEnabled, bool isAutoLobbyRevealEnabled, bool isAutoReportEnabled, string chatName, string mainAccountKey, string reportMessage, string multisearchPrefix, string multisearchSuffix, HashSet<string> blacklistedSummoners, HashSet<KnownInter> intlist) {
            this.isAutoAcceptEnabled = isAutoAcceptEnabled;
            this.isAutoLobbyRevealEnabled = isAutoLobbyRevealEnabled;
            this.isAutoReportEnabled = isAutoReportEnabled;
            this.chatName = chatName;
            this.mainAccountKey = mainAccountKey;
            this.reportMessage = reportMessage;
            this.multisearchPrefix = multisearchPrefix;
            this.multisearchSuffix = multisearchSuffix;
            this.blacklistedSummoners = blacklistedSummoners;
            this.intlist = intlist;
        }

        private Settings() {
            for (int i = 0; i < 3; i++) {
                if (loadSettings()) break;
                SetDefault();
            }
        }

        public void SetDefault() {
            this.isAutoAcceptEnabled = false;
            this.isAutoLobbyRevealEnabled = false;
            this.isAutoReportEnabled = false;
            this.chatName = "LeagueBuddy";
            this.mainAccountKey = "";
            this.reportMessage = "toxic, racist";
            this.multisearchPrefix = "https://porofessor.gg/pregame/euw/";
            this.multisearchSuffix = "/soloqueue";
            this.blacklistedSummoners = new HashSet<string>();
            this.intlist = new HashSet<KnownInter>();
        }
        private bool loadSettings() {
            if (!File.Exists(settingsPath)) return false;
            try {
                var rawJson = File.ReadAllText(settingsPath);
                var settings = JsonConvert.DeserializeObject<Settings>(rawJson);
                this.isAutoAcceptEnabled = settings.IsAutoAcceptEnabled;
                this.isAutoLobbyRevealEnabled = settings.IsAutoLobbyRevealEnabled;
                this.isAutoReportEnabled = settings.IsAutoReportEnabled;
                this.chatName = settings.chatName;
                this.mainAccountKey = settings.MainAccountKey;
                this.reportMessage = settings.ReportMessage;
                this.multisearchPrefix = settings.MultisearchPrefix;
                this.multisearchSuffix = settings.MultisearchSuffix;
                this.blacklistedSummoners = settings.blacklistedSummoners;
                if (blacklistedSummoners == null) this.blacklistedSummoners = new HashSet<string>(); 
                this.intlist = settings.intlist;
                if(intlist == null) this.intlist = new HashSet<KnownInter>();
                return true;
            } catch {
                return false;
            }
        }
        public static void LoadSettings() {
            Settings.current = new Settings();
        }
        public bool SaveSettings() {
            try {
                File.WriteAllText(settingsPath, JsonConvert.SerializeObject(this));
                return true;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return false;
            }
        }

        public bool AddAccountToBlacklist(string summonerName) {
            var safeAccountCount = blacklistedSummoners.Count();
            blacklistedSummoners.Add(summonerName);
            SaveSettings();
            LcuHandler.IsCurrentSummonerBlacklistedAsync();
            return safeAccountCount != blacklistedSummoners.Count();
        }
        public bool RemoveAccountFromBlacklist(string summonerName) {
            var safeAccountCount = blacklistedSummoners.Count();
            blacklistedSummoners.Remove(summonerName);
            SaveSettings();
            return safeAccountCount != blacklistedSummoners.Count();
        }
        public bool ContainsBlacklistedSummoner(string summonerName) {
            return blacklistedSummoners.Select(x => x.ToLower()).Contains(summonerName.ToLower());
        }
        public string[] GetBlacklistedSummoners() {
            return blacklistedSummoners.Select(x => x).ToArray();
        }

        internal static void CreateDataDir() {
            if (Directory.Exists(DataDir)) return;
            Directory.CreateDirectory(DataDir);
        }
    }
}
