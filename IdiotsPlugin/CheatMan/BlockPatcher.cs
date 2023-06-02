using NLog;
using System;

using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;

using Torch.Managers.PatchManager;
using System.Reflection;
using Sandbox.Game.Entities.Cube;
using Sandbox.Definitions;
using VRage.Sync;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
using VRage.Game;
using Sandbox.Game.Weapons;
using Sandbox.Game.Entities.Blocks;
using Sandbox;
using System.Collections.Concurrent;
using VRage.Network;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.GameSystems;
using Torch.Utils;
using Sandbox.Game.Gui;

namespace IdiotPlugin.CheatMan
{
	public delegate void OnInitCallback<T>(T __instance);
	
	[PatchShim]
	public static class FloatValidation
	{
		public static void Patch(PatchContext ctx)
		{
			ctx.OnInit(delegate (MyBeacon b)
			{
				b.Sync<float>("m_radius").ValidateRange(1f, 1000000f);
			});
			ctx.OnInit(delegate (MyGyro b)
			{
				b.Sync<float>("m_gyroPower").ValidateRange(0f, 1f);
				float maxVelocity = GetMaxVelocity(b.CubeGrid);
				b.Sync<Vector3>("m_gyroOverrideVelocity").ValidateRange(new Vector3(0f - maxVelocity, 0f - maxVelocity, 0f - maxVelocity), new Vector3(maxVelocity, maxVelocity, maxVelocity));
			});
			ctx.OnInit(delegate (MyJumpDrive b)
			{
				b.Sync<float>("m_jumpDistanceRatio").ValidateRange(0f, 1f);
			});
			ctx.OnInit(delegate (MyLargeTurretBase b)
			{
				b.Sync<float>("m_shootingRange").ValidateRange(0f, b.BlockDefinition.MaxRangeMeters);
			});
			ctx.OnInit(delegate (MyLightingBlock b)
			{
				MyLightingBlockDefinition blockDefinition = b.BlockDefinition;
				b.Sync<float>("m_lightRadius").ValidateRange(/*b.IsReflector ?*/ blockDefinition.LightReflectorRadius /*: blockDefinition.LightRadius*/);
				b.Sync<float>("m_lightFalloff").ValidateRange(blockDefinition.LightFalloff);
				b.Sync<float>("m_blinkIntervalSeconds").ValidateRange(blockDefinition.BlinkIntervalSeconds);
				b.Sync<float>("m_blinkLength").ValidateRange(blockDefinition.BlinkLenght);
				b.Sync<float>("m_blinkOffset").ValidateRange(blockDefinition.BlinkOffset);
				b.Sync<float>("m_intensity").ValidateRange(blockDefinition.LightIntensity);
				b.Sync<float>("m_lightOffset").ValidateRange(blockDefinition.LightOffset);
			});
			ctx.OnInit(delegate (MyMechanicalConnectionBlockBase b)
			{
				b.Sync<float>("m_safetyDetach").ValidateRange(b.BlockDefinition.CastO<MyMechanicalConnectionBlockBaseDefinition>().SafetyDetachMin, b.BlockDefinition.CastO<MyMechanicalConnectionBlockBaseDefinition>().SafetyDetachMax);
			});
			ctx.OnInit(delegate (MyMotorStator b)
			{
				MyMotorStatorDefinition myMotorStatorDefinition = (MyMotorStatorDefinition)b.BlockDefinition;
				b.Torque.ValidateRange(0f, myMotorStatorDefinition.MaxForceMagnitude);
				b.BrakingTorque.ValidateRange(0f, myMotorStatorDefinition.MaxForceMagnitude);
				b.TargetVelocity.ValidateRange(0f - b.MaxRotorAngularVelocity, b.MaxRotorAngularVelocity);
			});
			ctx.OnInit(delegate (MyMotorSuspension b)
			{
				b.Sync<float>("m_maxSteerAngle").ValidateRange(0f, b.BlockDefinition.MaxSteer);
				b.Sync<float>("m_power").ValidateRange(0f, 1f);
				b.Sync<float>("m_strenth").ValidateRange(0f, 1f);
				b.Sync<float>("m_height").ValidateRange(b.BlockDefinition.MinHeight, b.BlockDefinition.MaxHeight);
				b.Sync<float>("m_friction").ValidateRange(0f, 1f);
				b.Sync<float>("m_speedLimit").ValidateRange(0f, 360f);
				b.Sync<float>("m_propulsionOverride").ValidateRange(-1f, 1f);
				b.Sync<float>("m_steeringOverride").ValidateRange(-1f, 1f);
			});
			ctx.OnInit(delegate (MyOreDetector b)
			{
				b.Sync<float>("m_range").ValidateRange(0f, b.Range);
			});
			ctx.OnInit(delegate (MyPistonBase b)
			{
				b.Velocity.ValidateRange(0f - b.BlockDefinition.MaxVelocity, b.BlockDefinition.MaxVelocity);
				b.MaxLimit.ValidateRange(b.BlockDefinition.Minimum, b.BlockDefinition.Maximum);
				b.MinLimit.ValidateRange(b.BlockDefinition.Minimum, b.BlockDefinition.Maximum);
				float max = MySandboxGame.Config.ExperimentalMode ? float.MaxValue : b.BlockDefinition.UnsafeImpulseThreshold;
				b.MaxImpulseAxis.ValidateRange(100f, max);
				b.MaxImpulseNonAxis.ValidateRange(100f, max);
			});
			ctx.OnInit(delegate (MyRadioAntenna b)
			{
				b.Sync<float>("m_radius").ValidateRange(1f, b.BlockDefinition.CastO<MyRadioAntennaDefinition>().MaxBroadcastRadius);
			});
			ctx.OnInit(delegate (MySensorBlock b)
			{
				b.Sync<Vector3>("m_fieldMin").ValidateRange(new Vector3(0f - b.MaxRange), new Vector3(-0.1f));
				b.Sync<Vector3>("m_fieldMax").ValidateRange(new Vector3(0.1f), new Vector3(b.MaxRange));
			});
			ctx.OnInit(delegate (MyThrust b)
			{
				b.Sync<float>("m_thrustOverride").ValidateRange(0f, 100f);
			});
		}

