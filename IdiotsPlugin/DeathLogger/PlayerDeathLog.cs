using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdiotPlugin.DeathLogger
{
    
    public enum DeathType
    {
        Colision,
        Bullet,
        O2,
        Voxel,
        Explosion,
        Tool,
        Suicide,
        Idiocy
    }
    //SID, PN, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    public static class SQLLog
    {
        private static Config Config { get { return IdiotPlugin.Config; } }
        public static void Create(ulong steamId)
        {
            string query = $"INSERT IGNORE INTO {Config.SQLKDRName} " +
                "VALUES (" +
                steamId + ",' ', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0" +
                "); ";
            Util.SQL.Write(query);
        }
        public static void SetName(string name, ulong steamId)
        {
            Create(steamId);
            string query = $"UPDATE {Config.SQLKDRName} SET " +
                " PlayerName='" + name +
                "' WHERE SID =" +
                steamId +
                "; ";

            Util.SQL.Write(query);
        }
        public static void AddValue(DeathType Type, ulong steamId, bool kill = false)
        {
            Create(steamId);
            string t = kill ? "TotalKills" : "TotalDeaths";
            string c = "";
            switch (Type)
            {
                case DeathType.Colision:
                    c = kill ? "CollisonKills" : "CollisonDeaths";
                    break;
                case DeathType.Bullet:
                    c = kill ? "BulletKills" : "BulletDeaths";
                    break;
                case DeathType.Tool:
                    c = kill ? "ToolKills" : "ToolDeaths";
                    break;
                case DeathType.O2:
                    c = "O2Deaths";
                    break;
                case DeathType.Voxel:
                    c = "VoxelDeaths";
                    break;
                case DeathType.Suicide:
                    c = "SuicideDeaths";
                    break;
                case DeathType.Explosion:
                    c = kill ? "ExplosionKills" : "ExplosionDeaths";
                    break;
                case DeathType.Idiocy:
                    c = kill ? "OtherKills" : "OtherDeaths";
                    break;
            }
            string query = $"UPDATE {Config.SQLKDRName} SET " +
                            t + "="+ t + "+ 1" + ", " +
                            c + "=" + c+ "+ 1" +
                            " WHERE SID=" +
                            steamId +
                            ";";

            Util.SQL.Write(query);
        }
        public static int GetTotalDeaths(ulong steamId)
        {
            Create(steamId);
            return Convert.ToInt32(Util.SQL.GetPlayerDataFromMySQL(steamId,Util.SQLDataType.Deaths).Result[0]);
        }
        public static int GetTotalKills(ulong steamId)
        {
            Create(steamId);
            return Convert.ToInt32(Util.SQL.GetPlayerDataFromMySQL(steamId, Util.SQLDataType.Kills).Result[0]);
        }
        public static string GetAllDeaths(ulong steamId)
        {
            Create(steamId);
            object[] result = Util.SQL.GetPlayerDataFromMySQL(steamId, Util.SQLDataType.Deaths).Result;
            string s =
              $"Total: {result[0]} \n" +
              $"Suicide Deaths: {result[5]} \n" +
              $"Bullet Deaths: {result[1]} \n" +
              $"Collison Deaths: {result[2]} \n" +
              $"Explosion Deaths: {result[3]} \n" +
              $"O2 Deaths: {result[4]} \n" +
              $"Tool Deaths: {result[7]} \n" +
              $"Voxel Deaths: {result[8]} \n" +
              $"Other Deaths: {result[6]} \n"
            ;
            return s;
        }
        public static string GetAllKills(ulong steamId)
        {
            Create(steamId);
            object[] result = Util.SQL.GetPlayerDataFromMySQL(steamId, Util.SQLDataType.Kills).Result;
            string s =
                $"Total: {result[0]} \n" +
                $"Bullet Kills: {result[5]} \n" +
                $"Collison Kills: {result[4]} \n" +
                $"Explosion Kills: {result[3]} \n" +
                $"Tool Kills: {result[1]} \n" +
                $"Other Kills: {result[2]} \n"
                ;
            return s;
        }
        public static int[] GetDeaths(ulong steamId)
        {
            Create(steamId);
            object[] result = Util.SQL.GetPlayerDataFromMySQL(steamId, Util.SQLDataType.Deaths).Result;
            int[] deaths = new int[result.Length];
            for(int i = 0; i < result.Length; i++)
            {
                deaths[i] = Convert.ToInt32(result[i]);
            }
            return deaths;
        }
        public static int[] GetKills(ulong steamId)
        {
            Create(steamId);
            object[] result = Util.SQL.GetPlayerDataFromMySQL(steamId, Util.SQLDataType.Kills).Result;
            int[] kills = new int[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                kills[i] = Convert.ToInt32(result[i]);
            }
            return kills;
        }
        public static void SetDeaths(int[] deaths, ulong steamId)
        {
           Create(steamId);
           string query = @$"UPDATE {Config.SQLKDRName} SET " +
                       " TotalDeaths=" + deaths[0] +
                       ", CollisonDeaths=" + deaths[1] +
                       ", BulletDeaths=" + deaths[2] +
                       ", ToolDeaths=" + deaths[3] +
                       ", O2Deaths=" + deaths[4] +
                       ", VoxelDeaths=" + deaths[5] +
                       ", SuicideDeaths=" + deaths[6] +
                       ", ExplosionDeaths=" + deaths[7] +
                       ", OtherDeaths=" + deaths[8] +
                       " WHERE SID =" +
                       steamId +
                       "; ";

            Util.SQL.Write(query);
        }
        public static void SetKills(int[] kills, ulong steamId)
        {
            Create(steamId);
            string query = @$"UPDATE {Config.SQLKDRName} SET " +
                            " TotalKills=" + kills[0] +
                            ", CollisonKills=" + kills[1] +
                            ", BulletKills=" + kills[2] +
                            ", ToolKills=" + kills[3] +
                            ", ExplosionKills=" + kills[4] +
                            ", OtherKills=" + kills[5] +
                            " WHERE SID =" +
                            steamId +
                            "; ";

            Util.SQL.Write(query);
        }
    }

    [Serializable]
    public class PlayerDeathLog 
    {
        private static readonly Logger Log = LogManager.GetLogger("Idiot");
        internal  int TotalDeaths ;
        
        internal  int CollisonDeaths ;
        internal  int BulletDeaths ;
        internal  int ToolDeaths ;
        internal  int O2Deaths ;
        internal  int VoxelDeaths ;
        internal  int SuicideDeaths ;
        internal  int ExplosionDeaths ;
        internal  int OtherDeaths ;

        public PlayerDeathLog()
        {
            this.TotalDeaths = 0;
            this.CollisonDeaths = 0;
            this.BulletDeaths = 0;
            this.ToolDeaths = 0;
            this.O2Deaths = 0;
            this.VoxelDeaths = 0;
            this.SuicideDeaths = 0;
            this.ExplosionDeaths = 0;
            this.OtherDeaths = 0;
        }

        public void Clear()
        {
            this.TotalDeaths = 0;
            this.CollisonDeaths = 0;
            this.BulletDeaths = 0;
            this.ToolDeaths = 0;
            this.O2Deaths = 0;
            this.VoxelDeaths = 0;
            this.SuicideDeaths = 0;
            this.ExplosionDeaths = 0;
            this.OtherDeaths = 0;
        }
        public void AddDeath(DeathType Type)
        {
            TotalDeaths++;
            switch(Type)
            {
                case DeathType.Colision:
                    CollisonDeaths++;
                    break;
                case DeathType.Bullet:
                    BulletDeaths++;
                    break;
                case DeathType.Tool:
                    ToolDeaths++;
                    break;
                case DeathType.O2:
                    O2Deaths++;
                    break;
                case DeathType.Voxel:
                    VoxelDeaths++;
                    break;
                case DeathType.Suicide:
                    SuicideDeaths++;
                    break;
                case DeathType.Explosion:
                    ExplosionDeaths++;
                    break;
                case DeathType.Idiocy:
                    OtherDeaths++;
                    break;
            }
        }
        public int GetTotalDeaths()
        {
            return TotalDeaths;
        }

        public string GetAllDeaths()
        {
            string s =
                $"Total: {this.TotalDeaths} \n" +
                $"Collison Deaths: {this.CollisonDeaths} \n" +
                $"Bullet Deaths: {this.BulletDeaths} \n" +
                $"Tool Deaths: {this.ToolDeaths} \n" +
                $"O2 Deaths: {this.O2Deaths} \n" +
                $"Voxel Deaths: {this.VoxelDeaths} \n" +
                $"Suicide Deaths: {this.SuicideDeaths} \n" +
                $"Explosion Deaths: {this.ExplosionDeaths} \n" +
                $"Other Deaths: {this.OtherDeaths} \n";
            return s;
        }
        public int[] GetDeaths()
        {
            int[] deaths = 
            {
            TotalDeaths,
            CollisonDeaths,
            BulletDeaths,
            ToolDeaths,
            O2Deaths,
            VoxelDeaths,
            SuicideDeaths,
            ExplosionDeaths,
            OtherDeaths
            };
            return deaths;
        }
        public void AddDeaths(int[] deaths)
        {
            this.TotalDeaths += deaths[0];
            this.CollisonDeaths += deaths[1];
            this.BulletDeaths += deaths[2];
            this.ToolDeaths += deaths[3];
            this.O2Deaths += deaths[4];
            this.VoxelDeaths += deaths[5];
            this.SuicideDeaths += deaths[6];
            this.ExplosionDeaths += deaths[7];
            this.OtherDeaths += deaths[8];
        }
        public void SetDeaths(int[] deaths)
        {
            this.TotalDeaths = deaths[0];
            this.CollisonDeaths = deaths[1];
            this.BulletDeaths = deaths[2];
            this.ToolDeaths = deaths[3];
            this.O2Deaths = deaths[4];
            this.VoxelDeaths = deaths[5];
            this.SuicideDeaths = deaths[6];
            this.ExplosionDeaths = deaths[7];
            this.OtherDeaths = deaths[8];
        }
        public override string ToString()
        {
            string s =
            $"{this.TotalDeaths}:" +
            $"{this.CollisonDeaths}:" +
            $"{this.BulletDeaths}:" +
            $"{this.ToolDeaths}:" +
            $"{this.O2Deaths}:" +
            $"{this.VoxelDeaths}:" +
            $"{this.SuicideDeaths}:" +
            $"{this.ExplosionDeaths}:" +
            $"{this.OtherDeaths}";
            return s;
        }
        public void FromString(string s)
        {
            string[] sl = s.Split(':');

            try
            {
                TotalDeaths = int.Parse(sl[0]);
                CollisonDeaths = int.Parse(sl[1]);
                BulletDeaths = int.Parse(sl[2]);
                ToolDeaths = int.Parse(sl[3]);
                O2Deaths = int.Parse(sl[4]);
                VoxelDeaths = int.Parse(sl[5]);
                SuicideDeaths = int.Parse(sl[6]);
                ExplosionDeaths = int.Parse(sl[7]);
                OtherDeaths = int.Parse(sl[8]);
            } catch (System.IndexOutOfRangeException e){
                Log.Error(e.Message + s);
            }

        }
    }
    [Serializable]
    public class PlayerKillLog
    {
        internal int TotalKills;

        internal int CollisonKills;
        internal int BulletKills;
        internal int ToolKills;
        internal int ExplosionKills;
        internal int OtherKills;

        public PlayerKillLog()
        {
            this.TotalKills = 0;
            this.CollisonKills = 0;
            this.BulletKills = 0;
            this.ToolKills = 0;
            this.ExplosionKills = 0;
            this.OtherKills = 0;
        }
        public int[] GetKills()
        {
            int[] kills =
            {
             TotalKills,
             CollisonKills,
             BulletKills,
             ToolKills,
             ExplosionKills,
             OtherKills
            };
            return kills;
        }
        public void AddKills(int[] kills)
        {
            this.TotalKills += kills[0];
            this.CollisonKills += kills[1];
            this.BulletKills += kills[2];
            this.ToolKills += kills[3];
            this.ExplosionKills += kills[4];
            this.OtherKills += kills[5];
        }
        public void SetKills(int[] kills)
        {
            this.TotalKills = kills[0];
            this.CollisonKills = kills[1];
            this.BulletKills = kills[2];
            this.ToolKills = kills[3];
            this.ExplosionKills = kills[4];
            this.OtherKills = kills[5];
        }
        public void AddKill(DeathType Type)
        {
            TotalKills++;
            switch (Type)
            {
                case DeathType.Colision:
                    CollisonKills++;
                    break;
                case DeathType.Bullet:
                    BulletKills++;
                    break;
                case DeathType.Tool:
                    ToolKills++;
                    break;
                case DeathType.Explosion:
                    ExplosionKills++;
                    break;
                case DeathType.Idiocy:
                    OtherKills++;
                    break;
            }
        }
        public int GetTotalKills()
        {
            return TotalKills;
        }

        public string GetAllKills()
        {
            string s =
                $"Total: {this.TotalKills} \n" +
                $"Collison Kills: {this.CollisonKills} \n" +
                $"Bullet Kills: {this.BulletKills} \n" +
                $"Tool Kills: {this.ToolKills} \n" +
                $"Explosion Kills: {this.ExplosionKills} \n" +
                $"Other Kills: {this.OtherKills} \n";
            return s;
        }
        public override string ToString()
        {
            string s =
            $"{this.TotalKills}:" +
            $"{this.CollisonKills}:" +
            $"{this.BulletKills}:" +
            $"{this.ToolKills}:" +
            $"{this.ExplosionKills}:" +
            $"{this.OtherKills}";
            return s;
        }
        public void FromString(string s)
        {
            string[] sl = s.Split(':');

            TotalKills = int.Parse(sl[0]);
            CollisonKills = int.Parse(sl[1]);
            BulletKills = int.Parse(sl[2]);
            ToolKills = int.Parse(sl[3]);
            ExplosionKills = int.Parse(sl[4]);
            OtherKills = int.Parse(sl[5]);
        }
    }
}
