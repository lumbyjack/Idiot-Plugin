using NLog;
using Sandbox.Game;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using Torch;
using Torch.API.Session;
using VRageMath;
using IdiotPlugin.Util;
using Torch.API;

namespace IdiotPlugin
{
     public class RestartMan
    {
            private int Interval;
            private int Offset;
            public static readonly Logger Log = LogManager.GetCurrentClassLogger();
            private readonly DateTime today = DateTime.Today;
            private static readonly List<DateTime> sch = new List<DateTime>();
            private static Config cfg { get { return IdiotPlugin.Config; } }
            public static bool JumpDisableReached;
            private DateTime lastMessage = DateTime.MinValue;
            private static readonly TimeSpan MessageSpan = TimeSpan.FromSeconds(1);
            private DateTime NextSchRestart;
            internal bool saveComplete = false;
            internal bool save = false;
            internal bool SendMessage = false;
            internal bool restart = false;
            internal bool restartSent = false;
            public RestartMan()
            {
                Interval = cfg.RestartInterval;
                Offset = cfg.RestartOffset;
                JumpDisableReached = false;
                this.MakeSch(); 
            }

            public bool IsTimeForRestart(int secondsFrom = 0 )
            {
                if ((DateTime.Now + TimeSpan.FromSeconds(secondsFrom)) >= NextRestart())
                {
                    return true;
                }
                return false;
            }

            public void UpdateSch()
            {
                this.MakeSch();
            }
            public void UpdateNextSchRestart()
            {
                this.NextSchRestart = NextRestart();
            }
            public void UpdateInterval(int interval)
            {
                Interval = interval;
                this.MakeSch();
                UpdateNextSchRestart();
                var n = TimeSpanTillNextRestart();
                string s = n.ToString("hh\\:mm\\:ss");
                Utils.SendColourChatMessage("Restart Modified Next restart in: " + s, Color.Yellow, "Idiot Restart Manager", 0L);
            }
            public void UpdateOffset(int offset)
            {
                Offset = offset;

                this.MakeSch();
                UpdateNextSchRestart();
                var n = TimeSpanTillNextRestart();
                string s = n.ToString("hh\\:mm\\:ss");
                Utils.SendColourChatMessage("Restart Modified Next restart in: " + s, Color.Yellow, "Idiot Restart Manager", 0L);
            }
            public DateTime NextRestart()
            {
                foreach(var r in sch)
                {
                    if (r < (DateTime.Now + TimeSpan.FromSeconds(10))) continue;
                    return r;
                }
                return DateTime.MinValue;
            }

            public bool SkipRestart()
            {
                DateTime NR = NextRestart();
                sch.Remove(NR);
                UpdateNextSchRestart();
                var n = TimeSpanTillNextRestart();
                string s = n.ToString("hh\\:mm\\:ss");
                Utils.SendColourChatMessage("Restart Skipped Next restart in: " + s, Color.Yellow, "Idiot Restart Manager", 0L);
                return true;
            }
            public List<DateTime> GetSch()
            {
                return sch;
            }

            private void MakeSch()
            {
                sch.Clear();
                int i = 48 / Interval;
                TimeSpan mod = cfg.IsLobby ? TimeSpan.FromMinutes(10) : TimeSpan.Zero;
                for (int q = 1; q <= i; q++)
                {
                    sch.Add(today + TimeSpan.FromHours((Interval * q ) + Offset) - mod);
                }
            }

            internal int TimeTillNextRestart()
            {
                TimeSpan t = NextSchRestart - DateTime.Now;
                return (int)t.TotalSeconds;
            }
            internal TimeSpan TimeSpanTillNextRestart()
            {
                return NextSchRestart - DateTime.Now;
            }

            public bool CheckUptime(TorchPluginBase b)
            {
                if (((ITorchServer)b.Torch).ElapsedPlayTime.Hours < (Interval / 2))
                {
                    SkipRestart();
                    Utils.SendColourChatMessage("Restart skipped automaticaly due to low uptime.", Color.DarkOrange, "Idiot Restart Manager", 0L);
                    return true;
                }
                return false;
            }

           public void CheckRestart(TorchPluginBase b)
           {

                switch (TimeTillNextRestart())
                {
                    case int e when e <= 1:
                        restart = true;
                        SendMessage = true;
                        break;
                    case 5:
                        SendMessage = true;
                        break;
                    case 10:
                        SendMessage = true;
                        break;
                    case 15:
                        SendMessage = true;
                        break;
                    case 30:
                        SendMessage = true;
                        break;
                    case 60:
                        SendMessage = true;
                        break;
                    case 300:
                        SendMessage = true;
                        break;
                    case 600:
                        SendMessage = true;
                        break;
                    case 1800:
                        SendMessage = true;
                        break;
                }

                if (SendMessage && (lastMessage == DateTime.MinValue || (lastMessage + MessageSpan) < DateTime.Now))
                {
                    if (CheckUptime(b))
                    {
                        SendMessage = false;
                        return;
                    }

                    lastMessage = DateTime.Now;
                    string message = "Server will restart in: " + TimeSpanTillNextRestart().ToString("hh\\:mm\\:ss");
                    Utils.SendColourChatMessage(message, Color.Plum, "Idiot Restart Manager", 0L);
                    Log.Info(message);
                    if (save)
                    {
                        Log.Warn("Saving Game 5s before restart.");
                        saveComplete = b.Torch.Save(-1,true).Result.Equals(GameSaveResult.Success);
                        if (saveComplete)
                        {
                            Utils.SendColourChatMessage("\n •'¯¨•.¸¸ GAME STATE SAVED ¸¸.•¨¯`•", Color.Yellow, "Idiot Restart Manager", 0L);
                        } else
                        {
                            Utils.SendColourChatMessage("\n ## SAVE FAILED ##", Color.LightYellow, "Idiot Restart Manager", 0L);
                        }

                        save = !saveComplete;
                    }
                    if (restart && !restartSent)
                    {
                        if (cfg.StopOnNextRestart)
                        {
                            Log.Warn("Server Stopping.");
                            b.Torch.Stop();
                            return;
                        }
                        Log.Warn("Server restarting");
                        b.Torch.Restart(true);
                        restart = false;
                        restartSent = true;
                    }
                    SendMessage = false;
                }
           }
    }
}