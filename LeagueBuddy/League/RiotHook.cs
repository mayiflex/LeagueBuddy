using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy.League
{
    internal struct RiotHook
    {
        public LCU.Hook Type { get; init; }
        public Process Process { get; set; }
        public string Port { get; init; }
        public string AuthToken { get; set; }
        public RiotHook(LCU.Hook type, Process process, string port, string authToken)
        {
            Type = type;
            Process = process;
            Port = port;
            AuthToken = authToken;
        }
    }
}