		private static float GetMaxVelocity(MyCubeGrid grid)
		{
			return (grid.GridSizeEnum == MyCubeSize.Small) ? MyGridPhysics.GetSmallShipMaxAngularVelocity() : MyGridPhysics.GetLargeShipMaxAngularVelocity();
		}

	}

    [PatchShim]
	public static class PBAccessFix
	{
		[ReflectedGetter(Type = typeof(MyGridTerminalSystem), Name = "m_blocks")]
		private static readonly Func<MyGridTerminalSystem, HashSet<MyTerminalBlock>> _getBlocks;

		public static void Patch(PatchContext ctx)
		{
			ctx.GetPattern(typeof(MyGridTerminalSystem).GetMethod("UpdateGridBlocksOwnership")).Prefixes.Add(typeof(PBAccessFix).GetMethod("DenyAccessToNoOwner", BindingFlags.Static | BindingFlags.Public));
		}
		public static bool DenyAccessToNoOwner(MyGridTerminalSystem __instance, long ownerID)
		{
			if (ownerID == 0L)
			{
				foreach (MyTerminalBlock item in _getBlocks(__instance))
				{
					item.IsAccessibleForProgrammableBlock = false;
				}
				return false;
			}
			return true;
		}
	}


	[PatchShim]
	public static class Extensions
	{
		private static readonly ConcurrentDictionary<Type, List<KeyValuePair<MethodInfo, object>>> _patches = new ConcurrentDictionary<Type, List<KeyValuePair<MethodInfo, object>>>();

