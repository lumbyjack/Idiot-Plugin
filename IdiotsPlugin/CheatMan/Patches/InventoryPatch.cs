using System.Reflection;
using NLog;
using Sandbox.Game;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage;
using IdiotPlugin.Util;

namespace IdiotPlugin.CheatMan.Patches
{
	internal class InventoryPatch : IPatch
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		[ReflectedMethodInfo(typeof(MyInventory), "TransferItemsFrom")]
		private static readonly MethodInfo _transferItemsFrom;

		[ReflectedMethodInfo(typeof(MyInventory), "TransferItemsTo")]
		private static readonly MethodInfo _transferItemsTo;

		private static bool PrefixTransferItems(MyFixedPoint? amount, ref bool __result)
		{
			if (amount.HasValue && amount.Value < 0)
			{
				__result = false;
				return false;
			}
			return true;
		}

		public void Patch(PatchContext ctx)
		{
			ctx.GetPattern(_transferItemsFrom).Prefixes.Add(ReflectionUtils.StaticMethod(typeof(InventoryPatch), "PrefixTransferItems"));
			ctx.GetPattern(_transferItemsTo).Prefixes.Add(ReflectionUtils.StaticMethod(typeof(InventoryPatch), "PrefixTransferItems"));
		}
	}

}
