using Torch;
using ProtoBuf;
using System.Collections.Generic;
using IdiotPlugin.VoteRewards;
using IdiotPlugin.Announcements;

namespace IdiotPlugin
{
    public class Config : ViewModel
    {
        [ProtoMember(1)]
        private string _FolderDirectory;
        public string FolderDirectory { get => _FolderDirectory; set => SetValue(ref _FolderDirectory, value); }

        private string _DLFolderDirectory;
        public string DLFolderDirectory { get => _DLFolderDirectory; set => SetValue(ref _DLFolderDirectory, value); }

        //Restart Stuff

        private bool _RestartManEnabled = true;
        public bool RestartManEnabled { get => _RestartManEnabled; set => SetValue(ref _RestartManEnabled, value); }

        private int _RestartInterval = 4;
        public int RestartInterval { get => _RestartInterval; set => SetValue(ref _RestartInterval, value); }

        private int _MaxBootTime = 2;
        public int MaxBootTime { get => _MaxBootTime; set => SetValue(ref _MaxBootTime, value); }

        private int _RestartOffset = 0;
        public int RestartOffset { get => _RestartOffset; set => SetValue(ref _RestartOffset, value); }
        
        private bool _StopOnNextRestart = false;
        public bool StopOnNextRestart { get => _StopOnNextRestart; set => SetValue(ref _StopOnNextRestart, value); }

        private bool _IsLobby = false;
        public bool IsLobby { get => _IsLobby; set => SetValue(ref _IsLobby, value); }

        private bool _HangTime = false;
        public bool HangTime { get => _HangTime; set => SetValue(ref _HangTime, value); }

        private int _HangTimeValue = 0;
        public int HangTimeValue { get => _HangTimeValue; set => SetValue(ref _HangTimeValue, value); }

        private bool _ChangeSession = false;
        public bool ChangeSession { get => _ChangeSession; set => SetValue(ref _ChangeSession, value); }

        private List<string> _Sessions = new List<string>();
        public List<string> Sessions { get => _Sessions; set => SetValue(ref _Sessions, value); }

        //DeathStuffs

        private bool _DeathCountEnabled = true;
        public bool DeathCountEnabled { get => _DeathCountEnabled; set => SetValue(ref _DeathCountEnabled, value); }

        private bool _DeathFeedEnabled = true;
        public bool DeathFeedEnabled { get => _DeathFeedEnabled; set => SetValue(ref _DeathFeedEnabled, value); }

        private bool _DeathCostEnabled = true;
        public bool DeathCostEnabled { get => _DeathCostEnabled; set => SetValue(ref _DeathCostEnabled, value); }

        private double _DeathCostPrecentage = 5.0;
        public double DeathCostPercentage { get => _DeathCostPrecentage; set => SetValue(ref _DeathCostPrecentage, value); }

        //SQL stuffs

        private bool _SQLEnabled = true;
        public bool SQLEnabled { get => _SQLEnabled; set => SetValue(ref _SQLEnabled, value); }

        private string _SQLAddress = "";
        public string SQLAddress { get => _SQLAddress; set => SetValue(ref _SQLAddress, value); }

        private string _SQLPort = "";
        public string SQLPort { get => _SQLPort; set => SetValue(ref _SQLPort, value); }

        private string _SQLUN = "";
        public string SQLUN { get => _SQLUN; set => SetValue(ref _SQLUN, value); }

        private string _SQLPW = "";
        public string SQLPW { get => _SQLPW; set => SetValue(ref _SQLPW, value); }

        private string _SQLDBName = "";
        public string SQLDBName { get => _SQLDBName; set => SetValue(ref _SQLDBName, value); }

        private string _SQLKDRName = "";
        public string SQLKDRName { get => _SQLKDRName; set => SetValue(ref _SQLKDRName, value); }
        
        //VoteStuffs

        private List<string> _VoteLinks = new List<string>();
        public List<string> VoteLinks { get => _VoteLinks; set => SetValue(ref _VoteLinks, value); }

        private List<string> _VoteKeys = new List<string>();
        public List<string> VoteKeys { get => _VoteKeys; set => SetValue(ref _VoteKeys, value); }

        private bool _RewardTicket = true;
        public bool RewardTicket { get => _RewardTicket; set => SetValue(ref _RewardTicket, value); }

        private int _MinVoteRandomWeight = 1;
        public int MinVoteRandomWeight { get => _MinVoteRandomWeight; set => SetValue(ref _MinVoteRandomWeight, value); }

        private List<RewardItem> _Rewards = new List<RewardItem>();
        public List<RewardItem> Rewards { get => _Rewards; set => SetValue(ref _Rewards, value); }

        //AnnMan
        private bool _AnnouncementsEnabled = true;
        public bool AnnouncementsEnabled { get => _AnnouncementsEnabled; set => SetValue(ref _AnnouncementsEnabled, value); }

        private List<Announcement> _Announcements = new List<Announcement>();
        public List<Announcement> Announcements { get => _Announcements; set => SetValue(ref _Announcements, value); }

    }
}
