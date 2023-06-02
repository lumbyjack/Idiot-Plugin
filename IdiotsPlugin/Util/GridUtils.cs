using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Entities.Interfaces;
using Sandbox.Game.GameSystems.Electricity;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace IdiotPlugin.Util
{
    public static class GridUtils
    {
        public static async void FillTank(long GridID)
        {
            MyCubeGrid Grid = GetIngameGrid(GridID);

            static Task Fill(MySlimBlock b)
            {
                ((MyGasTank)b.FatBlock).ChangeFillRatioAmount(1.0);
                ((MyGasTank)b.FatBlock).ResourceSink.Update();
                ((MyGasTank)b.FatBlock).SetDetailedInfoDirty();
                ((MyGasTank)b.FatBlock).RaisePropertiesChanged();
                return Task.CompletedTask;
            };
            List<MySlimBlock> l = new List<MySlimBlock>();
            l.AddRange(Grid.CubeBlocks.Where(cb => cb.FatBlock is MyGasTank));
            await l.ForEachAsync(Fill);
        }
        public static async void RechargeBatteries(long GridID)
        {
            MyCubeGrid Grid = GetIngameGrid(GridID);
            static Task Charge(MySlimBlock b)
            {
                ((MyBatteryBlock)b.FatBlock).CurrentStoredPower = ((MyBatteryBlock)b.FatBlock).MaxStoredPower;

                return Task.CompletedTask;
            };
            List<MySlimBlock> l = new List<MySlimBlock>();
            l.AddRange(Grid.CubeBlocks.Where(cb => cb.FatBlock is MyBatteryBlock));
            await l.ForEachAsync(Charge);
        }

        public static async void RechargeJump(long GridID)
        {
            MyCubeGrid Grid = GetIngameGrid(GridID);
            static Task Charge(MySlimBlock b)
            {
                ((MyJumpDrive)b.FatBlock).CurrentStoredPower = ((MyJumpDrive)b.FatBlock).BlockDefinition.PowerNeededForJump;

                return Task.CompletedTask;
            };
            List<MySlimBlock> l = new List<MySlimBlock>();
            l.AddRange(Grid.CubeBlocks.Where(cb => cb.FatBlock is MyJumpDrive));
            await l.ForEachAsync(Charge);
        }

        public static async void Repair(long GridID)
        {
            MyCubeGrid Grid = GetIngameGrid(GridID);
            static Task Repair(MySlimBlock b)
            {

                b.SetIntegrity(b.MaxIntegrity, b.MaxIntegrity, MyIntegrityChangeEnum.Repair, b.CubeGrid.BigOwners.First());
                return Task.CompletedTask;
            };
            List<MySlimBlock> l = new List<MySlimBlock>();
            l.AddRange(Grid.CubeBlocks.Where(cb => !cb.IsFullIntegrity));
            await l.ForEachAsync(Repair);
        }

        internal static MyCubeGrid GetIngameGrid(long GridID)
        {
            MyCubeGrid e = MyEntities.GetEntities().OfType<MyCubeGrid>().First(x => x.EntityId == GridID);
            return e;
        }
    }
}
