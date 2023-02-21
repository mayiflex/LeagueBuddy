using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy {
    internal record UserPass(string Username, string Password) {
        [JsonIgnore]
        public bool IsEmpty { get => Username == "" && Password == ""; }
        [JsonIgnore]
        public bool IsAnyEmpty { get => Username == "" || Password == ""; }
    }
}
