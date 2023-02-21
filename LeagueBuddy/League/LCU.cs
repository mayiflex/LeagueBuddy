using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;
using WebSocketSharp;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LeagueBuddy.Main;
using System.Windows;
using System.Management;

namespace LeagueBuddy.League
{
    internal class OnWebsocketEventArgs : EventArgs
    {
        // URI    
        public string Path { get; set; }

        // Update create delete     
        public string Type { get; set; }

        // data :D
        public dynamic Data { get; set; }
    }

    internal class LCU
    {
        #region some_ENUMS
        public enum Hook
        {
            riotClientUx,
            leagueClientUx
        }
        public enum RequestMethod
        {
            GET, POST, PATCH, DELETE, PUT
        }
        #endregion
        #region important_variabls
        private HttpClient client;

        private Dictionary<string, List<Action<OnWebsocketEventArgs>>> Subscribers = new Dictionary<string, List<Action<OnWebsocketEventArgs>>>();

        private WebSocket socketConnection;

        private RiotHook? _hook;
        private RiotHook hook { get => _hook.Value; }
        private bool shouldGetNewHook;

        public bool IsConnected { get => _hook.HasValue; }

        public event Action OnConnected;

        public event Action OnDisconnected;

        public event Action<OnWebsocketEventArgs> OnWebsocketEvent;
        private static readonly Regex RIOT_AUTH_TOKEN_FROM_LEAGUE_REGEX = new Regex("--riotclient-auth-token=(.+?)[\\\"\\s]");
        private static readonly Regex RIOT_PORT__FROM_LEAGUE_REGEX = new Regex("--riotclient-app-port=(\\d+)");

        private static readonly Regex AUTH_TOKEN_REGEX = new Regex("--remoting-auth-token=(.+?)[\\\"\\s]");

        private static readonly Regex PORT_REGEX = new Regex("--app-port=(\\d+)");

        public Hook origin { get; init; }
        #endregion