		public static readonly MethodInfo InitMethod = typeof(MyTerminalBlock).GetMethod("Init", new Type[2]
		{
			typeof(MyObjectBuilder_CubeBlock),
			typeof(MyCubeGrid)
		});

		public static Sync<T, SyncDirection.BothWays> Sync<T>(this MyTerminalBlock block, string fieldName)
		{
			return (Sync<T, SyncDirection.BothWays>)block.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(block);
		}

		public static void ValidateRange<T>(this Sync<T, SyncDirection.BothWays> sync, T min, T max) where T : IComparable
		{
			sync.Validate = delegate (T value)
			{
				if (value.CompareTo(max) > 0 && value.CompareTo(min) < 0)
				{
					if (!MyEventContext.Current.IsLocallyInvoked)
					{
						ValidationPatch.ValidationFailed(MyEventContext.Current.Sender.Value, kick: false, "value validation failed");
					}
					return false;
				}
				return true;
			};
		}

		public static void ValidateRange(this Sync<Vector3, SyncDirection.BothWays> sync, Vector3 min, Vector3 max)
		{
			sync.Validate = delegate (Vector3 value)
			{
				if (!value.IsInsideInclusive(ref min, ref max))
				{
					if (!MyEventContext.Current.IsLocallyInvoked)
					{
						ValidationPatch.ValidationFailed(MyEventContext.Current.Sender.Value, kick: false, "value validation failed");
					}
					return false;
				}
				return true;
			};
		}

		public static void ValidateRange(this Sync<float, SyncDirection.BothWays> sync, MyBounds bounds)
		{
			sync.Validate = delegate (float value)
			{
				if (!(value <= bounds.Max) || !(value >= bounds.Min))
				{
					if (!MyEventContext.Current.IsLocallyInvoked)
					{
						ValidationPatch.ValidationFailed(MyEventContext.Current.Sender.Value, kick: false, "value validation failed");
					}
					return false;
				}
				return true;
			};
		}

		public static void InitPatch(MyTerminalBlock __instance)
		{
			_patches.GetValueOrDefault(__instance.GetType(), null)?.ForEach(delegate (KeyValuePair<MethodInfo, object> b)
			{
				MethodInfo key = b.Key;
				object value = b.Value;
				object[] parameters = new MyTerminalBlock[1] { __instance };
				key.Invoke(value, parameters);
			});
		}

		public static void Patch(PatchContext ctx)
		{
			ctx.GetPattern(InitMethod).Suffixes.Add(typeof(Extensions).GetMethod("InitPatch"));
		}

		public static void OnInit<T>(this PatchContext ctx, OnInitCallback<T> action)
		{
			_patches.AddOrUpdate(typeof(T), new List<KeyValuePair<MethodInfo, object>>
			{
				new KeyValuePair<MethodInfo, object>(action.Method, action.Target)
			}, delegate (Type a, List<KeyValuePair<MethodInfo, object>> b)
			{
				b.Add(new KeyValuePair<MethodInfo, object>(action.Method, action.Target));
				return b;
			});
		}

		public static T CastO<T>(this object obj)
		{
			return (T)obj;
		}
	}
	public static class ValidationPatch
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		public static bool ValidationFailed(ulong clientId, bool kick, string additionalInfo)
		{
			Log.Error("Validation Failed!\n" + Environment.StackTrace);
			Log.Warn(string.Format("{0} ({1}) {2}", MySession.Static.Players.TryGetIdentityNameFromSteamId(clientId), clientId, kick ? "was trying to cheat! [SAPe]" : "action was blocked. [SAPe]"));
			if (!string.IsNullOrEmpty(additionalInfo))
			{
				Log.Info(additionalInfo);
			}
			if (kick)
			{
				MyMultiplayer.Static.KickClient(clientId);
			}
			else
			{
				MyMultiplayer.Static.DisconnectClient(clientId);
			}
			return false;
		}
	}
	internal interface IPatch
	{
		void Patch(PatchContext ctx);
	}
}
