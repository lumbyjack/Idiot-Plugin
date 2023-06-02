using IdiotPlugin.Util;
using System.Collections.Generic;
using System.Timers;
using System.Xml.Serialization;
using VRageMath;

namespace IdiotPlugin.Announcements
{
    [XmlRoot("Announcement")]
    public class Announcement
    {
        private readonly Timer aTimer = new Timer();

        [XmlElement("Interval")]
        public double Interval = 10.0;
        [XmlElement("Enabled")]
        public bool Enabled = true;

        [XmlArray("Messages")]
        [XmlArrayItem("Text")]
        public string[] aText;

        private int counter = 0;
        public void SetupTimer()
        {
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = Interval * 60000;
            aTimer.Enabled = Enabled;
            aTimer.AutoReset = true;
            aTimer.Start();
        }
        public void SetInterval(int interval)
        {
            aTimer.Stop();
            aTimer.Interval = (double)interval;
            aTimer.Start();
        }
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (counter > aText.Length - 1) counter = 0;
            Utils.SendColourChatMessage( aText[counter], Color.DeepPink, "Idiot Announcements", 0L);
            counter++;
        }
    }
}
