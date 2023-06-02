using NLog;
using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.World;
using IdiotPlugin.Util;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Torch;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.ObjectBuilders;

namespace IdiotPlugin.VoteRewards
{

    /*
        URL https://space-engineers.com/api/?object=votes&element=claim&key={ServerKey}&steamid={SteamID}
        URL https://space-engineers.com/api/?object=votes&element=claim&key={ServerKey}&username={Username}

        Parameter	    Description	                Data Type	    Required
        key	            Your Server API Key	        String	        Yes
        steamid	        64bit SteamID of the user	Integer     	Yes
        username	    Username of the voter	    String      	Yes

        Response	Description
            0	    Not found
            1	    Has voted and not claimed
            2	    Has voted and claimed
     */


    /*
        URL https://space-engineers.com/api/?action=post&object=votes&element=claim&key={ServerKey}&steamid={SteamID}
        URL https://space-engineers.com/api/?action=post&object=votes&element=claim&key={ServerKey}&username={Username}

        Parameter   	Description	                Data Type	    Required
        key	            Your Server API Key	        String	        Yes
        steamid	        64bit SteamID of the user	Integer	        Yes
        username	    Username of the voter	    String 	        Yes

        Response	Description
            0	    Vote has not been claimed
            1	    Vote has been claimed
     */

    public static class Vote
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Config cfg { get { return IdiotPlugin.Config; } }
        private static readonly HttpClient client = new HttpClient();
        private static readonly List<string> ServerKey = cfg.VoteKeys;