        public LCU(Hook origin, bool shouldGetNewHook = true)
        {
            this.origin = origin;
            this.shouldGetNewHook = shouldGetNewHook;

            //we initialize the http client
            try
            {
                client = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                });
            }
            catch
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client = new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                });
            }
            //wait before we start initializing the connections 
            Task.Delay(1000).ContinueWith(e => TryConnectOrRetry()).GetAwaiter().GetResult();
        }

        private void TryConnect()
        {
            try
            {
                if (IsConnected && shouldGetNewHook) return;

                if (shouldGetNewHook)
                {
                    var newHook = GetLcuAuthPort();
                    if (newHook == null) return;
                    _hook = newHook.Value;
                }

                var byteArray = Encoding.ASCII.GetBytes("riot:" + hook.AuthToken);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                socketConnection = new WebSocket("wss://127.0.0.1:" + hook.Port + "/", "wamp");
                socketConnection.SetCredentials("riot", hook.AuthToken, true);

                socketConnection.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                socketConnection.SslConfiguration.ServerCertificateValidationCallback = (a, b, c, d) => true;
                socketConnection.OnMessage += HandleMessage;
                socketConnection.OnClose += HandleDisconnect;
                socketConnection.Connect();
                //socketConnection.Send($"[5, \"OnJsonApiEvent\"]")
                OnConnected?.Invoke();
                shouldGetNewHook = false;
            }
            catch (Exception e)
            {
                _hook = null;
            }
        }
        private void TryConnectOrRetry()
        {
            TryConnect();
            if (IsConnected) return;
            Task.Delay(2000).ContinueWith(a => TryConnectOrRetry());
        }


        public void UpdateRiotAuthWithLeagueOrigin(LCU leagueLCU)
        {
            var riotHookFromLeague = leagueLCU.GetRiotHookFromLeagueOrigin();
            if (riotHookFromLeague == null) return;
            _hook = riotHookFromLeague;
            TryConnectOrRetry();
        }

        private RiotHook? GetRiotHookFromLeagueOrigin()
        {
            if (origin == Hook.riotClientUx) return null;
            return GetHookFromProcess(hook.Process, RIOT_PORT__FROM_LEAGUE_REGEX, RIOT_AUTH_TOKEN_FROM_LEAGUE_REGEX);
        }
        private RiotHook? GetLcuAuthPort()
        {
            Func<Process?> GetProcessToAttach;
            if (origin == Hook.riotClientUx) GetProcessToAttach = new Func<Process?>(() => Utils.GetRiotClientUxProcess());
            else GetProcessToAttach = new Func<Process?>(() => Utils.GetLeagueClientUxProcess());

            return GetHookFromProcess(GetProcessToAttach(), PORT_REGEX, AUTH_TOKEN_REGEX);
        }
        private RiotHook? GetHookFromProcess(Process? process, Regex portRegex, Regex authRegex)
        {
            if (process == null) return null;
            using (var mos = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id.ToString()))
            using (var moc = mos.Get())
            {
                var commandLine = (string)moc.OfType<ManagementObject>().First()["CommandLine"];

                try
                {
                    var authToken = authRegex.Match(commandLine).Groups[1].Value;
                    var port = portRegex.Match(commandLine).Groups[1].Value;

                    return new RiotHook(origin, process, port, authToken);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Error while trying to get the status for LeagueClientUx: {e.ToString()}\n\n(CommandLine = {commandLine})");
                }
            }
        }
















        //the method to do requests based on parameters
        public Task<string> Request(RequestMethod method, string url, object body = null)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to LCU");
            string RequestMethod;

            switch (method)
            {
                case LCU.RequestMethod.GET:
                    RequestMethod = "GET";
                    body = null;
                    break;
                case LCU.RequestMethod.POST:
                    RequestMethod = "POST";
                    break;
                case LCU.RequestMethod.PATCH:
                    RequestMethod = "PATCH";
                    break;
                case LCU.RequestMethod.DELETE:
                    RequestMethod = "DELETE";
                    break;
                case LCU.RequestMethod.PUT:
                    RequestMethod = "PUT";
                    break;
                default:
                    RequestMethod = "post";
                    break;
            }
            // to give the user the ability to write the uri with or without the '/' in start
            if (url[0] != '/')
            {
                url = "/" + url;
            }

            return client.SendAsync(new HttpRequestMessage(new HttpMethod(RequestMethod), "https://127.0.0.1:" + hook.Port + url)
            {
                Content = body == null ? null : new StringContent(body.ToString(), Encoding.UTF8, "application/json")
            }).Result.Content.ReadAsStringAsync();
        }

        public async Task<dynamic> getStringJsoned(string url)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to LCU");

            var res = await client.GetAsync("https://127.0.0.1:" + hook.Port + url);
            var stringContent = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.NotFound) return null;
            return JsonConvert.DeserializeObject(stringContent);
        }

        public async void GetData(string url, Action<dynamic> handler)
        {
            OnWebsocketEvent += data =>
            {
                if (data.Path == url) handler(data.Data);
            };

            if (IsConnected)
            {
                handler(await getStringJsoned(url));
            }
            else
            {
                Action connectHandler = null;
                connectHandler = async () =>
                {
                    OnConnected -= connectHandler;
                    handler(await getStringJsoned(url));
                };

                OnConnected += connectHandler;
            }
        }

        public void ClearAllListeners()
        {
            OnWebsocketEvent = null;
        }

        public KeyValuePair<string, string>
            CreateAuthorizationHeader(ICredentials credentials)
        {
            NetworkCredential networkCredential =
                credentials.GetCredential(null, null);

            string userName = networkCredential.UserName;
            string userPassword = networkCredential.Password;

            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            return new KeyValuePair<string, string>("Authorization", "Basic " + authInfo);
        }

        private void HandleDisconnect(object sender, CloseEventArgs args)
        {
            //check if we are trying to swap account
            //if (Process.GetProcessesByName(hook.Process.ProcessName) == null) return;

            _hook = null;
            socketConnection = null;

            OnDisconnected?.Invoke();

            TryConnectOrRetry();
        }

        private void HandleMessage(object sender, MessageEventArgs args)
        {
            if (!args.IsText) return;
            var payload = JsonConvert.DeserializeObject<JArray>(args.Data);

            if (payload.Count != 3) return;
            if ((long)payload[0] != 8 || !((string)payload[1]).Equals("OnJsonApiEvent")) return;

            var ev = (dynamic)payload[2];
            OnWebsocketEvent?.Invoke(new OnWebsocketEventArgs()
            {
                Path = ev["uri"],
                Type = ev["eventType"],
                Data = ev["eventType"] == "Delete" ? null : ev["data"]
            });
            if (Subscribers.ContainsKey((string)ev["uri"]))
            {
                foreach (var item in Subscribers[(string)ev["uri"]])
                {
                    item(new OnWebsocketEventArgs()
                    {
                        Path = ev["uri"],
                        Type = ev["eventType"],
                        Data = ev["eventType"] == "Delete" ? null : ev["data"]
                    });
                }
            }
        }
        public void Subscribe(string URI, Action<OnWebsocketEventArgs> args)
        {
            if (!Subscribers.ContainsKey(URI))
            {
                Subscribers.Add(URI, new List<Action<OnWebsocketEventArgs>>() { args });
            }
            else
            {
                Subscribers[URI].Add(args);
            }
        }

        public void Unsubscribe(string URI, Action<OnWebsocketEventArgs> action)
        {
            if (Subscribers.ContainsKey(URI))
            {
                if (Subscribers[URI].Count == 1)
                {
                    Subscribers.Remove(URI);
                }
                else if (Subscribers[URI].Count > 1)
                {
                    foreach (var item in Subscribers[URI].ToArray())
                    {
                        if (item == action)
                        {
                            var index = Subscribers[URI].IndexOf(action);
                            Subscribers[URI].RemoveAt(index);

                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }
}

