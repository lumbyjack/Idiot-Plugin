using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using IdiotPlugin.DeathLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using IdiotPlugin.Util;
using VRage.Game.Entity;

namespace IdiotPlugin
{
	public static class DeathPatch
	{
		private static bool _init;

		public static ulong _lastKiller;

		private static Config Config { get { return IdiotPlugin.Config; } }
		private static DeathLog DL { get { return IdiotPlugin.DeathLog; } }
		private static SerializableDictionary<ulong, PlayerDeathLog> ADL { get { return IdiotPlugin.ActiveDeathLog; } }
		private static SerializableDictionary<ulong, PlayerKillLog> AKL { get { return IdiotPlugin.ActiveKillLog; } }
		static readonly string[] suicideReasons =
					{
					"clone malfunction",
					"tied shoelaces",
					"backspace",
					"nanites",
					"resurrection",
					"bigdaddylag",
					"dysentery",
					"a bad case of death",
					"the poor man's warp jump",
					"a green potato",
					"a bad pun",
					"Quantum fluctuations"
					};
		static readonly string[] bulletReasons =
					{
					"holy lungs",
					"excessive perforation",
					"kinetic projectiles",
					"poor conflict de-escalation",
					"an overdose of magnesium",
					"hostile ventilation",
					"rapid munitions delivery",
					"failed negotiations"

					};
		static readonly string[] rocksReasons =
					{
					"texting while flying",
					"flying drunk",
					"fogged up visor",
					"conveniently timed lag spike",
					"\"moving\" rocks",
					"an inability to calculate stopping distances"
					};
		static readonly string[] gridReasons =
					{
					"sudden deceleration",
					"vehicular manslaughter",
					"quantum super-positioning",
					"rootin' tootin' Isaac Newton"
					};
		static readonly string[] lowpressureReasons =
					{
					"lack of breathable air",
					"venting lungs to space",
					"forgeting to breathe"
					};
        static readonly string[] explosionReasons =
					{
					"Explosions",
					"rapid unplanned disassembly",
					"sudden conversion to pink mist"
					};
		static readonly Random rnd = new Random();
		public static void Init()
		{
			if (!_init)
			{
				_init = true;
				MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(10, ProcessDamage);
			}

				if (Config.SQLEnabled == false) return;
				string initQ = "USE " + Config.SQLKDRName + ";"
					+
					$"CREATE TABLE IF NOT EXISTS {Config.SQLKDRName}(" +
					"SID varchar(255) NOT NULL," +
					" PlayerName varchar(255)," +

					" TotalDeaths int," +
					" CollisonDeaths int," +
					" BulletDeaths int," +
					" ToolDeaths int," +
					" O2Deaths int," +
					" VoxelDeaths int," +
					" SuicideDeaths int," +
					" ExplosionDeaths int," +
					" OtherDeaths int," +

					" TotalKills int," +
					" CollisonKills int," +
					" BulletKills int," +
					" ToolKills int," +
					" ExplosionKills int," +
					" OtherKills int," +

					" PRIMARY KEY(SID)" + ");";
				Util.SQL.Write(initQ);
		}

