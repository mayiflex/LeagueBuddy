using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy.League.DTOs
{
    internal class SummonerDTO
    {
        public string availability { get; set; }
        public int displayGroupId { get; set; }
        public string displayGroupName { get; set; }
        public string gameName { get; set; }
        public string gameTag { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int icon { get; set; }
        public string id { get; set; }
        public bool isP2PConversationMuted { get; set; }
        public object lastSeenOnlineTimestamp { get; set; }
        public Lol lol { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        public string patchline { get; set; }
        public string pid { get; set; }
        public string platformId { get; set; }
        public string product { get; set; }
        public string productName { get; set; }
        public string puuid { get; set; }
        public string statusMessage { get; set; }
        public string summary { get; set; }
        public long summonerId { get; set; }
        public long time { get; set; }
    }

    public class Lol
    {
        public string pty { get; set; }
        public string championId { get; set; }
        public string companionId { get; set; }
        public string damageSkinId { get; set; }
        public string gameQueueType { get; set; }
        public string gameStatus { get; set; }
        public string iconOverride { get; set; }
        public string level { get; set; }
        public string mapId { get; set; }
        public string mapSkinId { get; set; }
        public string masteryScore { get; set; }
        public string puuid { get; set; }
        public string rankedLeagueDivision { get; set; }
        public string rankedLeagueQueue { get; set; }
        public string rankedLeagueTier { get; set; }
        public string rankedLosses { get; set; }
        public string rankedSplitRewardLevel { get; set; }
        public string rankedWins { get; set; }
        public string regalia { get; set; }
        public string skinVariant { get; set; }
        public string skinname { get; set; }
    }
}
