using NLog;
using System;

namespace IdiotPlugin.Announcements
{
    public class AnnMan
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Config Config { get { return IdiotPlugin.Config; } }

        public AnnMan()
        {
            if (!Config.AnnouncementsEnabled) return;
            if(Config.Announcements.Count == 0)
            {
                AddAnn("Please Setup some Announcements or disable Announcements in your config.", 10);
            }
            static void Setup(Announcement a)
            {
                if (a.Enabled)
                {
                    a.SetupTimer();
                }
            }
            Config.Announcements.ForEach(Setup);
            Log.Warn($"Setup {Config.Announcements.Count} Announcements");
        }
        private void AddAnn(string message, int ineterval)
        {
            string[] m =
            {
                message
            };
            Announcement a = new Announcement { aText = m };
            a.SetInterval(ineterval);
            Config.Announcements.Add(a);
        }
    }
}
