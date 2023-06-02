
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using ProtoBuf;
using Torch;
using Torch.Managers;
using Torch.Managers.PatchManager;
using Torch.API;
using NLog;

namespace IdiotPlugin.CheatMan
{
	

	public class CheatManager : Manager
    {
		private static GPSCheat gpsCheat;
		private static CheatLog CL { get { return IdiotPlugin.CheatLog; } }
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		[Dependency(Ordered = false)]
		private readonly PatchManager _patchMgr;

		private static bool _patched = false;

		private PatchContext _patchContext;

		public CheatManager(ITorchBase torchInstance)
			: base(torchInstance)
		{
		}

		public void LogCheat(string info)
        {
			if (!CL.CheatFlag.Contains(info))
			{
				CL.CheatFlag.Add(info);
				IdiotPlugin.SaveCheatLog();
			}
		}
		public void RunGPS()
        {
			if(gpsCheat == null)
            {
				gpsCheat = new GPSCheat();
			}
			gpsCheat.runGPS(this);
		}
		public override void Attach()
		{
			base.Attach();
			if (!_patched)
			{
				_patched = true;
				_patchContext = _patchMgr.AcquireContext();
				PatchFromAssembly(_patchContext);
			}
			Log.Info("[IAC] Attached");
		}

		public override void Detach()
		{
			base.Detach();
			if (_patched)
			{
				_patched = false;
				_patchMgr.FreeContext(_patchContext);
			}
			Log.Info("[IAC] Detached");
		}

		private void PatchFromAssembly(PatchContext ctx)
		{
			foreach (TypeInfo definedType in GetType().Assembly.DefinedTypes)
			{
				if (definedType.ImplementedInterfaces.Contains(typeof(IPatch)))
				{
					(Activator.CreateInstance(definedType) as IPatch)?.Patch(ctx);
				}
			}
		}
	}

	public class CheatLog : ViewModel
	{
		[ProtoMember(1)]
		private List<string> _CheatFlag = new List<string>();
		public List<string> CheatFlag { get => _CheatFlag; set => SetValue(ref _CheatFlag, value); }
	}
}
