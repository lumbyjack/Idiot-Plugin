using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace IdiotPlugin.Util
{
    public enum SQLDataType
    {
        All,
        Kills,
        Deaths
    }
    public class SQL
    {
        private static Config Config { get { return IdiotPlugin.Config; } }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static MySqlConnectionStringBuilder ConnString = new MySqlConnectionStringBuilder();
        public static void ReloadConfig()
        {
            ConnString = new MySqlConnectionStringBuilder
            {
                Server = Config.SQLAddress,
                Port = Convert.ToUInt32(Config.SQLPort),
                UserID = Config.SQLUN,
                Password = Config.SQLPW,
                Database = Config.SQLDBName,
                Pooling = false,
            };

            Log.Warn("SQL config reloaded.");
        }
        public static async void Write(string text = "", bool vote = false)
        {
            MySqlConnection dbCon = new MySqlConnection(ConnString.ConnectionString);
            MySqlCommand query = dbCon.CreateCommand();

            query.CommandText = text;
            query.CommandType = System.Data.CommandType.Text;

            try
            {
                await dbCon.OpenAsync();
                int x = await query.ExecuteNonQueryAsync();
                dbCon.Close();
            }
            catch (Exception e)
            {
                dbCon.Close();
                Log.Info(e.Message);
            }
        }
        public static void Init()
        {
            ConnString = new MySqlConnectionStringBuilder
            {
                Server = Config.SQLAddress,
                Port = Convert.ToUInt32(Config.SQLPort),
                UserID = Config.SQLUN,
                Password = Config.SQLPW,
                Database = Config.SQLDBName,
                Pooling = false,
            };
        }
        public static async Task<int> GetInt(string text = "", bool vote = false)
        {
            MySqlConnection dbCon = new MySqlConnection(ConnString.ConnectionString);

            MySqlCommand query = dbCon.CreateCommand();
            query.CommandText = text;
            query.CommandType = System.Data.CommandType.Text;
            object DataOut = null;
            try
            {
                await dbCon.OpenAsync();
                DataOut = await query.ExecuteScalarAsync();
                dbCon.Close();
            }
            catch (Exception e)
            {
                dbCon.Close();
                Log.Info(e.Message);
            }
            return Convert.ToInt32(DataOut);
        }
        public static async Task<object[]> GetPlayerDataFromMySQL(ulong SID, SQLDataType type)
        {
            MySqlConnection dbCon = new MySqlConnection(ConnString.ConnectionString);

            int DataLength = 0;
            MySqlCommand query = dbCon.CreateCommand();
            switch (type)
            {
                case SQLDataType.All:
                    query.CommandText =
                    @$"Select
                        {Config.SQLKDRName}.TotalKills,
                        {Config.SQLKDRName}.ToolKills,
                        {Config.SQLKDRName}.OtherKills,
                        {Config.SQLKDRName}.ExplosionKills,
                        {Config.SQLKDRName}.CollisonKills,
                        {Config.SQLKDRName}.BulletKills,
                        {Config.SQLKDRName}.TotalDeaths,
                        {Config.SQLKDRName}.BulletDeaths,
                        {Config.SQLKDRName}.CollisonDeaths,
                        {Config.SQLKDRName}.ExplosionDeaths,
                        {Config.SQLKDRName}.O2Deaths,
                        {Config.SQLKDRName}.OtherDeaths,
                        {Config.SQLKDRName}.SuicideDeaths,
                        {Config.SQLKDRName}.ToolDeaths,
                        {Config.SQLKDRName}.VoxelDeaths
                    From
                        {Config.SQLKDRName}
                    Where
                        {Config.SQLKDRName}.SID = @peram";
                    DataLength = 15;
                    break;
                case SQLDataType.Deaths:
                    query.CommandText =
                         @$"Select
                            {Config.SQLKDRName}.TotalDeaths,
                            {Config.SQLKDRName}.BulletDeaths,
                            {Config.SQLKDRName}.CollisonDeaths,
                            {Config.SQLKDRName}.ExplosionDeaths,
                            {Config.SQLKDRName}.O2Deaths,
                            {Config.SQLKDRName}.OtherDeaths,
                            {Config.SQLKDRName}.SuicideDeaths,
                            {Config.SQLKDRName}.ToolDeaths,
                            {Config.SQLKDRName}.VoxelDeaths
                        From
                            {Config.SQLKDRName}
                        Where {Config.SQLKDRName}.SID = @peram";
                    DataLength = 9;
                    break;
                case SQLDataType.Kills:
                    query.CommandText =
                        @$"Select
                            {Config.SQLKDRName}.TotalKills,
                            {Config.SQLKDRName}.ToolKills,
                            {Config.SQLKDRName}.OtherKills,
                            {Config.SQLKDRName}.ExplosionKills,
                            {Config.SQLKDRName}.CollisonKills,
                            {Config.SQLKDRName}.BulletKills
                        From
                            {Config.SQLKDRName}
                        Where
                            {Config.SQLKDRName}.SID = @peram";
                    DataLength = 6;
                    break;
            }
            query.Parameters.AddWithValue("@peram", SID.ToString());
            query.CommandType = System.Data.CommandType.Text;
            object[] DataOut = new object[DataLength];
            try
            {
                await dbCon.OpenAsync();
                using MySqlDataReader reader = await query.ExecuteReaderAsync();
                while (reader.Read())
                {
                    reader.GetValues(DataOut);
                }
                dbCon.Close();
            }
            catch (Exception e)
            {
                dbCon.Close();
                Log.Info(e.Message);
            }
            return DataOut;
        }
        public static bool CheckConn(bool vote = false) 
        {
            bool r = false;
            MySqlConnection dbCon = new MySqlConnection(ConnString.ConnectionString);
            try
            {
                dbCon.Open();
                r = dbCon.Ping();
                dbCon.Close();
            }
            catch (Exception e)
            {
                dbCon.Close();
                Log.Info(e.Message);
            }
            return r;
        }
    }
}
