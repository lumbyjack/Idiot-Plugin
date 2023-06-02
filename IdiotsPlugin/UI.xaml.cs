using NLog;
using IdiotPlugin.DeathLogger;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
using Torch.API.Managers;
using System.Threading.Tasks;
using Torch.API.Session;
using Sandbox.Game.World;
using System.Windows.Media;
using IdiotPlugin.Util;

namespace IdiotPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UI : UserControl               
    {
        private static readonly Logger Log = IdiotPlugin.Log;
        private static  Config cfg { get { return IdiotPlugin.Config; } }
        private static IdiotPlugin P;
        private static SerializableDictionary<ulong, PlayerDeathLog> ADL { get { return IdiotPlugin.ActiveDeathLog; } }
        private static SerializableDictionary<ulong, PlayerKillLog> AKL { get { return IdiotPlugin.ActiveKillLog; } }
        public UI(IdiotPlugin SP)
        {
            P = SP;
            InitializeComponent();
        }
        private void Imported_LostFocus(object sender, RoutedEventArgs e)
        {
            cfg.RefreshModel();
        }

        private void btnSkipRestart_Click(object sender, RoutedEventArgs e)
        {
            P.getRMan().SkipRestart();
        }

        private void btnStopRestart_Click(object sender, RoutedEventArgs e)
        {
            cfg.StopOnNextRestart = true;
            P.saveConfig();
            btnStopRestart.IsEnabled = false;
        }

        private void btnSaveRestart_Click(object sender, RoutedEventArgs e)
        {
            cfg.RestartInterval = Convert.ToInt32(tbInterval.Text) ;
            P.getRMan().UpdateInterval(Convert.ToInt32(tbInterval.Text));

            cfg.RestartOffset = Convert.ToInt32(tbOffset.Text) ;
            P.getRMan().UpdateOffset(Convert.ToInt32(tbOffset.Text));

            cfg.IsLobby = (bool)cbLobby.IsChecked ;
            cfg.RestartManEnabled = (bool)cbEnable.IsChecked ;
            cfg.HangTimeValue = Convert.ToInt32(tbHangTime.Text);

            P.saveConfig();
            RefreshTB();
        }
        private void RefreshTB()
        {
            
            tbRestartText.Clear(); 
            String strBuilder = "";
            
            foreach (DateTime dt in P.getRMan().GetSch())
            {
                strBuilder += $"Restart Time: {dt:HH:mm:ss} \n ";
            }
            tbRestartText.Text = strBuilder;
            
        }

        private void cbEnable_Checked(object sender, RoutedEventArgs e)
        {
            cfg.RestartManEnabled = (bool)cbEnable.IsChecked;
            P.saveConfig();
        }

        private void cbLobby_Checked(object sender, RoutedEventArgs e)
        {
            cfg.IsLobby = (bool)cbLobby.IsChecked;
            P.saveConfig();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbInterval.Text = cfg.RestartInterval.ToString();
            tbOffset.Text = cfg.RestartOffset.ToString();
            tbHangTime.Text = cfg.HangTimeValue.ToString();
            cbLobby.IsChecked = cfg.IsLobby;
            cbEnable.IsChecked = cfg.RestartManEnabled;
            RefreshTB();
            tbSQLAddress.Text = cfg.SQLAddress;
            tbSQLPort.Text = cfg.SQLPort;
            tbSQLUN.Text = cfg.SQLUN;
            tbSQLPW.Password = cfg.SQLPW;
            tbSQLDBN.Text = cfg.SQLDBName;
            tbKDRName.Text = cfg.SQLKDRName;
            tbDeathCost.Text = cfg.DeathCostPercentage.ToString();
            cbDCEnable.IsChecked = cfg.DeathCostEnabled;
            cbDFEnable.IsChecked = cfg.DeathFeedEnabled;
            cbDLEnable.IsChecked = cfg.DeathCountEnabled;
            cbSessionSwap.IsChecked = cfg.ChangeSession;
            cbSQLEnabled.IsChecked = cfg.SQLEnabled;
            lvVoteRewards.ItemsSource = cfg.Rewards;
            lvVoteLinks.ItemsSource = cfg.VoteLinks;
            lvVoteKeys.ItemsSource = cfg.VoteKeys;
            lvSession.ItemsSource = cfg.Sessions;
        }

        private void cbDLEnable_Checked(object sender, RoutedEventArgs e)
        {
            cfg.DeathCountEnabled = (bool)cbDLEnable.IsChecked;
            P.saveConfig();
        }

        private void cbDFEnable_Checked(object sender, RoutedEventArgs e)
        {
            cfg.DeathFeedEnabled = (bool)cbDFEnable.IsChecked;
            P.saveConfig();
        }
        private void cbDCEnable_Checked(object sender, RoutedEventArgs e)
        {
            cfg.DeathCostEnabled = (bool)cbDCEnable.IsChecked;
            P.saveConfig();
        }
        private void btnSaveDL_Click(object sender, RoutedEventArgs e)
        {
            cfg.DeathCostPercentage = Convert.ToDouble(tbDeathCost.Text);
            P.saveConfig();
            RefreshTB();
        }

        /* private async Task<Dictionary<int,Dictionary<string, int>>> FillList( Dictionary<string, int> killLeaderboard, Dictionary<string, int> deathLeaderboard, ulong k, bool loaded)
         {


             string name = k.ToString();
             if (loaded) name = Utils.GetPlayerName(k);
             //P.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerServer>()?.GetSteamUsername(k);
             try
             {
                 deathLeaderboard.Add(name, ADL[k].GetTotalDeaths());
                 if (AKL.ContainsKey(k))
                 {
                     name += "  [K: " + AKL[k].GetTotalKills() + " | D: " + ADL[k].GetTotalDeaths() + "] KDR: ";
                     if (ADL[k].GetTotalDeaths() == 0)
                     {
                         killLeaderboard.Add(name, AKL[k].GetTotalKills() / 1);
                     }
                     else
                     {
                         killLeaderboard.Add(name, AKL[k].GetTotalKills() / ADL[k].GetTotalDeaths());
                     }
                 }
                 else
                 {
                     name += "  [K: 0 | D: " + ADL[k].GetTotalDeaths() + "] KDR: ";
                     if (ADL[k].GetTotalDeaths() == 0)
                     {
                         killLeaderboard.Add(name, 0 / 1);
                     }
                     else
                     {
                         killLeaderboard.Add(name, 0 / ADL[k].GetTotalDeaths());
                     }
                 }
             }
             catch (Exception e)
             {
                 Log.Error(e.Message + "\n" + name);
             }

             Dictionary<int, Dictionary<string, int>> result = new Dictionary<int, Dictionary<string, int>>
             {
                 { 0, killLeaderboard },
                 { 1, deathLeaderboard }
             };
             return result;
         }*/
        private void LoadKDR(object sender, RoutedEventArgs e)
        {
/*            tbKillLB.Clear();
            tbDeathLB.Clear();
            cbDLEnable.IsChecked = cfg.DeathCountEnabled;
            cbDFEnable.IsChecked = cfg.DeathFeedEnabled;
            bool loaded = false;

            if (MySession.Static != null)
            {
                loaded = true;
            }
            Dictionary<string, int> killLeaderboard = new Dictionary<string, int>();
            Dictionary<string, int> deathLeaderboard = new Dictionary<string, int>();
            killLeaderboard.Clear();
            deathLeaderboard.Clear();

            Dictionary<int, Dictionary<string, int>> result = new Dictionary<int, Dictionary<string, int>>();
            await Utils.ForEachAsync(ADL.Keys.ToList(), async k => {
               result =  await FillList(killLeaderboard,deathLeaderboard, k, loaded);
            });
            var orderedKLB = from pair in result[0]
                            orderby pair.Value descending
                            select pair;
            int inc = 0;
            foreach (KeyValuePair<string, int> pair in orderedKLB)
            {
                inc++;
                if (inc <= 10)
                {
                    tbKillLB.AppendText(pair.Key + " : " + pair.Value + "\n");
                }
            }

            var orderedDLB = from pair in result[1]
                            orderby pair.Value descending
                            select pair;
            int inc2 = 0;
            foreach (KeyValuePair<string, int> pair in orderedDLB)
            {
                inc2++;
                if (inc2 <= 10)
                {
                    tbDeathLB.AppendText(pair.Key + " : " + pair.Value + "\n");
                }
            }*/
        }

        private void btSQLTest_Click(object sender, RoutedEventArgs e)
        {
            if (!cfg.SQLEnabled) return;
            cfg.SQLAddress = tbSQLAddress.Text;
            cfg.SQLPort = tbSQLPort.Text;
            cfg.SQLUN = tbSQLUN.Text;
            cfg.SQLPW = tbSQLPW.Password;
            cfg.SQLDBName = tbSQLDBN.Text;
            cfg.SQLKDRName = tbKDRName.Text;
            if (Util.SQL.CheckConn())
            {
                eSQLVisual.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 255, 0));
                P.saveConfig();
            }
            else
            {
                eSQLVisual.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 0, 0));
            }
        }

        private void cbSQLEnabled_Checked(object sender, RoutedEventArgs e)
        {
            cfg.SQLEnabled = true;
            P.saveConfig();
        }
        private void cbSQLEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            cfg.SQLEnabled = false;
            P.saveConfig();
        }

        private void CBHangTime_Checked(object sender, RoutedEventArgs e)
        {
            cfg.HangTime = true;
            P.saveConfig();
        }
        private void CBHangTime_Unchecked(object sender, RoutedEventArgs e)
        {
            cfg.HangTime = false;
            P.saveConfig();
        }

        private void cbSessionSwap_Checked(object sender, RoutedEventArgs e)
        {
            cfg.ChangeSession = true;
            P.saveConfig();
        }
        private void cbSessionSwap_Unchecked(object sender, RoutedEventArgs e)
        {
            cfg.ChangeSession = false;
            P.saveConfig();
        }

        private void btnRefeshSessions_Click(object sender, RoutedEventArgs e)
        {
            lvSession.Items.Refresh();
            P.GetSessions();
        }
    }
}
