using NLog;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.World;
using IdiotPlugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdiotPlugin.DeathLogger
{
    public static class DeathCost
    {
        private static Config Config => IdiotPlugin.Config;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static Task ChargeForNewClone(MyIdentity p)
        {
            long amount = - Convert.ToInt64(Economy.GetPlayerWealth(p.IdentityId) * (Config.DeathCostPercentage / 100));
            if(!Economy.ModifyPlayerBalance(p.IdentityId, amount))
            {
                Economy.ModifyPlayerBalanceForced(p.IdentityId, amount);
            }
            Log.Info($"Player: {p.DisplayName} has been charged {amount} for a new clone.");
            return Task.CompletedTask;
        }
    }
}
