using NLog;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Controls;

using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;

using IdiotPlugin.DeathLogger;
using IdiotPlugin.Util;
using IdiotPlugin.CheatMan;
using Sandbox.ModAPI;
using Sandbox.Engine.Multiplayer;
using System.Threading.Tasks;
using IdiotPlugin.Announcements;
using System.Xml.Linq;

namespace IdiotPlugin
{
    public class IdiotPlugin : TorchPluginBase, IWpfPlugin
    {
        public static bool _init;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static Config Config => _config?.Data;
        private static Persistent<Config> _config;
        public static DeathLog DeathLog => _DeathLog?.Data;
        private static Persistent<DeathLog> _DeathLog;
        public static CheatLog CheatLog => _CheatLog?.Data;
        private static Persistent<CheatLog> _CheatLog;
        public static SerializableDictionary<ulong, PlayerDeathLog> ActiveDeathLog => _ActiveDeathLog;
        private static readonly SerializableDictionary<ulong,PlayerDeathLog> _ActiveDeathLog = new SerializableDictionary<ulong, PlayerDeathLog>();
        public static SerializableDictionary<ulong, PlayerKillLog> ActiveKillLog => _ActiveKillLog;
        private static readonly SerializableDictionary<ulong, PlayerKillLog> _ActiveKillLog = new SerializableDictionary<ulong, PlayerKillLog>();
        private static int count = 0;

        private static RestartMan rMan;
        private static AnnMan annMan;
        private readonly Stopwatch timer = new Stopwatch();
        private readonly Stopwatch BootTime = new Stopwatch();
        private static UI _control;
        //private readonly List<NopeMan> Nm = new List<NopeMan>();
        //private readonly List<Nope> N = new List<Nope>();
        public UserControl GetControl() => _control ??= new UI(this);

        //private PatchManager _pm;
        //private PatchContext _context;
        public TorchSessionManager TorchSession { get; private set; }



