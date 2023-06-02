using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Network;
using Sandbox.Game.Entities;
using IdiotPlugin.Util;

namespace IdiotPlugin.CheatMan.Patches
{
	class ExplosionPatch : IPatch
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		[ReflectedMethodInfo(typeof(MyExplosions), "ProxyExplosionRequest")]
		private static readonly MethodInfo _proxyExplosionRequest;

		private static bool PrefixProxyExplosionRequest()
		{
			if (!MyEventContext.Current.IsLocallyInvoked)
			{
				Log.Warn($"{MySession.Static.Players.TryGetIdentityNameFromSteamId(MyEventContext.Current.Sender.Value)}, SteamID: {MyEventContext.Current.Sender.Value} Tried to spawn an explosion!");
				(MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, (string)null, true);
				MyEventContext.ValidationFailed();
				return false;
			}
			return true;
		}

		public void Patch(PatchContext ctx)
		{
			ctx.GetPattern(_proxyExplosionRequest).Prefixes.Add(ReflectionUtils.StaticMethod(typeof(ExplosionPatch), "PrefixProxyExplosionRequest"));
		}
	}
}
