using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VRage.Game.ModAPI;


namespace IdiotPlugin.CheatMan
{
	public class GPSCheat
	{
		
		private bool hackyGPSDescript(IMyGps g)
		{
			if(g == null || g.Description == null || g.Description == "") return false;

			if (g.Description.Contains("GridType:"))
			{
				return true;
			}
			if (g.Description.Contains("HasShields:"))
			{
				return true;
			}
			if (g.Description.Contains("Number of Reactors:"))
			{
				return true;
			}
			if (g.Description.Contains("Number of Turrets:"))
			{
				return true;
			}
			if (g.Description.Contains("Number of Static Guns:"))
			{
				return true;
			}
			if (g.Description.Contains("Number of JumpDrives:"))
			{
				return true;
			}
			return false;
		}
		public bool hackyGPSName(IMyGps g)
        {
			if (g == null || g.Name == null || g.Name == "") return false;
			string pattern = @"\[\w\w\w\]";
			if (Regex.Match(g.Name, pattern).Success)
			{
				return true;
			}
			string pattern1 = @"\[\d+\]";
			if (Regex.Match(g.Name, pattern1).Success)
			{
				return true;
			}
			return false;
		}
		public void runGPS(CheatManager cm)
		{
			IMySession s = MyAPIGateway.Session;
			IMyPlayerCollection play = MyAPIGateway.Multiplayer.Players;
			List<IMyPlayer> myPlayerList = new List<IMyPlayer>();
			List<IMyNetworkClient> clientList = new List<IMyNetworkClient>();
			play.GetPlayers(myPlayerList);
			foreach (IMyPlayer pp in myPlayerList)
			{
				if (pp == null)
				{
					continue;
				}
				List<IMyGps> MyGpsCollection = MyAPIGateway.Session.GPS.GetGpsList(pp.IdentityId);
				foreach (IMyGps g in MyGpsCollection)
				{
					if(g == null)
                    {
						continue;
                    }
					if (hackyGPSName(g))
					{
						cm.LogCheat("PlayerName: " + pp.DisplayName + " steamID: " + pp.SteamUserId + " Flag: GPS Name" + " Evidence: " + g.Name);
					} 
                    if (hackyGPSDescript(g))
                    {
						cm.LogCheat("PlayerName: " + pp.DisplayName + " steamID: " + pp.SteamUserId + " Flag: GPS Desc" + " Evidence: " + g.Description);
					}
				}
			}
		}
	}
}