        public override void Init(ITorchBase torch)
        {

            //BootTime.Start();
            SetupConfig();
            setupBootTimer();  
            SetupDeathLog();
            SetupCheatLog();
            base.Init(torch);
            TorchSessionManager manager = Torch.Managers.GetManager<TorchSessionManager>();
            if (manager != null)
            {
                manager.SessionStateChanged += SessionChanged;
            }
            else
            {
                Log.Warn("No session manager loaded!");
            }
            rMan = new RestartMan();
            annMan = new AnnMan();
            CheatManager Cheatmanager = new CheatManager(torch);
            torch.Managers.AddManager(Cheatmanager);
            rMan.UpdateNextSchRestart();
            if (Config.SQLEnabled) SQL.Init();
            VoteRewards.Vote.Init();
            _init = true;
        }
        public void setupBootTimer()
        {
            Task<int> t = BootTImer();
            t.ContinueWith(_ =>
            {
                Log.Info($"Boot Timer {t.Result}");
            }, TaskScheduler.Default);
        }
        public async Task<int> BootTImer()
        {
            while (BootTime.IsRunning) 
            {
               /* if (BootTime.Elapsed > TimeSpan.FromMinutes(Config.MaxBootTime) && base.Torch.CurrentSession.State != TorchSessionState.Loaded)
                {
                    Log.Fatal("Boot time limit exceeded.");
                }*/
                await Task.Delay(1000);

            }
            return (int)BootTime.Elapsed.TotalSeconds;

        }
        public void ReloadConfig()
        {
            string path = Path.Combine(StoragePath, "IdiotPlugin.cfg");

            _config = Persistent<Config>.Load(path);

            if (Config.FolderDirectory == null || Config.FolderDirectory == "")
            {
                Config.FolderDirectory = Path.Combine(StoragePath, "IdiotPlugin");
            }
            VoteRewards.Reward.ReloadConfig();
            SQL.ReloadConfig();
            saveConfig();
        }
        public override void Update()
        {
            try
            {


                if (!timer.IsRunning || base.Torch.CurrentSession == null || base.Torch.CurrentSession.State != TorchSessionState.Loaded)
                {
                    return;
                }

                double time = 1000;
                if (count == 30){
                    SaveDeathLog();
                    count = 0;
                }
                if (time != 0 && timer.Elapsed.TotalMilliseconds >= time)
                {
                    if (Config.RestartManEnabled) rMan.CheckRestart(this);
                    timer.Restart();
                    count++;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "It shat a brick.");
            }
        }
        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loading:
                    HangTime();
                    GetSessions();
                    break;
                case TorchSessionState.Loaded:
                    BootTime.Stop();
                    Log.Info($"Boot time: {BootTime.Elapsed:hh\\:mm\\:ss}");
                    timer.Start();
                    Communication.RegisterHandlers();
                    DeathPatch.Init();
                    //MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(10401, dNSync.OnMessageReceived);
                    //MyMultiplayer.Static.ClientLeft += dNSync.DoSendSync;
                    break;
                case TorchSessionState.Unloading:
                    SaveDeathLog();
                    timer.Stop();
                   // MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(10401, dNSync.OnMessageReceived);
                   // if (MyMultiplayer.Static != null)
                   // {
                   //     MyMultiplayer.Static.ClientLeft -= dNSync.DoSendSync;
                   // }
                    break;
                case TorchSessionState.Unloaded:
                    ChangeSession();
                    break;
            }
        }

        public void GetSessions()
        {
            Log.Warn("populating Session List.");
            string path = Path.Combine(StoragePath, "Saves");
            Config.Sessions.Clear();
            Config.Sessions.AddRange(Directory.GetDirectories(path));
            saveConfig();
        }
        private void ChangeSession()
        {
            if (!Config.ChangeSession) return;
            Log.Warn("Changing Session.");
            string path = Path.Combine(StoragePath, "Saves\\LastSession.sbl");
            var LS = XDocument.Load(path);
            string Previous = LS.Root.Element("Path").Value;
            int index = 0;
            index += Config.Sessions.FindIndex(s => s.Contains(Previous)) +1;
            if (index > Config.Sessions.Count -1) index = 0;
            Log.Warn("session changed to " + Config.Sessions[index].ToString() + " index: " + index);
            LS.Root.Element("Path").Value = Config.Sessions[index].ToString();
            LS.Save(path);
            return;
        }

        private void HangTime()
        {
            if (!Config.HangTime) return;
            Log.Warn("HangTime Activated.");
            Stopwatch T = new Stopwatch();
            T.Start();
            while (T.IsRunning)
            {
                if (T.Elapsed.TotalMinutes > Config.HangTimeValue)
                {
                    T.Stop();
                }
            }
            Log.Warn("HangTime Compleete.");
        }

        private void SetupConfig()
        {
            string path = Path.Combine(StoragePath, "IdiotPlugin.cfg");

            _config = Persistent<Config>.Load(path);

            if (Config.FolderDirectory == null || Config.FolderDirectory == "")
            {
                Config.FolderDirectory = Path.Combine(StoragePath, "IdiotPlugin");
            }
            //Config.SkipNextRestart = false;
            Config.StopOnNextRestart = false;
            saveConfig();
            
        }
        public void saveConfig()
        {
            _config.Save();
        }

        
        private void SetupDeathLog()
        {
            if (Config.DLFolderDirectory == null || Config.DLFolderDirectory == "")
            {
                Config.DLFolderDirectory = Path.Combine(StoragePath);
            }
            try
            {
                string path = Path.Combine(Config.DLFolderDirectory, "IdiotDeathLog.cfg");
                _DeathLog = Persistent<DeathLog>.Load(path);
            } catch (DirectoryNotFoundException e)
            {
                Log.Error(e.Message);
                Config.DLFolderDirectory = Path.Combine(StoragePath);
                string path = Path.Combine(Config.DLFolderDirectory, "IdiotDeathLog.cfg");
                _DeathLog = Persistent<DeathLog>.Load(path);
            }
            saveConfig();

            foreach (var s in DeathLog.DeathCounter)
            {
                var _pdl = new PlayerDeathLog(); 
                _pdl.FromString(s.Value);
                _ActiveDeathLog.Add(s.Key, _pdl);
            }
            Log.Warn($"Loaded {_ActiveDeathLog.Count} Death Logs");
        }
        public static void SaveDeathLog()
        {
            if (Config.SQLEnabled) return;

            try
            {
                _DeathLog = Persistent<DeathLog>.Load(Path.Combine(Config.DLFolderDirectory, "IdiotDeathLog.cfg"));
                PlayerDeathLog pdl = new PlayerDeathLog();
                foreach (var s in _ActiveDeathLog)
                {
                    if (!DeathLog.DeathCounter.ContainsKey(s.Key))
                    {
                        DeathLog.DeathCounter.Add(s.Key, s.Value.ToString());
                    }
                    else
                    {
                        pdl.FromString(DeathLog.DeathCounter[s.Key]);
                        pdl.SetDeaths(s.Value.GetDeaths());
                        DeathLog.DeathCounter[s.Key] = pdl.ToString();
                        pdl.Clear();
                    }
                }
                PlayerKillLog pkl = new PlayerKillLog();
                foreach (var s in _ActiveKillLog)
                {
                    if (!DeathLog.KillCounter.ContainsKey(s.Key))
                    {
                        DeathLog.KillCounter.Add(s.Key, s.Value.ToString());
                    }
                    else
                    {
                        pkl.FromString(DeathLog.KillCounter[s.Key]);
                        pkl.SetKills(s.Value.GetKills());
                        DeathLog.KillCounter[s.Key] = pkl.ToString();
                        pdl.Clear();
                    }
                }
                _DeathLog.Save();
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }

        }
        private void SetupCheatLog()
        {
            string path = Path.Combine(StoragePath, "IdiotCheatLog.cfg");

            _CheatLog = Persistent<CheatLog>.Load(path);

            if (Config.FolderDirectory == null || Config.FolderDirectory == "")
            {
                Config.FolderDirectory = Path.Combine(StoragePath, "IdiotCheatLog");
            }
            Log.Warn($"Loaded Cheat Logs");
            SaveCheatLog();
        }

        public static void SaveCheatLog()
        {
            _CheatLog.Save();
        }

        public RestartMan getRMan()
        {
            return rMan;
        }
        public AnnMan GetAnnMan()
        {
            return annMan;
        }
        public override void Dispose()
        {
            base.Dispose();
            Communication.UnregisterHandlers();
        }
        
      
    }
}
