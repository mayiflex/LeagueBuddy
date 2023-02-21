using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy.League.DTOs
{
    internal class ParticipantDTO
    {
        public object activePlatform { get; set; }
        public string cid { get; set; }
        public string game_name { get; set; }
        public string game_tag { get; set; }
        public bool muted { get; set; }
        public string name { get; set; }
        public string pid { get; set; }
        public string puuid { get; set; }
        public string region { get; set; }
    }
}
