using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Text.Json;
using System.Web;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Numerics;
using System.Collections.Concurrent;
using LeagueBuddy.League;
using LeagueBuddy.Main;
using LeagueBuddy.Main.DataCsses;

namespace LeagueBuddy
{
    internal class MainController {
        private string Status { get; set; } = null!;
        private string StatusFile { get; } = Path.Combine(Settings.DataDir, "status");
        private bool ConnectToMuc { get; set; } = true;
        private bool InsertedFakePlayer { get; set; }
        private bool SentFakePlayerPresence { get; set; }
        private bool SentIntroductionText { get; set; }
        private string? ValorantVersion { get; set; }

        private SslStream Incoming { get; set; } = null!;
        private SslStream Outgoing { get; set; } = null!;
        private bool Connected { get; set; }
        private string LastPresence { get; set; } = null!; // we resend this if the state changes

        internal event EventHandler? ConnectionErrored;
        private static ConcurrentQueueEnqueueEvent<string> messageQueue;
        public MainController(LCU lcu) {
            LoadStatus();
            LcuHandler.Init(lcu);
            messageQueue = new ConcurrentQueueEnqueueEvent<string>();
            messageQueue.Enqueued += (sender, e) => {
                var message = messageQueue.Dequeue();
                if (message != null) {
                    SendMessageFromFakePlayerAsync(message).GetAwaiter().GetResult();
                }
            };
        }

        public static void enqueueMessage(string message) {
            messageQueue.Enqueue(message);
        }

        public async Task SendMessageFromFakePlayerAsync(string message) {
            var stamp = DateTime.UtcNow.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss.fff");

            message = HttpUtility.HtmlEncode(message);
            var chatMessage = $"<message from='41c322a1-b328-495b-a004-5ccd3e45eae8@eu1.pvp.net/RC-Deceive' stamp='{stamp}' id='fake-{stamp}' type='chat'><body>{message}</body></message>";

            var bytes = Encoding.UTF8.GetBytes(chatMessage);
            await Incoming.WriteAsync(bytes, 0, bytes.Length);
            Trace.WriteLine("<!--DECEIVE TO RC-->" + chatMessage);
        }

        public void StartThreads(SslStream incoming, SslStream outgoing) {
            Incoming = incoming;
            Outgoing = outgoing;
            Connected = true;
            InsertedFakePlayer = false;
            SentFakePlayerPresence = false;

            Task.Run(IncomingLoopAsync);
            Task.Run(OutgoingLoopAsync);
        }

