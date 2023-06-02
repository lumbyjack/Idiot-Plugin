using ProtoBuf;
using Sandbox.Game.World;
using IdiotPlugin.DeathLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using IdiotPlugin.Util;

namespace IdiotPlugin
{
    public class DeathLog : ViewModel
    {
        [ProtoMember(1)]
        private SerializableDictionary<ulong, string> _DeathCounter = new SerializableDictionary<ulong, string> ();
        public SerializableDictionary<ulong, string> DeathCounter { get => _DeathCounter; set => SetValue(ref _DeathCounter, value); }

        private SerializableDictionary<ulong, string> _KillCounter = new SerializableDictionary<ulong, string>();
        public SerializableDictionary<ulong, string> KillCounter { get => _KillCounter; set => SetValue(ref _KillCounter, value); }
    }
}
