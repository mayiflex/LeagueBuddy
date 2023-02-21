using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using LeagueBuddy.League;
using LeagueBuddy.Main;

namespace LeagueBuddy
{
    public class Launcher {
        private static string loginsPath = Path.Combine(Settings.DataDir, "logins");
        private static Dictionary<string, UserPass> logins = new Dictionary<string, UserPass>() { { "?", new UserPass("", "") }, { "add", new UserPass("", "") }, { "remove", new UserPass("", "") }, { "update", new UserPass("", "")} };
        
        public static void LoadSettings() {
            Settings.CreateDataDir();
            Settings.LoadSettings();
        }
        private static bool SaveLogins() {
            try {
                Settings.CreateDataDir();
                File.WriteAllText(loginsPath, JsonConvert.SerializeObject(logins));
                return true;
            } catch {
                return false;
            }
        }
        public static void LoadLogins() {
            if (!File.Exists(loginsPath)) return;
            logins = JsonConvert.DeserializeObject<Dictionary<string, UserPass>>(File.ReadAllText(loginsPath));
        }
        internal static bool AddLogin(string key, string username, string password) {
            var userpass = new UserPass(username, password);
            if (userpass.IsAnyEmpty) return false;
            logins.Add(key, userpass);
            return SaveLogins();
        }
        internal static bool RemoveLogin(string key) {
            if (logins[key].IsEmpty) return false;
            logins.Remove(key);
            return SaveLogins();
        }
        internal static bool UpdateLogin(string key, string password) {
            var username = logins[key].Username;
            logins.Remove(key);
            return AddLogin(key, username, password);
        }
        internal static bool LoginsContainsKey(string key) {
            return logins.ContainsKey(key);
        }
        internal static string[] GetLoginKeys() {
            return logins.Keys.Where(key => !logins[key].IsEmpty).ToArray();
        }

        private static Task<LCU> ConnectLcuOnRiotClientStart() {
            return Task.Run((Func<LCU>)(() => {
                //wait for oldest riot clients to be dead
                Thread.Sleep(2000);
                do {
                    Thread.Sleep(100);
                } while (Utils.GetRiotClientUxProcess() == null);
                //wait till riot client fully  started
                Thread.Sleep(250);
                return new LCU(LCU.Hook.riotClientUx);
            }));
        }

        private static async Task<LCU> LoginOnLcuConnect(string key) {
            var loginData = logins[key];
            var lcu = await ConnectLcuOnRiotClientStart();

            var trustPayload = "{\"clientId\":\"riot-client\",\"trustLevels\":[\"always_trusted\"]}";
            await lcu.Request(LCU.RequestMethod.POST, "/rso-auth/v2/authorizations", trustPayload);
            var loginPayload = "{\"username\":\"" + loginData.Username + "\",\"password\":\"" + loginData.Password + "\",\"persistLogin\":false}";
            await lcu.Request(LCU.RequestMethod.PUT, "/rso-auth/v1/session/credentials", loginPayload);

            return lcu;
        }

        internal static async Task<MainController> Launch() {
            if(Settings.current.MainAccountKey != "") {
                return await Launch(Settings.current.MainAccountKey);
            }
            var lcuTask = ConnectLcuOnRiotClientStart();
            return await launch(lcuTask);
        }
        internal static async Task<MainController> Launch(string key) {
            var lcuTask = LoginOnLcuConnect(key);
            return await launch(lcuTask);
        }
        private static async Task<MainController> launch(Task<LCU> lcuTask) {
            // Step 0: Kill all current processes
            LcuHandler.RemoveInitStatus();
            Utils.KillClientProcesses();
            Thread.Sleep(2000);

            // Step 1: Open a port for our chat proxy, so we can patch chat port into clientconfig.
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            Trace.WriteLine($"Chat proxy listening on port {port}");

            // Step 2: Find the Riot Client.
            var riotClientPath = Utils.GetRiotClientPath();

            // Step 3: Start proxy web server for clientconfig
            var proxyServer = new ConfigProxy(port);

            // Step 4: Launch Riot Client (+game)
            var startArgs = new ProcessStartInfo { FileName = riotClientPath, Arguments = $"--client-config-url=\"http://127.0.0.1:{proxyServer.ConfigPort}\" --launch-product=\"league_of_legends\" --launch-patchline=live" };
            var riotClient = Process.Start(startArgs);
            // Kill Deceive when Riot Client has exited, so no ghost Deceive exists.
            if (riotClient is not null) {
                Utils.ListenToRiotClientExit(riotClient);
            }

            // Step 5: Get chat server and port for this player by listening to event from ConfigProxy.
            string? chatHost = null;
            var chatPort = 0;
            proxyServer.PatchedChatServer += (_, args) => {
                chatHost = args.ChatHost;
                chatPort = args.ChatPort;
                Trace.WriteLine($"The original chat server details were {chatHost}:{chatPort}");
            };

            Trace.WriteLine("Waiting for client to connect to chat server...");
            var incoming = await listener.AcceptTcpClientAsync();
            Trace.WriteLine("Client connected!");

            // Step 6: Connect sockets.
            var sslIncoming = new SslStream(incoming.GetStream());
            var cert = new X509Certificate2(LeagueBuddy.Properties.Resources.Certificate);

            await sslIncoming.AuthenticateAsServerAsync(cert);

            if (chatHost is null) {
                Console.WriteLine("Unable to find Riot Chatserver");
                return null;
            }

            var outgoing = new TcpClient(chatHost, chatPort);
            var sslOutgoing = new SslStream(outgoing.GetStream());
            await sslOutgoing.AuthenticateAsClientAsync(chatHost);

            var mainController = new MainController(lcuTask.Result);
            mainController.StartThreads(sslIncoming, sslOutgoing);
            mainController.ConnectionErrored += async (_, _) => {
                Trace.WriteLine("Trying to reconnect.");
                sslIncoming.Close();
                sslOutgoing.Close();
                incoming.Close();
                outgoing.Close();

                incoming = await listener.AcceptTcpClientAsync();
                sslIncoming = new SslStream(incoming.GetStream());
                await sslIncoming.AuthenticateAsServerAsync(cert);
                while (true)
                    try {
                        outgoing = new TcpClient(chatHost, chatPort);
                        break;
                    } catch (SocketException e) {
                        Trace.WriteLine(e);
                        Console.WriteLine("Unable to reconnect to the chat server.");
                    }
                sslOutgoing = new SslStream(outgoing.GetStream());
                await sslOutgoing.AuthenticateAsClientAsync(chatHost);
                mainController.StartThreads(sslIncoming, sslOutgoing);
            };
            Application.Run(mainController);
            return null;
        }
    }
}

