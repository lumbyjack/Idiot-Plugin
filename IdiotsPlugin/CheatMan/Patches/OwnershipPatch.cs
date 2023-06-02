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
	internal class OwnershipPatch : IPatch
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		[ReflectedMethodInfo(typeof(MyCubeGrid), "OnChangeOwnersRequest")]
		private static readonly MethodInfo _onChangeOwner;

		private static bool PrefixOnChangeOwnershipRequest(List<MyCubeGrid.MySingleOwnershipRequest> requests, long requestingPlayer)
		{
			ulong value = MyEventContext.Current.Sender.Value;
			bool flag = true;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"{MySession.Static.Players.TryGetIdentityNameFromSteamId(value)}, SteamID: {value} Ownership Change Requests:");
			if (requests != null)
			{
				foreach (MyCubeGrid.MySingleOwnershipRequest request in requests)
				{
					MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(request.Owner);
					MyCubeBlock myCubeBlock = MyEntities.GetEntityById(request.BlockId, false) as MyCubeBlock;
					if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserAdmin(value) && myCubeBlock.OwnerId != MySession.Static.Players.TryGetIdentityId(value) && myCubeBlock.OwnerId != 0L)
					{
						stringBuilder.Append("INVALID REQUEST! this may indicate some shenanigans: ");
						flag = false;
					}
					stringBuilder.AppendLine($"ownerID {request.Owner}, newOwnerName {myIdentity?.DisplayName}, blockId: {request.BlockId}, grid: {myCubeBlock.CubeGrid?.DisplayName}");
				}
			}
			if (flag)
			{
				Log.Info(stringBuilder);
			}
			else
			{
				Log.Warn(stringBuilder);
			}
			return flag;
		}

		public void Patch(PatchContext ctx)
		{
			ctx.GetPattern(_onChangeOwner).Prefixes.Add(ReflectionUtils.StaticMethod(typeof(OwnershipPatch), "PrefixOnChangeOwnershipRequest"));
		}
	}
}
