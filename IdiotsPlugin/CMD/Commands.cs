using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using IdiotPlugin.DeathLogger;
using IdiotPlugin.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace IdiotPlugin
{
    [Category("Idiot")]
    public class Commands : CommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Idiot");
        private static SerializableDictionary<ulong, PlayerDeathLog> ADL => IdiotPlugin.ActiveDeathLog;
        private static SerializableDictionary<ulong, PlayerKillLog> AKL => IdiotPlugin.ActiveKillLog;
        public IdiotPlugin Plugin => (IdiotPlugin)Context.Plugin;
        private static Config cfg => IdiotPlugin.Config;

        [Command("nr", "Returns time untill next restart", null)]
        [Permission(MyPromoteLevel.None)]
        public void NextRestart()
        {
            var n = Plugin.getRMan().TimeSpanTillNextRestart();
            string s = n.ToString("hh\\:mm\\:ss");
            Context.Respond($"Next restart in: {s}" );
        }
        [Command("deaths", "Returns Number of deaths", null)]
        [Permission(MyPromoteLevel.None)]
        public void Deaths()
        {
            if (cfg.SQLEnabled)
            {
                SQLLog.SetName(Context.Player.DisplayName, Context.Player.SteamUserId);
                Context.Respond(SQLLog.GetAllDeaths(Context.Player.SteamUserId));
               return;
            }
            if (!ADL.TryGetValue(Context.Player.SteamUserId, out PlayerDeathLog deaths))
            {
                deaths = new PlayerDeathLog();
                ADL.Add(Context.Player.SteamUserId,deaths);
                IdiotPlugin.SaveDeathLog();
            }

            Context.Respond(deaths.GetAllDeaths());

        }
        [Command("kills", "Returns Number of kills.", null)]
        [Permission(MyPromoteLevel.None)]
        public void Kills()
        {
            if (cfg.SQLEnabled)
            {
                SQLLog.SetName(Context.Player.DisplayName, Context.Player.SteamUserId);
                Context.Respond(SQLLog.GetAllKills(Context.Player.SteamUserId));
                return;
            }
            if (!AKL.TryGetValue(Context.Player.SteamUserId, out PlayerKillLog kills))
            {
                kills = new PlayerKillLog();
                AKL.Add(Context.Player.SteamUserId, kills);
                IdiotPlugin.SaveDeathLog();
            }

            Context.Respond(kills.GetAllKills());

        }
        [Command("kdr", "Returns Number of deaths and kills.", null)]
        [Permission(MyPromoteLevel.None)]
        public void KDR()
        {
            int TD;
            int TK;
            double KDR;
            if (cfg.SQLEnabled)
            {
                TD = Convert.ToInt32(SQLLog.GetAllDeaths(Context.Player.SteamUserId));
                TK = Convert.ToInt32(SQLLog.GetAllKills(Context.Player.SteamUserId));
            }
            else
            {
                if (!ADL.TryGetValue(Context.Player.SteamUserId, out PlayerDeathLog deaths))
                {
                    deaths = new PlayerDeathLog();
                    ADL.Add(Context.Player.SteamUserId, deaths);
                    IdiotPlugin.SaveDeathLog();
                }
                if (!AKL.TryGetValue(Context.Player.SteamUserId, out PlayerKillLog kills))
                {
                    kills = new PlayerKillLog();
                    AKL.Add(Context.Player.SteamUserId, kills);
                    IdiotPlugin.SaveDeathLog();
                }
                TD = deaths.TotalDeaths;
                TK = kills.TotalKills;
            }

            if (TD == 0)
            {
                KDR = TK / 1;
                Context.Respond("KDR: " + KDR + " | Kills: " + TK + " Deaths: " + TD);
            } else
            {
                KDR = TK / TD;
                Context.Respond("KDR: " + KDR + " | Kills: " + TK + " Deaths: " + TD);
            }
            
        }
        [Command("vote", "Gives vote screen.", null)]
        [Permission(MyPromoteLevel.None)]
        public void vote()
        {
            foreach (string s in cfg.VoteLinks)
            {
                VoteRewards.Reward.Create(Context.Player.SteamUserId);
                MyVisualScriptLogicProvider.OpenSteamOverlay($"https://steamcommunity.com/linkfilter/?url={s}", Context.Player.IdentityId);
            }
            MyVisualScriptLogicProvider.SendChatMessageColored($"Please type !Idiot claim to claim your tokens.", VRageMath.Color.Green, "Idiot Rewards System", Context.Player.IdentityId, "Blue");
        }

        [Command("claim", "gets vote reward token.", null)]
        [Permission(MyPromoteLevel.None)]
        public void claim()
        {
            VoteRewards.Reward.Create(Context.Player.SteamUserId);
            VoteRewards.Reward.CheckServerVotes(Context.Player.SteamUserId);
            MyVisualScriptLogicProvider.SendChatMessageColored($"Please type !Idiot reward <Number of tokens> to claim your reward.", VRageMath.Color.Green, "Idiot Rewards System", Context.Player.IdentityId, "Blue");
        }

        [Command("reward", "spends vot reward tokens.", null)]
        [Permission(MyPromoteLevel.None)]
        public void reward(int args = 1)
        {
            VoteRewards.Reward.Create(Context.Player.SteamUserId);
            VoteRewards.Reward.Give(Context.Player.SteamUserId, args);
        }

        [Command("tokens", "Gives total usable tokens.", null)]
        [Permission(MyPromoteLevel.None)]
        public void tokens()
        {
            VoteRewards.Reward.Create(Context.Player.SteamUserId);
            MyVisualScriptLogicProvider.SendChatMessageColored($"You have {VoteRewards.Reward.GetBalance(Context.Player.SteamUserId)} tokens.", VRageMath.Color.Green, "Idiot Rewards System", Context.Player.IdentityId, "Blue");
        }

        /* 
         * #==============================================#
         * |                                              |
         * |                Admin Commands                |
         * |                                              |
         * #==============================================#
         */

        [Command("reload", "Reloads Config", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void Reload()
        {
            Plugin.ReloadConfig();
            Context.Respond("Reload sleeps butt to face, Config reloaded from file. This can lead to errors, Torch restart is preferable.");
        }
        [Command("AddVoteToken", "Gives vote token.", null)]
        [Permission(MyPromoteLevel.Owner)]
        public void AddVoteToken(string var, int val = 1)
        {
            IMyPlayer player;
            if (ulong.TryParse(var, out ulong sid))
            {
                VoteRewards.Reward.Create(sid);
                player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.GetPlayerBySteamId(sid);
                VoteRewards.Reward.ModifyBalance(sid, "+" + val);
                Context.Respond($"Added {val} Vote token(s)");
                return;
            }
            else
            {
                player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.GetPlayerByName(var);
                if (player == null) return;
                VoteRewards.Reward.Create(player.SteamUserId);
                VoteRewards.Reward.ModifyBalance(player.SteamUserId, "+" + val);
                Context.Respond($"Added {val} Vote token(s)");
                return;
            }
        }

        [Command("clearscores", "This clears koth scores", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void ClearScores()
        {
            Communication.Message();
            this.Context.Respond("Idiot plugin reset koth scores");
        }
        [Command("SkipRestart", "This skips the next scheduled restart", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void SkipResart()
        {
            Plugin.getRMan().SkipRestart();
            var n = Plugin.getRMan().TimeSpanTillNextRestart();
            string s = n.ToString("hh\\:mm\\:ss");
            this.Context.Respond("Restart Skipped. Next restart at: " + s);
        }
        [Command("mnt", "Maintainance Mode ", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void mnt(string var = "Reload Sleeps Butt to Face")
        {
            this.Context.Respond("Maintainance Mode.");
            Context.Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf("Server shutting down: " + var);
            Task<bool> stop = Utils.InvokeAsync<bool>(() =>
            {
                Task.Delay(10000);
                Context.Torch.Stop();
                return true;
            });
        }
        [Command("wip", "locates a player", null)]
        [Permission(MyPromoteLevel.Admin)]
        public bool Wip(string var)
        {
            IMyPlayer player;
            if (ulong.TryParse(var,  out ulong sid))
            {
                player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.GetPlayerBySteamId(sid);
            } else
            {
                player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.GetPlayerByName(var);
            }
            if (player != null)
            {
                Context.Respond($"Player {player.DisplayName} at location GPS:{player.DisplayName}:{player.GetPosition().X}:{player.GetPosition().Y}:{player.GetPosition().Z}:#FFFF00D7:");
                return true;
            }
            return false;
        }
        [Command("stoponrestart", "Stops server on next restart instead or restarting.", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void StopOnRestart()
        {
            cfg.StopOnNextRestart = true;
            Plugin.saveConfig();
            this.Context.Respond("Server will stop on next restart.");
        }
        [Command("Slap", "Slaps player")]
        [Permission(MyPromoteLevel.Admin)]
        public bool Slap(string var)
        {
            Random rand = new Random();
            if (var != "")
            {
                IMyPlayer _player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.GetPlayerByName(var);
                if (_player == null)
                {
                    Context.Respond(var + " Not found.");
                    return false;
                }
                bool x = false;
                MyHitInfo? hitInfo = new MyHitInfo()
                {
                    ShapeKey = _player.Character.Components.Get< MyCharacterDetectorComponent >().ShapeKey,
                    Velocity = new Vector3D
                    {
                        X = rand.Next(0, 100),
                        Y = rand.Next(0, 100),
                        Z = rand.Next(0, 100)
                    },
                    Normal = new Vector3D
                    {
                        X = rand.Next(0, 100),
                        Y = rand.Next(0, 100),
                        Z = rand.Next(0, 100)
                    },
                    Position = new Vector3D
                    {
                        X = rand.Next(0, 100),
                        Y = rand.Next(0, 100),
                        Z = rand.Next(0, 100)
                    }

                };
                _player.Character.Physics.ApplyImpulse(
                    new Vector3D
                    {
                        X = rand.Next(0, 100),
                        Y = rand.Next(0, 100),
                        Z = rand.Next(0, 100)
                    },
                    new Vector3D
                    {
                        X = rand.Next(0, 100),
                        Y = rand.Next(0, 100),
                        Z = rand.Next(0, 100)
                    });

                _player.Character.DoDamage(5.1f, MyStringHash.GetOrCompute("Bullet"), x, hitInfo, 0L, 0L); // hurt
                Context.Respond("Slapped " + _player.DisplayName);
                return true;
            }
            return false;
        }
        [Command("repair", "repair a grid", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void Repair(string var)
        {
            if (long.TryParse(var, out long entID))
            {
                GridUtils.Repair(entID);
            }
            if (var != null)
            {
                using IEnumerator<MyCubeGrid> e = MyEntities.GetEntities().OfType<MyCubeGrid>().Where<MyCubeGrid>((Func<MyCubeGrid, bool>)(x => x.DisplayName == var)).GetEnumerator();
                while (e.MoveNext())
                {
                    GridUtils.Repair(e.Current.EntityId);
                }
            }
        }
        [Command("charge", "charge a grid", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void Charge(string var)
        {
            if (long.TryParse(var, out long entID))
            {
                GridUtils.RechargeBatteries(entID);
                GridUtils.RechargeJump(entID);
            }
            if (var != null)
            {
                using IEnumerator<MyCubeGrid> e = MyEntities.GetEntities().OfType<MyCubeGrid>().Where<MyCubeGrid>((Func<MyCubeGrid, bool>)(x => x.DisplayName == var)).GetEnumerator();
                while (e.MoveNext())
                {
                    GridUtils.RechargeBatteries(e.Current.EntityId);
                    GridUtils.RechargeJump(e.Current.EntityId);
                }
            }
        }
        [Command("filltanks", "fill a grid's tanks", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void FillTanks(string var)
        {
            if (long.TryParse(var, out long entID))
            {
                GridUtils.FillTank(entID);
            }
            if (var != null)
            {
                using IEnumerator<MyCubeGrid> e = MyEntities.GetEntities().OfType<MyCubeGrid>().Where<MyCubeGrid>((Func<MyCubeGrid, bool>)(x => x.DisplayName == var)).GetEnumerator();
                while (e.MoveNext())
                {
                    GridUtils.FillTank(e.Current.EntityId);
                }
            }
        }

        [Command("wig", "locates a grid", null)]
        [Permission(MyPromoteLevel.Admin)]
        public bool Wig(string var)
        {

            if (long.TryParse(var, out long entID))
            {
                using IEnumerator<MyCubeGrid> e = MyEntities.GetEntities().OfType<MyCubeGrid>().Where<MyCubeGrid>((Func<MyCubeGrid, bool>)(x => x.EntityId == entID)).GetEnumerator();
                while (((IEnumerator)e).MoveNext())
                {
                    Context.Respond($"Grid {e.Current.DisplayName} at location GPS:{e.Current.DisplayName}:{e.Current.WorldMatrix.Translation.X}:{e.Current.WorldMatrix.Translation.Y}:{e.Current.WorldMatrix.Translation.Z}:#FFFF00D7:");
                }
                return true;
            }
            if (var != null)
            {
                using IEnumerator<MyCubeGrid> e = MyEntities.GetEntities().OfType<MyCubeGrid>().Where<MyCubeGrid>((Func<MyCubeGrid, bool>)(x => x.DisplayName == var)).GetEnumerator();
                while (((IEnumerator)e).MoveNext())
                {
                    Context.Respond($"Grid {e.Current.DisplayName} at location GPS:{e.Current.DisplayName}:{e.Current.WorldMatrix.Translation.X}:{e.Current.WorldMatrix.Translation.Y}:{e.Current.WorldMatrix.Translation.Z}:#FFFF00D7:");
                }
                return true;
            }

            return false;
        }

        [Command("kickall", "Kicks all online players", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void KickAll()
        {
            var olp = Context.Torch.CurrentSession.KeenSession.Players;
            ICollection<MyPlayer> players = olp.GetOnlinePlayers();
            foreach (MyPlayer player in players)
            {

                if (player != null)
                {
                    Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.KickPlayer(player.Client.SteamUserId);
                    Context.Respond($"Player '{player.DisplayName}' kicked.");
                }
            }
            Context.Respond(players.Count + " Players removed");
        }

        [Command("reset gps", "Resets voxel damange in specified radius from given point")]
        [Permission(MyPromoteLevel.Admin)]
        public void ResetVoxelArea(float X, float Y, float Z, float Radius, int GridRadius = 500)
        {
            Vector3D ResetTarget = new Vector3D(X, Y, Z);
            if (Radius <= 0)
            {
                Log.Info("Invalid Radius Input!");
                return;
            }
            if (!StartVoxelThread(ResetTarget, Radius , GridRadius))
            {
                Context.Respond("Couldnt reset voxels! Check log for more information!");
                return;
            }
            Context.Respond("Voxel reset beginning!");
        }

        private static bool StartVoxelThread(Vector3D Center, float Radius, int GridRadius)
        {
            RegenThread.Center = Center;
            RegenThread.Radius = Radius;
            RegenThread.GridRadius = GridRadius;
            Thread thread = new Thread(RegenThread.DoAreaRegen);
            try
            {
                thread.Start();
                return true;
            } catch(Exception e)
            {
                Log.Error(e.ToString());
                return false;
            }
        }
       
        [Command("gridname", "This corrects subgrid names, to avoid accidental cleanup.", null)]
        [Permission(MyPromoteLevel.Admin)]
        public void GridName() //TODO: fix this bullshit
        {
            bool flag1 = true;
            Random random = new Random();
            Commands.Log.Info("Starting Grid Rename");
            try
            {
                while (flag1)
                {
                    flag1 = false;
                    bool flag2 = true;
                    while (flag2)
                    {
                        flag2 = false;
                        using IEnumerator<MyCubeGrid> enumerator1 = MyEntities.GetEntities().OfType<MyCubeGrid>().Where<MyCubeGrid>((Func<MyCubeGrid, bool>)(x => x.Projector == null)).GetEnumerator();
                        while (((IEnumerator)enumerator1).MoveNext())
                        {
                            using IEnumerator<MyFunctionalBlock> enumerator2 = ((IEnumerable)enumerator1.Current.GetFatBlocks()).OfType<MyFunctionalBlock>().GetEnumerator();
                            while (((IEnumerator)enumerator2).MoveNext())
                            {
                                MyFunctionalBlock current = enumerator2.Current;
                                string strB = ((MyDefinitionBase)((MyCubeBlock)current).BlockDefinition).Id.TypeId.ToString().Substring(16);
                                if (current != null)
                                {
                                    IMyCubeGrid cTop = (IMyCubeGrid)null;
                                    IMyCubeGrid cBase = (IMyCubeGrid)null;
                                    if (string.Compare("ExtendedPistonBase", strB, StringComparison.InvariantCultureIgnoreCase) == 0)
                                        this.GetGrids(current, "ExtendedPistonBase", ref cBase, ref cTop);
                                    if (string.Compare("MotorSuspension", strB, StringComparison.InvariantCultureIgnoreCase) == 0)
                                        this.GetGrids(current, "MotorSuspension", ref cBase, ref cTop);
                                    if (string.Compare("MotorStator", strB, StringComparison.InvariantCultureIgnoreCase) == 0)
                                        this.GetGrids(current, "MotorStator", ref cBase, ref cTop);
                                    if (string.Compare("MotorAdvancedStator", strB, StringComparison.InvariantCultureIgnoreCase) == 0)
                                        this.GetGrids(current, "MotorAdvancedStator", ref cBase, ref cTop);
                                    if (cTop != null && cBase != null && (this.RenameCheck(cTop, cBase) || this.RenameCheck(cBase, cTop)))
                                    {
                                        flag2 = true;
                                        flag1 = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Commands.Log.Warn("String data:");
                Commands.Log.Warn(ex.ToString());
                Commands.Log.Warn("Message:");
                Commands.Log.Warn(ex.Message);
                Commands.Log.Warn("Inner Exception:");
                Commands.Log.Warn<Exception>(ex.InnerException);
                Commands.Log.Warn("Stack Trace:");
                string stackTrace = ex.StackTrace;
                char[] chArray = new char[1] { '\n' };
                foreach (string str in stackTrace.Split(chArray))
                    Commands.Log.Warn(str);
            }
            Commands.Log.Info("Finished Grid Rename");
        }

        public void GetGrids(
          MyFunctionalBlock block,
          string sType,
          ref IMyCubeGrid cBase,
          ref IMyCubeGrid cTop)
        {
            switch (sType)
            {
                case "ExtendedPistonBase":
                    MyExtendedPistonBase extendedPistonBase = (MyExtendedPistonBase)block;
                    cTop = (IMyCubeGrid)((MyMechanicalConnectionBlockBase)extendedPistonBase).TopGrid;
                    cBase = (IMyCubeGrid)((MyCubeBlock)extendedPistonBase).CubeGrid;
                    break;
                case "MotorSuspension":
                    MyMotorSuspension myMotorSuspension = (MyMotorSuspension)block;
                    cTop = (IMyCubeGrid)((MyMechanicalConnectionBlockBase)myMotorSuspension).TopGrid;
                    cBase = (IMyCubeGrid)((MyCubeBlock)myMotorSuspension).CubeGrid;
                    break;
                case "MotorStator":
                    MyMotorStator myMotorStator = (MyMotorStator)block;
                    cTop = (IMyCubeGrid)((MyMechanicalConnectionBlockBase)myMotorStator).TopGrid;
                    cBase = (IMyCubeGrid)((MyCubeBlock)myMotorStator).CubeGrid;
                    break;
                case "MotorAdvancedStator":
                    MyMotorAdvancedStator motorAdvancedStator = (MyMotorAdvancedStator)block;
                    cTop = (IMyCubeGrid)((MyMechanicalConnectionBlockBase)motorAdvancedStator).TopGrid;
                    cBase = (IMyCubeGrid)((MyCubeBlock)motorAdvancedStator).CubeGrid;
                    break;
            }
        }

        public bool RenameCheck(IMyCubeGrid c1, IMyCubeGrid c2)
        {
            bool flag = false;
            Random random = new Random();
            if (c2.CustomName.Contains("Grid") && !c1.CustomName.Contains("Grid"))
            {
                flag = true;
                c2.CustomName = c1.CustomName.Length <= 9 || !(c1.CustomName.Substring(c1.CustomName.Length - 9, 5) == " Sub-") ? c1.CustomName + " Sub-" + random.Next(1000, 9999).ToString() : c1.CustomName.Substring(0, c1.CustomName.Length - 9) + " Sub-" + random.Next(1000, 9999).ToString();
            }
            return flag;
        }
    }
}