		private static void ProcessDamage(object target, ref MyDamageInformation info)
		{
			MyCharacter character = target as MyCharacter;
			if (!Config.DeathCountEnabled || target == null)
			{
				return;
			}
			if (character == null || character.IsBot || !character.IsPlayer || character.IsDead || character.Integrity < 0f || character.GetIdentity() == null)
			{
				return;
			}
			if (character.Integrity - info.Amount > 0f)
			{
				return;
			}
			if (character.PromoteLevel == MyPromoteLevel.Admin)
			{
				return;
			}
			var victim = "";
			var causeOfDeath = "";
			DeathType TypeOfDeath = DeathType.Idiocy;

			victim = character?.DisplayName;
			causeOfDeath = info.Type.String;
			if (string.IsNullOrEmpty(causeOfDeath) || string.IsNullOrEmpty(victim))
			{
				return;
			}
			MyIdentity attackingIdentity = MySession.Static.Players.TryGetIdentity(info.AttackerId);
			ulong steamId = MySession.Static.Players.TryGetSteamId(character.GetIdentity().IdentityId);

            MyEntity attacker;
            if (MyEntities.TryGetEntityById(info.AttackerId, out attacker, allowClosed: true))
			{

				switch (attacker)
				{
					case MyCubeBlock b:
						attackingIdentity = MySession.Static.Players.TryGetIdentity(b.CubeGrid.BigOwners.FirstOrDefault());
						break;
					case MyVoxelBase v:
						causeOfDeath = rocksReasons[rnd.Next(rocksReasons.Length)];
						TypeOfDeath = DeathType.Voxel;
						attackingIdentity = character.GetIdentity();
						break;
					case MyCubeGrid g:
						causeOfDeath = gridReasons[rnd.Next(gridReasons.Length)];
						TypeOfDeath = DeathType.Colision;
						attackingIdentity = MySession.Static.Players.TryGetIdentity(g.BigOwners.FirstOrDefault());
						break;
					case IMyHandheldGunObject<MyGunBase> gb:
						attackingIdentity = MySession.Static.Players.TryGetIdentity(gb.OwnerIdentityId);
						TypeOfDeath = DeathType.Bullet;
						break;
					case IMyHandheldGunObject<MyToolBase> tb:
						attackingIdentity = MySession.Static.Players.TryGetIdentity(tb.OwnerIdentityId);
						TypeOfDeath = DeathType.Tool;
						causeOfDeath = "a tool";
						break;
				}

			}
            switch (causeOfDeath.ToLower())
            {
				case "suicide":
					causeOfDeath = suicideReasons[rnd.Next(suicideReasons.Length)];
					attackingIdentity = character.GetIdentity();
					TypeOfDeath = DeathType.Suicide;
					break;
				case "lowpressure":
					causeOfDeath = lowpressureReasons[rnd.Next(lowpressureReasons.Length)];
					attackingIdentity = character.GetIdentity();
					TypeOfDeath = DeathType.O2;
					break;
				case "bullet":
					causeOfDeath = bulletReasons[rnd.Next(bulletReasons.Length)];
					TypeOfDeath = DeathType.Bullet;
					break;
				case "explosion":
					causeOfDeath = explosionReasons[rnd.Next(explosionReasons.Length)];
					TypeOfDeath = DeathType.Explosion;
					break;
				default:
					causeOfDeath = "something";
					TypeOfDeath = DeathType.Idiocy;
					break;

			}
			if(Config.DeathCostEnabled)
            {
				DeathCost.ChargeForNewClone(character.GetIdentity());
            }
			if (Config.SQLEnabled)
			{
				SQLLog.AddValue(TypeOfDeath, steamId);
			}
			else
			{
				if (!ADL.TryGetValue(steamId, out PlayerDeathLog ndl))
				{
					ndl = new PlayerDeathLog();
					ADL.Add(steamId, ndl);
				}
				ADL[steamId].AddDeath(TypeOfDeath);

			}

			if (Config.DeathFeedEnabled)
			{
				MyVisualScriptLogicProvider.SendChatMessage($" [ {victim} died to {causeOfDeath} ]", "", 0L);
			}

			IMyPlayer p = MyAPIGateway.Multiplayer.Players.GetPlayerControllingEntity(attacker);
			if (p != null)
			{
				ulong attackerSteamId = p.SteamUserId;

				if (steamId != attackerSteamId && attackerSteamId != 0uL)
				{
					if (Config.SQLEnabled)
					{
						SQLLog.AddValue(TypeOfDeath, attackerSteamId, true);
					}
					else
					{
						if (!AKL.TryGetValue(attackerSteamId, out PlayerKillLog nkl))
						{
							nkl = new PlayerKillLog();
							AKL.Add(attackerSteamId, nkl);
						}
						AKL[attackerSteamId].AddKill(TypeOfDeath);
					}
					if (Config.DeathFeedEnabled)
					{
						MyVisualScriptLogicProvider.SendChatMessage($" [ You killed {victim} with {causeOfDeath} ]", "", p.IdentityId);
					}
				}

			}
			DL.RefreshModel();
		}
	}
}
