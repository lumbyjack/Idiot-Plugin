using System;
using NLog;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.World;
using VRage.Game.ModAPI;

namespace IdiotPlugin.Util
{
    public static class Economy
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static long GetPlayerBallance(long PID)
        {
            try
            {
                return MyBankingSystem.GetBalance(PID);
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return 0L;
            }
        }
        public static long GetPlayerWealth(long PID)
        {
            long value = 0L;
            value += MyBankingSystem.GetBalance(PID) < 0 ? 0 : MyBankingSystem.GetBalance(PID);
            IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(PID);
            if (faction == null) return value;
            if(faction.TryGetBalanceInfo(out long facValue))
            {
                value += facValue;
            }
            return value;
        }
        public static bool ModifyPlayerBalance(long PID, long amount)
        {
            if (amount < 0L && GetPlayerBallance(PID) < Math.Abs(amount)) return false;
            bool s = MyBankingSystem.ChangeBalance(PID, amount);
            Utils.SendColourChatMessage($"Balance modified by: {amount}", amount < 0 ? VRageMath.Color.Red : VRageMath.Color.Green, "Idiot Bank Manager", PID);
            return s;
        }
        public static bool ModifyPlayerBalanceForced(long PID, long amount)
        {
            bool s = false;
            long bal = GetPlayerBallance(PID);
            if(MyBankingSystem.Static.RemoveAccount(PID))
            {
                MyBankingSystem.Static.CreateAccount(PID, bal + amount);
                s = true;
                Utils.SendColourChatMessage($"Balance modified by: {amount}", amount < 0 ? VRageMath.Color.Red : VRageMath.Color.Green, "Idiot Bank Manager", PID);
            }
            return s;
        }
    }
}