        public static void Init()
        {
            if (cfg.SQLEnabled)
            {
                string initQ = "USE " + cfg.SQLDBName + ";"
                +
                "CREATE TABLE IF NOT EXISTS VoteRewards(" +
                "SID varchar(255) NOT NULL," +
                " CurrentTokens int," +
                " LifetimeTokens int," +

                " PRIMARY KEY(SID)" + ");";
                if (cfg.RewardTicket) SQL.Write(initQ, true);
            }
            if (cfg.Rewards.Count < 1)
            {
                RewardItem item = new RewardItem();
                cfg.Rewards.Add(item);
            }
            if (cfg.VoteLinks.Count < 1)
            {
                cfg.VoteLinks.Add("https://space-engineers.com/server/");
            }
            if (cfg.VoteKeys.Count < 1)
            {
                cfg.VoteKeys.Add("Vote reward key here");
            }

        }
        public static async Task<int> Get(ulong SID, int server)
        {
            try
            {
                string response = await client.GetStringAsync($"https://space-engineers.com/api/?action=post&object=votes&element=claim&key={ServerKey[server]}&steamid={SID}");
                if (response.Length > 1) return -1;
                return Convert.ToInt32(response);
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
                return -1;
            }

        }
        public static async Task<int> Set(ulong SID, int server)
        {
            try
            {
                string response = await client.GetStringAsync($"https://space-engineers.com/api/?action=post&object=votes&element=claim&key={ServerKey[server]}&steamid={SID}");
                if (response.Length > 1) return -1;
                return Convert.ToInt32(response);
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
                return -1;
            }
}
    }
    public static class Reward
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Config cfg { get { return IdiotPlugin.Config; } }
        internal static List<RewardItem> Rewards = cfg.Rewards;
        internal static List<RewardItem> ConstItems = Rewards.FindAll(r => r.Always == true);
        internal static List<RewardItem> RandItems = Rewards.FindAll(r => r.Always == false);
        internal static readonly Random rnd = new Random();
        public static void ReloadConfig()
        {
            Rewards = cfg.Rewards;
            ConstItems = Rewards.FindAll(r => r.Always == true);
            RandItems = Rewards.FindAll(r => r.Always == false);
            Log.Warn("Rewards config reloaded.");
        }
        public static async void CheckServerVotes(ulong SID)
        {
            for (int i = 0; i < cfg.VoteKeys.Count; i++)
            {
                await CheckVote(SID, i);
            }
        }
        public static async Task CheckVote(ulong SID, int server)
        {
            int GR = await Vote.Get(SID, server);
            string chatmessage;
            switch (GR)
            {
                case 0:
                    chatmessage = $"server {server + 1} vote not found!";
                    //not voted
                    break;
                case 1:
                    //give reward token
                    ModifyBalance(SID, "+1", true);
                    _ = await Vote.Set(SID, server);
                    chatmessage = "You gained 1 vote token!";
                    break;
                case 2:
                    chatmessage = $"server {server + 1} vote Not found.";
                    //already claimed 
                    break;
                default:
                    chatmessage = "Invalid Vote Config please inform the staff.";
                    //invalid vote config;
                    break;
            }
            MyVisualScriptLogicProvider.SendChatMessageColored(chatmessage, VRageMath.Color.Green, "Idiot Rewards System", Utils.GetIdentityByNameOrId(SID.ToString()).IdentityId, "Blue");
        }
        internal static int GetBalance(ulong SID)
        {
            if (!cfg.RewardTicket) return -1;
            try
            {
                return SQL.GetInt(@"SELECT VoteRewards.CurrentTokens
                                    FROM VoteRewards
                                    WHERE VoteRewards.SID=" + SID, true).Result;
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return -1;
            }
        }
        internal static int GetLifetime(ulong SID)
        {
            if (!cfg.RewardTicket) return -1;
            try
            {
                return SQL.GetInt(@"SELECT VoteRewards.LifetimeTokens
                                    FROM VoteRewards
                                    WHERE VoteRewards.SID=" + SID, true).Result;
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return -1;
            }
        }
        public static void Create(ulong steamId)
        {
            string query = "INSERT IGNORE INTO VoteRewards " +
                "VALUES (" +
                steamId + ",0, 0" +
                "); ";
            SQL.Write(query, true);
        }
        public static void ModifyBalance(ulong SID, string edit, bool vote = false)
        {
            if (!cfg.RewardTicket) return;
            try
            {
                SQL.Write(@$"UPDATE VoteRewards
                                    SET VoteRewards.CurrentTokens=VoteRewards.CurrentTokens{edit}
                                    WHERE VoteRewards.SID={SID}", true);
                if (vote)
                {
                    SQL.Write(@$"UPDATE VoteRewards
                                    SET VoteRewards.LifetimeTokens=VoteRewards.LifetimeTokens{edit}
                                    WHERE VoteRewards.SID={SID}", true);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
            }
        }

        internal static async void Give(ulong SID, int tokens = 1)
        {
            if (tokens > 60) tokens = 60;
            if (tokens <= 0) tokens = 1;
            if (cfg.RewardTicket)
            {
                int bal = GetBalance(SID);
                if (bal < tokens)
                {
                    MyVisualScriptLogicProvider.SendChatMessageColored("Not Enough Vote Tokens.", VRageMath.Color.Green, "Idiot Rewards System", Utils.GetIdentityByNameOrId(SID.ToString()).IdentityId, "Blue");
                    return;
                }
            }

            int TokenDiv5 = Math.DivRem(tokens, 5, out _);

            double TokenModifier = TokenDiv5 != 0 ? tokens + Math.Log(Math.Pow(tokens, 2 * TokenDiv5)) : tokens;

            double weightMod = cfg.MinVoteRandomWeight * TokenModifier;

            double weight = 0 - weightMod;

            Dictionary<MyDefinitionId, int> RRL = new Dictionary<MyDefinitionId, int>();

            Task FillConstant(RewardItem i)
            {
                try
                {
                    MyDefinitionId ID = MyDefinitionId.Parse($"MyObjectBuilder_{i.ItemType}/{i.ItemSubType}");
                    int amt = rnd.Next(i.Min, i.Max) * (int)TokenModifier;
                    if (!RRL.ContainsKey(ID))
                    {
                        RRL.Add(ID, amt);
                    } 
                    else
                    {
                        RRL[ID] += amt;
                    }
                    
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    Log.Warn(e.Message);
                    return Task.FromException(e);
                }
            }
            await ConstItems.ForEachAsync(FillConstant);
            //fill reward list
            try
            {
                while (weight <= 0)
                {
                    RewardItem Ri = RandItems[rnd.Next(RandItems.Count)];
                    int amt = rnd.Next(Ri.Min, Ri.Max);
                    MyDefinitionId ID = MyDefinitionId.Parse($"MyObjectBuilder_{Ri.ItemType}/{Ri.ItemSubType}");
                    if (!RRL.ContainsKey(ID))
                    {
                        RRL.Add(ID, amt);
                    }
                    else
                    {
                        RRL[ID] += amt;
                    }
                    weight += Ri.Weight * amt;
                }
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
            }
            MyIdentity p = Utils.GetIdentityByNameOrId(SID.ToString());

            Utils.SpawnLootBag(p.IdentityId, RRL);

            if (cfg.RewardTicket)
            {
                ModifyBalance(SID, "-" + tokens);
            }
            MyVisualScriptLogicProvider.SendChatMessageColored($"Rewards claimed for {tokens} tokens. Total reward value {(int)TokenModifier} tokens", VRageMath.Color.Green, "Idiot Rewards System", Utils.GetIdentityByNameOrId(SID.ToString()).IdentityId, "Blue");

        }
    }
}
