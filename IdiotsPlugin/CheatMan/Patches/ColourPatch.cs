using System.Collections.Generic;
using System.Reflection;
using IdiotPlugin.Util;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Network;
using VRageMath;

namespace IdiotPlugin.CheatMan.Patches
{
	internal class ColourPatch : IPatch
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		[ReflectedMethodInfo(typeof(MyPlayerCollection), "OnPlayerColorsChangedRequest")]
		private static readonly MethodInfo _colorsChangeReq;

		public static bool PrefixColorsChange(ref List<Vector3> newColors)
		{
			if (newColors.Count == 14)
			{
				return true;
			}
			Log.Error($"{MySession.Static.Players.TryGetIdentityNameFromSteamId(MyEventContext.Current.Sender.Value)} SteamId {MyEventContext.Current.Sender.Value} sent invalid color request!");
			(MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, (string)null, true);
			MyEventContext.ValidationFailed();
			return false;
		}

		public void Patch(PatchContext ctx)
		{
			ctx.GetPattern(_colorsChangeReq).Prefixes.Add(ReflectionUtils.StaticMethod(typeof(ColourPatch), "PrefixColorsChange"));
		}
	}
}
