using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBuddy.League.DTOs
{

    public class EndOfGameDTO
    {
        public int basePoints { get; set; }
        public int battleBoostIpEarned { get; set; }
        public int boostIpEarned { get; set; }
        public int boostXpEarned { get; set; }
        public bool causedEarlySurrender { get; set; }
        public int currentLevel { get; set; }
        public string difficulty { get; set; }
        public bool earlySurrenderAccomplice { get; set; }
        public int experienceEarned { get; set; }
        public int experienceTotal { get; set; }
        public int firstWinBonus { get; set; }
        public bool gameEndedInEarlySurrender { get; set; }
        public long gameId { get; set; }
        public int gameLength { get; set; }
        public string gameMode { get; set; }
        public string[] gameMutators { get; set; }
        public string gameType { get; set; }
        public int globalBoostXpEarned { get; set; }
        public bool invalid { get; set; }
        public int ipEarned { get; set; }
        public int ipTotal { get; set; }
        public bool leveledUp { get; set; }
        public Localplayer localPlayer { get; set; }
        public int loyaltyBoostXpEarned { get; set; }
        public int missionsXpEarned { get; set; }
        public string multiUserChatId { get; set; }
        public string multiUserChatJWT { get; set; }
        public string multiUserChatPassword { get; set; }
        public string myTeamStatus { get; set; }
        public object[] newSpells { get; set; }
        public int nextLevelXp { get; set; }
        public int preLevelUpExperienceTotal { get; set; }
        public int preLevelUpNextLevelXp { get; set; }
        public int previousLevel { get; set; }
        public int previousXpTotal { get; set; }
        public string queueType { get; set; }
        public bool ranked { get; set; }
        public long reportGameId { get; set; }
        public Rerolldata rerollData { get; set; }
        public string roomName { get; set; }
        public string roomPassword { get; set; }
        public int rpEarned { get; set; }
        public Teamboost teamBoost { get; set; }
        public bool teamEarlySurrendered { get; set; }
        public Team[] teams { get; set; }
        public int timeUntilNextFirstWinBonus { get; set; }
        public int xbgpBoostXpEarned { get; set; }
    }

    public class Localplayer
    {
        public bool botPlayer { get; set; }
        public int championId { get; set; }
        public string championName { get; set; }
        public string championSquarePortraitPath { get; set; }
        public string detectedTeamPosition { get; set; }
        public long gameId { get; set; }
        public bool isLocalPlayer { get; set; }
        public int[] items { get; set; }
        public bool leaver { get; set; }
        public int leaves { get; set; }
        public int level { get; set; }
        public int losses { get; set; }
        public int profileIconId { get; set; }
        public string puuid { get; set; }
        public string selectedPosition { get; set; }
        public object[] skinEmblemPaths { get; set; }
        public string skinSplashPath { get; set; }
        public string skinTilePath { get; set; }
        public int spell1Id { get; set; }
        public int spell2Id { get; set; }
        public Stats stats { get; set; }
        public long summonerId { get; set; }
        public string summonerName { get; set; }
        public int teamId { get; set; }
        public int wins { get; set; }
    }

    public class Stats
    {
        public int ASSISTS { get; set; }
        public int BARRACKS_KILLED { get; set; }
        public int CHAMPIONS_KILLED { get; set; }
        public int GAME_ENDED_IN_EARLY_SURRENDER { get; set; }
        public int GAME_ENDED_IN_SURRENDER { get; set; }
        public int GOLD_EARNED { get; set; }
        public int LARGEST_CRITICAL_STRIKE { get; set; }
        public int LARGEST_KILLING_SPREE { get; set; }
        public int LARGEST_MULTI_KILL { get; set; }
        public int LEVEL { get; set; }
        public int LOSE { get; set; }
        public int MAGIC_DAMAGE_DEALT_PLAYER { get; set; }
        public int MAGIC_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int MAGIC_DAMAGE_TAKEN { get; set; }
        public int MINIONS_KILLED { get; set; }
        public int NEUTRAL_MINIONS_KILLED { get; set; }
        public int NEUTRAL_MINIONS_KILLED_ENEMY_JUNGLE { get; set; }
        public int NEUTRAL_MINIONS_KILLED_YOUR_JUNGLE { get; set; }
        public int NUM_DEATHS { get; set; }
        public int PERK0 { get; set; }
        public int PERK0_VAR1 { get; set; }
        public int PERK0_VAR2 { get; set; }
        public int PERK0_VAR3 { get; set; }
        public int PERK1 { get; set; }
        public int PERK1_VAR1 { get; set; }
        public int PERK1_VAR2 { get; set; }
        public int PERK1_VAR3 { get; set; }
        public int PERK2 { get; set; }
        public int PERK2_VAR1 { get; set; }
        public int PERK2_VAR2 { get; set; }
        public int PERK2_VAR3 { get; set; }
        public int PERK3 { get; set; }
        public int PERK3_VAR1 { get; set; }
        public int PERK3_VAR2 { get; set; }
        public int PERK3_VAR3 { get; set; }
        public int PERK4 { get; set; }
        public int PERK4_VAR1 { get; set; }
        public int PERK4_VAR2 { get; set; }
        public int PERK4_VAR3 { get; set; }
        public int PERK5 { get; set; }
        public int PERK5_VAR1 { get; set; }
        public int PERK5_VAR2 { get; set; }
        public int PERK5_VAR3 { get; set; }
        public int PERK_PRIMARY_STYLE { get; set; }
        public int PERK_SUB_STYLE { get; set; }
        public int PHYSICAL_DAMAGE_DEALT_PLAYER { get; set; }
        public int PHYSICAL_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int PHYSICAL_DAMAGE_TAKEN { get; set; }
        public int SIGHT_WARDS_BOUGHT_IN_GAME { get; set; }
        public int SPELL1_CAST { get; set; }
        public int SPELL2_CAST { get; set; }
        public int TEAM_EARLY_SURRENDERED { get; set; }
        public int TEAM_OBJECTIVE { get; set; }
        public int TIME_CCING_OTHERS { get; set; }
        public int TOTAL_DAMAGE_DEALT { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_BUILDINGS { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_OBJECTIVES { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_TURRETS { get; set; }
        public int TOTAL_DAMAGE_SELF_MITIGATED { get; set; }
        public int TOTAL_DAMAGE_SHIELDED_ON_TEAMMATES { get; set; }
        public int TOTAL_DAMAGE_TAKEN { get; set; }
        public int TOTAL_HEAL { get; set; }
        public int TOTAL_HEAL_ON_TEAMMATES { get; set; }
        public int TOTAL_TIME_CROWD_CONTROL_DEALT { get; set; }
        public int TOTAL_TIME_SPENT_DEAD { get; set; }
        public int TRUE_DAMAGE_DEALT_PLAYER { get; set; }
        public int TRUE_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int TRUE_DAMAGE_TAKEN { get; set; }
        public int TURRETS_KILLED { get; set; }
        public int VISION_SCORE { get; set; }
        public int VISION_WARDS_BOUGHT_IN_GAME { get; set; }
        public int WARD_KILLED { get; set; }
        public int WARD_PLACED { get; set; }
        public int WAS_AFK { get; set; }
    }

    public class Rerolldata
    {
        public int pointChangeFromChampionsOwned { get; set; }
        public int pointChangeFromGameplay { get; set; }
        public int pointsUntilNextReroll { get; set; }
        public int pointsUsed { get; set; }
        public int previousPoints { get; set; }
        public int rerollCount { get; set; }
        public int totalPoints { get; set; }
    }

    public class Teamboost
    {
        public object[] availableSkins { get; set; }
        public int ipReward { get; set; }
        public int ipRewardForPurchaser { get; set; }
        public int price { get; set; }
        public string skinUnlockMode { get; set; }
        public string summonerName { get; set; }
        public bool unlocked { get; set; }
    }

    public class Team
    {
        public string fullId { get; set; }
        public bool isBottomTeam { get; set; }
        public bool isPlayerTeam { get; set; }
        public bool isWinningTeam { get; set; }
        public string memberStatusString { get; set; }
        public string name { get; set; }
        public Player[] players { get; set; }
        public Stats1 stats { get; set; }
        public string tag { get; set; }
        public int teamId { get; set; }
    }

    public class Stats1
    {
        public int ASSISTS { get; set; }
        public int BARRACKS_KILLED { get; set; }
        public int CHAMPIONS_KILLED { get; set; }
        public int GAME_ENDED_IN_EARLY_SURRENDER { get; set; }
        public int GAME_ENDED_IN_SURRENDER { get; set; }
        public int GOLD_EARNED { get; set; }
        public int LARGEST_CRITICAL_STRIKE { get; set; }
        public int LARGEST_KILLING_SPREE { get; set; }
        public int LARGEST_MULTI_KILL { get; set; }
        public int LEVEL { get; set; }
        public int LOSE { get; set; }
        public int MAGIC_DAMAGE_DEALT_PLAYER { get; set; }
        public int MAGIC_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int MAGIC_DAMAGE_TAKEN { get; set; }
        public int MINIONS_KILLED { get; set; }
        public int NEUTRAL_MINIONS_KILLED { get; set; }
        public int NEUTRAL_MINIONS_KILLED_ENEMY_JUNGLE { get; set; }
        public int NEUTRAL_MINIONS_KILLED_YOUR_JUNGLE { get; set; }
        public int NUM_DEATHS { get; set; }
        public int PERK0 { get; set; }
        public int PERK0_VAR1 { get; set; }
        public int PERK0_VAR2 { get; set; }
        public int PERK0_VAR3 { get; set; }
        public int PERK1 { get; set; }
        public int PERK1_VAR1 { get; set; }
        public int PERK1_VAR2 { get; set; }
        public int PERK1_VAR3 { get; set; }
        public int PERK2 { get; set; }
        public int PERK2_VAR1 { get; set; }
        public int PERK2_VAR2 { get; set; }
        public int PERK2_VAR3 { get; set; }
        public int PERK3 { get; set; }
        public int PERK3_VAR1 { get; set; }
        public int PERK3_VAR2 { get; set; }
        public int PERK3_VAR3 { get; set; }
        public int PERK4 { get; set; }
        public int PERK4_VAR1 { get; set; }
        public int PERK4_VAR2 { get; set; }
        public int PERK4_VAR3 { get; set; }
        public int PERK5 { get; set; }
        public int PERK5_VAR1 { get; set; }
        public int PERK5_VAR2 { get; set; }
        public int PERK5_VAR3 { get; set; }
        public int PERK_PRIMARY_STYLE { get; set; }
        public int PERK_SUB_STYLE { get; set; }
        public int PHYSICAL_DAMAGE_DEALT_PLAYER { get; set; }
        public int PHYSICAL_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int PHYSICAL_DAMAGE_TAKEN { get; set; }
        public int SIGHT_WARDS_BOUGHT_IN_GAME { get; set; }
        public int SPELL1_CAST { get; set; }
        public int SPELL2_CAST { get; set; }
        public int TEAM_EARLY_SURRENDERED { get; set; }
        public int TEAM_OBJECTIVE { get; set; }
        public int TIME_CCING_OTHERS { get; set; }
        public int TOTAL_DAMAGE_DEALT { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_BUILDINGS { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_OBJECTIVES { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_TURRETS { get; set; }
        public int TOTAL_DAMAGE_SELF_MITIGATED { get; set; }
        public int TOTAL_DAMAGE_SHIELDED_ON_TEAMMATES { get; set; }
        public int TOTAL_DAMAGE_TAKEN { get; set; }
        public int TOTAL_HEAL { get; set; }
        public int TOTAL_HEAL_ON_TEAMMATES { get; set; }
        public int TOTAL_TIME_CROWD_CONTROL_DEALT { get; set; }
        public int TOTAL_TIME_SPENT_DEAD { get; set; }
        public int TRUE_DAMAGE_DEALT_PLAYER { get; set; }
        public int TRUE_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int TRUE_DAMAGE_TAKEN { get; set; }
        public int TURRETS_KILLED { get; set; }
        public int VISION_SCORE { get; set; }
        public int VISION_WARDS_BOUGHT_IN_GAME { get; set; }
        public int WARD_KILLED { get; set; }
        public int WARD_PLACED { get; set; }
        public int WAS_AFK { get; set; }
        public int WIN { get; set; }
    }

    public class Player
    {
        public bool botPlayer { get; set; }
        public int championId { get; set; }
        public string championName { get; set; }
        public string championSquarePortraitPath { get; set; }
        public string detectedTeamPosition { get; set; }
        public long gameId { get; set; }
        public bool isLocalPlayer { get; set; }
        public int[] items { get; set; }
        public bool leaver { get; set; }
        public int leaves { get; set; }
        public int level { get; set; }
        public int losses { get; set; }
        public int profileIconId { get; set; }
        public string puuid { get; set; }
        public string selectedPosition { get; set; }
        public object[] skinEmblemPaths { get; set; }
        public string skinSplashPath { get; set; }
        public string skinTilePath { get; set; }
        public int spell1Id { get; set; }
        public int spell2Id { get; set; }
        public Stats2 stats { get; set; }
        public long summonerId { get; set; }
        public string summonerName { get; set; }
        public int teamId { get; set; }
        public int wins { get; set; }
    }

    public class Stats2
    {
        public int ASSISTS { get; set; }
        public int BARRACKS_KILLED { get; set; }
        public int CHAMPIONS_KILLED { get; set; }
        public int GAME_ENDED_IN_EARLY_SURRENDER { get; set; }
        public int GAME_ENDED_IN_SURRENDER { get; set; }
        public int GOLD_EARNED { get; set; }
        public int LARGEST_CRITICAL_STRIKE { get; set; }
        public int LARGEST_KILLING_SPREE { get; set; }
        public int LARGEST_MULTI_KILL { get; set; }
        public int LEVEL { get; set; }
        public int LOSE { get; set; }
        public int MAGIC_DAMAGE_DEALT_PLAYER { get; set; }
        public int MAGIC_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int MAGIC_DAMAGE_TAKEN { get; set; }
        public int MINIONS_KILLED { get; set; }
        public int NEUTRAL_MINIONS_KILLED { get; set; }
        public int NEUTRAL_MINIONS_KILLED_ENEMY_JUNGLE { get; set; }
        public int NEUTRAL_MINIONS_KILLED_YOUR_JUNGLE { get; set; }
        public int NUM_DEATHS { get; set; }
        public int PERK0 { get; set; }
        public int PERK0_VAR1 { get; set; }
        public int PERK0_VAR2 { get; set; }
        public int PERK0_VAR3 { get; set; }
        public int PERK1 { get; set; }
        public int PERK1_VAR1 { get; set; }
        public int PERK1_VAR2 { get; set; }
        public int PERK1_VAR3 { get; set; }
        public int PERK2 { get; set; }
        public int PERK2_VAR1 { get; set; }
        public int PERK2_VAR2 { get; set; }
        public int PERK2_VAR3 { get; set; }
        public int PERK3 { get; set; }
        public int PERK3_VAR1 { get; set; }
        public int PERK3_VAR2 { get; set; }
        public int PERK3_VAR3 { get; set; }
        public int PERK4 { get; set; }
        public int PERK4_VAR1 { get; set; }
        public int PERK4_VAR2 { get; set; }
        public int PERK4_VAR3 { get; set; }
        public int PERK5 { get; set; }
        public int PERK5_VAR1 { get; set; }
        public int PERK5_VAR2 { get; set; }
        public int PERK5_VAR3 { get; set; }
        public int PERK_PRIMARY_STYLE { get; set; }
        public int PERK_SUB_STYLE { get; set; }
        public int PHYSICAL_DAMAGE_DEALT_PLAYER { get; set; }
        public int PHYSICAL_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int PHYSICAL_DAMAGE_TAKEN { get; set; }
        public int SIGHT_WARDS_BOUGHT_IN_GAME { get; set; }
        public int SPELL1_CAST { get; set; }
        public int SPELL2_CAST { get; set; }
        public int TEAM_EARLY_SURRENDERED { get; set; }
        public int TEAM_OBJECTIVE { get; set; }
        public int TIME_CCING_OTHERS { get; set; }
        public int TOTAL_DAMAGE_DEALT { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_BUILDINGS { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_OBJECTIVES { get; set; }
        public int TOTAL_DAMAGE_DEALT_TO_TURRETS { get; set; }
        public int TOTAL_DAMAGE_SELF_MITIGATED { get; set; }
        public int TOTAL_DAMAGE_SHIELDED_ON_TEAMMATES { get; set; }
        public int TOTAL_DAMAGE_TAKEN { get; set; }
        public int TOTAL_HEAL { get; set; }
        public int TOTAL_HEAL_ON_TEAMMATES { get; set; }
        public int TOTAL_TIME_CROWD_CONTROL_DEALT { get; set; }
        public int TOTAL_TIME_SPENT_DEAD { get; set; }
        public int TRUE_DAMAGE_DEALT_PLAYER { get; set; }
        public int TRUE_DAMAGE_DEALT_TO_CHAMPIONS { get; set; }
        public int TRUE_DAMAGE_TAKEN { get; set; }
        public int TURRETS_KILLED { get; set; }
        public int VISION_SCORE { get; set; }
        public int VISION_WARDS_BOUGHT_IN_GAME { get; set; }
        public int WARD_KILLED { get; set; }
        public int WARD_PLACED { get; set; }
        public int WAS_AFK { get; set; }
        public int WIN { get; set; }
    }

}