        private async Task IncomingLoopAsync() {
            try {
                int byteCount;
                var bytes = new byte[8192];

                do {
                    byteCount = await Incoming.ReadAsync(bytes, 0, bytes.Length);
                    var content = Encoding.UTF8.GetString(bytes, 0, byteCount);

                    // If this is possibly a presence stanza, rewrite it.
                    if (content.Contains("<presence")) {
                        Trace.WriteLine("<!--RC TO SERVER ORIGINAL-->" + content);
                        await PossiblyRewriteAndResendPresenceAsync(content, Status);
                    } else if (content.Contains("41c322a1-b328-495b-a004-5ccd3e45eae8@eu1.pvp.net")) {
                        //Don't send anything involving our fake user to chat servers
                        Trace.WriteLine("<!--RC TO SERVER REMOVED-->" + content);

                        var start = content.IndexOf("<body>") + 6;
                        var end = content.IndexOf("</body>");
                        if (start == -1 || end == -1) continue;

                        var message = content.Substring(start, end - start);
                        using (var sw = new StringWriter()) {
                            HttpUtility.HtmlDecode(message, sw);
                            message = sw.ToString();
                        }
                        var args = message.Split(' ');

                        switch (args[0]) {
                            case "help":
                                if (args.Length == 1) {
                                    await SendMessageFromFakePlayerAsync("help [(command)]");
                                    Thread.Sleep(200);
                                    //await SendMessageFromFakePlayerAsync("profile <?/save/on/off>"); //todo
                                    await SendMessageFromFakePlayerAsync("login <?/(alias)/add/remove/update/auto>");
                                    Thread.Sleep(200);
                                    await SendMessageFromFakePlayerAsync("multisearch [<?/on/off/prefix/suffix>]");
                                    Thread.Sleep(200);
                                    await SendMessageFromFakePlayerAsync("appear <?/offline/online/mobile>");
                                    Thread.Sleep(200);
                                    await SendMessageFromFakePlayerAsync("report [<?/on/off/message>]");
                                    Thread.Sleep(200);
                                    await SendMessageFromFakePlayerAsync("blacklist <?/add/remove> (summonerName)");
                                    Thread.Sleep(200);
                                    await SendMessageFromFakePlayerAsync("autoaccept <?/on/off>");
                                    Thread.Sleep(200);
                                    await SendMessageFromFakePlayerAsync("dodge");
                                    break;
                                }
                                switch(args[1]) {
                                    case "login":
                                        await SendMessageFromFakePlayerAsync("\"login\" can be used to swap between accounts quickly. Login data is saved in clear text on your computer, use at own risk.");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("login ? - Shows your saved logins");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("login (alias) - Logs in to the account specified");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("login <add/remove> (alias) (username) (password) - Adds/removes login for use");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("login auto (alias) - Specified account gets logged in automatically on first start");
                                        break;
                                    case "multisearch":
                                        await SendMessageFromFakePlayerAsync("\"multisearch\" creates a multisearch link for your current champion select. (Can be used to reveal solo/duo lobbies)");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("multisearch ? - Generates a sample multisearch link");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("multisearch - Generates a multisearch for your current champion select");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("multisearch <on/off> - Enabled/disables automatic creation of multisearches");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("multisearch <prefix/suffix> (url snippet) - Sets the prefix and suffix for multisearches");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("Generated multisearch link will be [prefix][comma seperated summonernames][suffix]");
                                        break;
                                    case "appear":
                                        await SendMessageFromFakePlayerAsync("\"appear\" changes how you appear in your friends friendlist. Made possible by molenzwiebel's Deceive: https://github.com/molenzwiebel/Deceive");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("appear ? - Shows your current appearance");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("appear <offline/online/mobile> - Changes your appearance");
                                        break;
                                    case "report":
                                        await SendMessageFromFakePlayerAsync("\"report\" reports all player from the aftergame lobby that isn't a friend of yours.");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("report ? - Shows you the current report message");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("report <on/off> - Turns auto reporting after game on/off");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("report message (message) - Sets the report message (bottom text box) of reports");
                                        break;
                                    case "blacklist":
                                        await SendMessageFromFakePlayerAsync("\"blacklist\" disables all potentially bannable tools (report/multisearch/autoaccept/dodge) for given summoner names. This is also important if you aren't actually using the tools since your status is still being checked for in the background.");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("blacklist ? - Lists all summoner names on your blacklist");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("blacklist <add/remove> (summoner name) - Adds/removes summoner from the blacklist");
                                        break;
                                    case "autoaccept":
                                        await SendMessageFromFakePlayerAsync("\"autoaccept\" automatically accept every ready check.");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("autoaccept ? - Shows you if autoaccept is enabled.");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync("autoaccept <enable/disable> - Enables/disables autoaccept");
                                        break;
                                    case "dodge":
                                        await SendMessageFromFakePlayerAsync("dodge - dodges the champion select without having to close the client");
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "appear":
                                if (args.Length == 1) break;
                                switch (args[1]) {
                                    case "offline":
                                        UpdateStatusAsync("offline");
                                        break;
                                    case "mobile":
                                        UpdateStatusAsync("mobile");
                                        break;
                                    case "online":
                                        UpdateStatusAsync("chat");
                                        break;
                                    case "?":
                                        await SendMessageFromFakePlayerAsync(Status == "chat" ? "You are appearing online." : "You are appearing " + Status + ".");
                                        break;
                                }
                                break;
                            case "login":
                                if (args.Length == 1) break;
                                switch (args[1]) {
                                    case "?":
                                        await SendMessageFromFakePlayerAsync((Settings.current.MainAccountKey == "" ? "Auto login is disabled." : $"Auto logging on \"{Settings.current.MainAccountKey}\".") + " Saved logins:");
                                        var savedLogins = Launcher.GetLoginKeys();
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync(String.Join(", ", savedLogins));
                                        break;
                                    case "add":
                                        if (args.Length < 5) break;
                                        if (Launcher.LoginsContainsKey(args[2])) {
                                            await SendMessageFromFakePlayerAsync("Alias is already in use.");
                                            break;
                                        }
                                        await SendMessageFromFakePlayerAsync(Launcher.AddLogin(args[2], args[3], args[4]) ? "The Login has been added." : "Failed saving the changes to file.");
                                        break;
                                    case "remove":
                                        if (args.Length < 3) break;
                                        if (!Launcher.LoginsContainsKey(args[2])) {
                                            await SendMessageFromFakePlayerAsync("Alias does not exist.");
                                            break;
                                        }
                                        await SendMessageFromFakePlayerAsync(Launcher.RemoveLogin(args[2]) ? "The login has been removed." : "Failed saving the changes to file.");
                                        break;
                                    case "update":
                                        if (args.Length < 4) break;
                                        if (!Launcher.LoginsContainsKey(args[2])) {
                                            await SendMessageFromFakePlayerAsync("Alias does not exist.");
                                            break;
                                        }
                                        await SendMessageFromFakePlayerAsync(Launcher.UpdateLogin(args[2], args[3]) ? "The login has been updated." : "Failed saving the changes to file.");
                                        break;
                                    case "auto":
                                        if (args.Length < 3) break;
                                        if (args[2] == "") {
                                            Settings.current.MainAccountKey = "";
                                            await SendMessageFromFakePlayerAsync($"Auto login has been disabled.");
                                            break;
                                        }

                                        var oldMainAccountKey = Settings.current.MainAccountKey;
                                        Settings.current.MainAccountKey = args[2];
                                        if (oldMainAccountKey != Settings.current.MainAccountKey)
                                            await SendMessageFromFakePlayerAsync($"Alias will be used to automatically login.");
                                        else
                                            await SendMessageFromFakePlayerAsync($"Alias does not exist.");
                                        break;
                                    default:
                                        if (Launcher.LoginsContainsKey(args[1]))
                                            Launcher.Launch(args[1]);
                                        else
                                            await SendMessageFromFakePlayerAsync("Alias is unkown.");
                                        break;
                                }
                                break;
                            case "dodge":
                                await SendMessageFromFakePlayerAsync(LcuHandler.DodgeAsync().Result ? "Phew! Dodged that bullet." : "Not in champ select.");
                                break;
                            case "autoaccept":
                                if (args.Length == 1) break;
                                switch (args[1]) {
                                    case "on":
                                        await SendMessageFromFakePlayerAsync("Auto accept " + (Settings.current.IsAutoAcceptEnabled ? "was already" : "is now") + " enabled.");
                                        Settings.current.IsAutoAcceptEnabled = true;
                                        break;
                                    case "off":
                                        await SendMessageFromFakePlayerAsync("Auto accept " + (!Settings.current.IsAutoAcceptEnabled ? "was already" : "is now") + " disabled.");
                                        Settings.current.IsAutoAcceptEnabled = false;
                                        break;
                                    case "?":
                                        await SendMessageFromFakePlayerAsync("Auto accept is currently " + (Settings.current.IsAutoAcceptEnabled ? "enabled." : "disabled."));
                                        break;
                                }
                                break;
                            case "multisearch":
                                if (args.Length == 1) {
                                    var multisearch = await LcuHandler.GetMultisearchAsync();
                                    if (multisearch == "") await SendMessageFromFakePlayerAsync("Not in champ select.");
                                    else await SendMessageFromFakePlayerAsync(multisearch);
                                    break;
                                }
                                switch (args[1]) {
                                    case "on":
                                        await SendMessageFromFakePlayerAsync("Auto multisearch " + (Settings.current.IsAutoLobbyRevealEnabled ? "was already" : "is now") + " enabled.");
                                        Settings.current.IsAutoLobbyRevealEnabled = true;
                                        break;
                                    case "off":
                                        await SendMessageFromFakePlayerAsync("Auto multisearch " + (!Settings.current.IsAutoLobbyRevealEnabled ? "was already" : "is now") + " disabled.");
                                        Settings.current.IsAutoLobbyRevealEnabled = false;
                                        break;
                                    case "?":
                                        await SendMessageFromFakePlayerAsync("Auto multisearch is currently " + (Settings.current.IsAutoLobbyRevealEnabled ? "enabled." : "disabled.") + " Example multisearch link:");
                                        Thread.Sleep(200);
                                        await SendMessageFromFakePlayerAsync($"{Settings.current.MultisearchPrefix}Raport,Krug,Scuttle%20Crab,Gromp,Murk%20Wolf{Settings.current.MultisearchSuffix}");
                                        break;
                                    case "prefix":
                                        if (args.Length < 3) break;
                                        Settings.current.MultisearchPrefix = args[2];
                                        await SendMessageFromFakePlayerAsync($"Set multisearch prefix to \"{args[2]}\".");
                                        break;
                                    case "suffix":
                                        if (args.Length < 3) break;
                                        Settings.current.MultisearchSuffix = args[2];
                                        await SendMessageFromFakePlayerAsync($"Set multisearch suffix to \"{args[2]}\".");
                                        break;
                                }
                                break;
                            case "report":
                                if (args.Length == 1) {
                                    if (await LcuHandler.ReportLobbyAsync()) await SendMessageFromFakePlayerAsync("Cleansing the rift 🤝🏻");
                                    else await SendMessageFromFakePlayerAsync("Not in end of game lobby.");
                                    break;
                                }
                                switch (args[1]) {
                                    case "on":
                                        await SendMessageFromFakePlayerAsync("Auto report " + (Settings.current.IsAutoReportEnabled ? "was already" : "is now") + " enabled.");
                                        Settings.current.IsAutoReportEnabled = true;
                                        break;
                                    case "off":
                                        await SendMessageFromFakePlayerAsync("Auto report " + (!Settings.current.IsAutoReportEnabled ? "was already" : "is now") + " disabled.");
                                        Settings.current.IsAutoReportEnabled = false;
                                        break;
                                    case "?":
                                        await SendMessageFromFakePlayerAsync("Auto report is currently " + (Settings.current.IsAutoReportEnabled ? "enabled." : "disabled.") + $" The report message is set to \"{Settings.current.ReportMessage}\"");
                                        break;
                                    case "message":
                                        if (args.Length < 3) break;
                                        Settings.current.ReportMessage = Utils.CombineStringArrayAfter(args, 2);
                                        await SendMessageFromFakePlayerAsync($"Set report message to \"{Settings.current.ReportMessage}\".");
                                        break;
                                }
                                break;
                            case "blacklist":
                                if (args.Length < 2) break;
                                switch (args[1]) {
                                    case "?":
                                        var blacklistedSummoners = Settings.current.GetBlacklistedSummoners();
                                        if (blacklistedSummoners.Length == 0) {
                                            await SendMessageFromFakePlayerAsync("You have no summoners blacklisted.");
                                        } else {
                                            await SendMessageFromFakePlayerAsync("Blacklisted summoners:");
                                            Thread.Sleep(200);
                                            await SendMessageFromFakePlayerAsync(String.Join(", ", blacklistedSummoners));
                                        }
                                        break;
                                    case "add":
                                        if (args.Length < 3) break;
                                        var name = Utils.CombineStringArrayAfter(args, 2);
                                        await SendMessageFromFakePlayerAsync(name + " " + (Settings.current.AddAccountToBlacklist(name) ? "is now" : "was already") + " blacklisted.");
                                        break;
                                    case "remove":
                                        if (args.Length < 3) break;
                                        name = Utils.CombineStringArrayAfter(args, 2);
                                        await SendMessageFromFakePlayerAsync(name + " " + (Settings.current.RemoveAccountFromBlacklist(name) ? "is now" : "was already") + " whitelisted.");
                                        break;
                                }
                                break;
                            case "chatname":
                                if(args.Length < 3) break;
                                Settings.current.ChatName = Utils.CombineStringArrayAfter(args, 1);
                                await SendMessageFromFakePlayerAsync("The name will update on the next start.");
                                break;
                        }
                    } else {
                        await Outgoing.WriteAsync(bytes, 0, byteCount);
                        Trace.WriteLine("<!--RC TO SERVER-->" + content);
                    }

                    if (InsertedFakePlayer && !SentFakePlayerPresence)
                        await SendFakePlayerPresenceAsync();

                    if (!SentIntroductionText)
                        await SendIntroductionTextAsync();
                } while (byteCount != 0 && Connected);
            } catch (Exception e) {
                Trace.WriteLine("Incoming errored.");
                Trace.WriteLine(e);
            } finally {
                Trace.WriteLine("Incoming closed.");
                SaveStatus();
                if (Connected)
                    OnConnectionErrored();
            }
        }

        private async Task OutgoingLoopAsync() {
            try {
                int byteCount;
                var bytes = new byte[8192];
                do {
                    byteCount = await Outgoing.ReadAsync(bytes, 0, bytes.Length);
                    var content = Encoding.UTF8.GetString(bytes, 0, byteCount);
                    Console.WriteLine();
                    Console.WriteLine(content);
                    Console.WriteLine();
                    // Insert fake player into roster
                    const string roster = "<query xmlns='jabber:iq:riotgames:roster'>";
                    if (!InsertedFakePlayer && content.Contains(roster)) {
                        InsertedFakePlayer = true;
                        Trace.WriteLine("<!--SERVER TO RC ORIGINAL-->" + content);
                        content = content.Insert(content.IndexOf(roster, StringComparison.Ordinal) + roster.Length,
                            $"<item jid='41c322a1-b328-495b-a004-5ccd3e45eae8@eu1.pvp.net' name='&#9;{Settings.current.ChatName}' subscription='both' puuid='41c322a1-b328-495b-a004-5ccd3e45eae8'>" +
                            "<group priority='-9999'>mayiflex.dev</group>" +
                            $"<id name='&#9;https://mayiflex.dev' tagline=''/><lol name=''/>" +
                            "</item>");
                        var contentBytes = Encoding.UTF8.GetBytes(content);
                        await Incoming.WriteAsync(contentBytes, 0, contentBytes.Length);
                        Trace.WriteLine("<!--DECEIVE TO RC-->" + content);
                    } else {
                        await Incoming.WriteAsync(bytes, 0, byteCount);
                        Trace.WriteLine("<!--SERVER TO RC-->" + content);
                    }
                } while (byteCount != 0 && Connected);
            } catch (Exception e) {
                Trace.WriteLine("Outgoing errored.");
                Trace.WriteLine(e);
            } finally {
                Trace.WriteLine("Outgoing closed.");
                SaveStatus();
                if (Connected)
                    OnConnectionErrored();
            }
        }
        private async Task SendIntroductionTextAsync() {
            if (!InsertedFakePlayer)
                return;
            SentIntroductionText = true;
            await SendMessageFromFakePlayerAsync("Write me \"help\" to see all commands");
        }

        private async Task SendFakePlayerPresenceAsync() {
            SentFakePlayerPresence = true;

            var randomStanzaId = Guid.NewGuid();
            var unixTimeMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var presenceMessage =
                $"<presence from='41c322a1-b328-495b-a004-5ccd3e45eae8@eu1.pvp.net/RC-Deceive' id='b-{randomStanzaId}'>" +
                "<games>" +
                $"<keystone><st>chat</st><s.t>{unixTimeMilliseconds}</s.t><s.p>keystone</s.p></keystone>" +
                $"<league_of_legends><st>chat</st><s.t>{unixTimeMilliseconds}</s.t><s.p>league_of_legends</s.p><p>{{&quot;pty&quot;:true}}</p></league_of_legends>" + // No Region s.r keeps it in the main "League" category rather than "Other Servers" in every region with "Group Games & Servers" active
                "</games>" +
                "<show>chat</show>" +
                "</presence>";

            var bytes = Encoding.UTF8.GetBytes(presenceMessage);
            await Incoming.WriteAsync(bytes, 0, bytes.Length);
            Trace.WriteLine("<!--DECEIVE TO RC-->" + presenceMessage);
        }

        private void OnConnectionErrored() {
            Connected = false;
            ConnectionErrored?.Invoke(this, EventArgs.Empty);
        }

        private async Task UpdateStatusAsync(string newStatus) {
            if (string.IsNullOrEmpty(LastPresence))
                return;

            await PossiblyRewriteAndResendPresenceAsync(LastPresence, newStatus);

            if (Status == "chat")
                await SendMessageFromFakePlayerAsync("You are now appearing online.");
            else
                await SendMessageFromFakePlayerAsync("You are now appearing " + Status + ".");
        }

        private void LoadStatus() {
            if (File.Exists(StatusFile)) {
                var statusText = File.ReadAllText(StatusFile);
                var possible = new string[] { "offline", "mobile", "chat" };
                Status = possible.Contains(statusText) ? statusText : "chat";
            } else {
                Status = "chat";
            }
        }

        private void SaveStatus() => File.WriteAllText(StatusFile, Status);



        private async Task PossiblyRewriteAndResendPresenceAsync(string content, string targetStatus) {
            try {
                if (Status != targetStatus) {
                    Status = targetStatus;
                    SaveStatus();
                }
                LastPresence = content;
                var wrappedContent = "<xml>" + content + "</xml>";
                var xml = XDocument.Load(new StringReader(wrappedContent));

                if (xml.Root is null)
                    return;
                if (xml.Root.HasElements is false)
                    return;

                foreach (var presence in xml.Root.Elements()) {
                    if (presence.Name != "presence")
                        continue;
                    if (presence.Attribute("to") is not null) {
                        if (ConnectToMuc)
                            continue;
                        presence.Remove();
                    }

                    if (targetStatus != "chat" || presence.Element("games")?.Element("league_of_legends")?.Element("st")?.Value != "dnd") {
                        presence.Element("show")?.ReplaceNodes(targetStatus);
                        presence.Element("games")?.Element("league_of_legends")?.Element("st")?.ReplaceNodes(targetStatus);
                    }

                    if (targetStatus == "chat")
                        continue;
                    presence.Element("status")?.Remove();

                    if (targetStatus == "mobile") {
                        presence.Element("games")?.Element("league_of_legends")?.Element("p")?.Remove();
                        presence.Element("games")?.Element("league_of_legends")?.Element("m")?.Remove();
                    } else {
                        presence.Element("games")?.Element("league_of_legends")?.Remove();
                    }

                    // Remove Legends of Runeterra presence
                    presence.Element("games")?.Element("bacon")?.Remove();

                    // Extracts current VALORANT from the user's own presence, so that we can show a fake
                    // player with the proper version and avoid "Version Mismatch" from being shown.
                    //
                    // This isn't technically necessary, but people keep coming in and asking whether
                    // the scary red text means Deceive doesn't work, so might as well do this and
                    // get a slightly better user experience.
                    if (ValorantVersion is null) {
                        var valorantBase64 = presence.Element("games")?.Element("valorant")?.Element("p")?.Value;
                        if (valorantBase64 is not null) {
                            var valorantPresence = Encoding.UTF8.GetString(Convert.FromBase64String(valorantBase64));
                            var valorantJson = JsonSerializer.Deserialize<JsonNode>(valorantPresence);
                            ValorantVersion = valorantJson?["partyClientVersion"]?.GetValue<string>();
                            Trace.WriteLine("Found VALORANT version: " + ValorantVersion);
                            // only resend
                            if (InsertedFakePlayer && ValorantVersion is not null)
                                await SendFakePlayerPresenceAsync();
                        }
                    }

                    // Remove VALORANT presence
                    presence.Element("games")?.Element("valorant")?.Remove();
                }

                var sb = new StringBuilder();
                var xws = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, ConformanceLevel = ConformanceLevel.Fragment, Async = true };
                using (var xw = XmlWriter.Create(sb, xws)) {
                    foreach (var xElement in xml.Root.Elements())
                        xElement.WriteTo(xw);
                }

                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                await Outgoing.WriteAsync(bytes, 0, bytes.Length);
                Trace.WriteLine("<!--DECEIVE TO SERVER-->" + sb);
            } catch (Exception e) {
                Trace.WriteLine(e);
                Trace.WriteLine("Error rewriting presence.");
            }
        }
    }
}
