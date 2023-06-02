using NLog;
using Sandbox;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Voxels;
using VRageMath;

namespace IdiotPlugin.Util
{
    public class RegenThread
    {
        public static Vector3D Center { get; set; }
        public static float Radius { get; set; }
        public static int GridRadius { get; set; }
        private static readonly Logger Log = LogManager.GetLogger("Idiot");
        public static void DoAreaRegen()
        {
            ResetVoxelInArea(Center, Radius);
        }
        private static void ResetVoxelInArea(Vector3D Center, float Radius)
        {
            try
            {
                BoundingSphereD Sphere = new BoundingSphereD(Center, Radius);
                List<MyVoxelBase> Maps = MyEntities.GetEntitiesInSphere(ref Sphere).OfType<MyVoxelBase>().ToList();
                if (Maps.Count == 0)
                    return;

                foreach (var voxelMap in Maps)
                {
                    using (voxelMap.Pin())
                    {
                        Task<bool> vox = Utils.InvokeAsync(() => ResetVoxels(voxelMap));
                        if (!vox.Wait(700))
                        {
                            Log.Warn("voxel reset timed out.");
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Voxel reset failed!");
            }
        }
        private static bool ResetVoxels(MyVoxelBase voxelMap)
        {
            if (voxelMap.MarkedForClose)
            {
                return false;
            }
            MyShapeSphere shape = new MyShapeSphere
            {
                Center = Center,
                Radius = Radius
            };
            Vector3I numCells;
            BoundingBoxD shapeAabb = shape.GetWorldBoundaries();
            Vector3I StorageSize = voxelMap.Storage.Size;
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMap.PositionLeftBottomCorner, ref shapeAabb.Min, out Vector3I minCorner);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMap.PositionLeftBottomCorner, ref shapeAabb.Max, out Vector3I maxCorner);
            minCorner += voxelMap.StorageMin;
            maxCorner += voxelMap.StorageMin;
            maxCorner += 1;
            StorageSize -= 1;
            Vector3I.Clamp(ref minCorner, ref Vector3I.Zero, ref StorageSize, out minCorner);
            Vector3I.Clamp(ref maxCorner, ref Vector3I.Zero, ref StorageSize, out maxCorner);
            numCells = new Vector3I((maxCorner.X - minCorner.X) / 16, (maxCorner.Y - minCorner.Y) / 16, (maxCorner.Z - minCorner.Z) / 16);
            minCorner = Vector3I.Max(Vector3I.One, minCorner);
            maxCorner = Vector3I.Max(minCorner, maxCorner - Vector3I.One);

            voxelMap.Storage.DeleteRange(MyStorageDataTypeFlags.ContentAndMaterial, minCorner, maxCorner, false);
            BoundingBoxD cutOutBox = shape.GetWorldBoundaries();

            MySandboxGame.Static.Invoke(delegate
            {
                if (voxelMap.Storage != null)
                {
                    voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.ContentAndMaterial);
                    MyVoxelGenerator.NotifyVoxelChanged(MyVoxelBase.OperationType.Revert, voxelMap, ref cutOutBox);
                }
            }, "RevertShape notify");

            return true;
        }

    }

}
