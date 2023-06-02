using Sandbox;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Groups;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using VRage.Game.VisualScripting;
using VRageMath;
using Sandbox.Game.World;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage;
using Sandbox.Game;
using IdiotPlugin.VoteRewards;
using System.Text;
using Sandbox.Game.Components;
using Sandbox.Game.Entities.Character;

namespace IdiotPlugin.Util
{
    public static class Utils
    {
        public static Task<T> InvokeAsync<T>(Func<T> action, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke());
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }
        public static Task<T2> InvokeAsync<T1, T2>(Func<T1, T2> action, T1 arg, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T2>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }

        public static Task<T3> InvokeAsync<T1, T2, T3>(Func<T1, T2, T3> action, T1 arg, T2 arg2, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T3>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg, arg2));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }

        public static Task<T4> InvokeAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4> action, T1 arg, T2 arg2, T3 arg3, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T4>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg, arg2, arg3));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }
        public static bool TryGetBiggestGroup(MyCubeGrid Target, List<long> Entities, out MyCubeGrid BiggestGrid, out List<MyCubeGrid> AllG)
        {
            BiggestGrid = Target;
            double num = 0.0;
            AllG = new List<MyCubeGrid>();
            var enumerator = MyCubeGridGroups.Static.Physical.GetGroup(Target).Nodes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node current = enumerator.Current;
                    //Entities.Add(current.NodeData.EntityId);
                    AllG.Add(current.NodeData);
                    double volume = current.NodeData.PositionComp.WorldAABB.Size.Volume;
                    if (volume > num)
                    {
                        num = volume;
                        BiggestGrid = current.NodeData;
                    }
                }

                return true;
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
        }
        public static void SendColourChatMessage(string message,  Color colour, string author = "", long playerId = 0L, string font = "Blue")
        {
            if (MyMultiplayer.Static != null)
            {
                ScriptedChatMsg msg = default;
                msg.Text = message;
                msg.Author = author;
                msg.Target = playerId;
                msg.Font = font;
                msg.Color = colour;
                MyMultiplayerBase.SendScriptedChatMessage(ref msg);
            }
            else
            {
                try
                {
                    MyHud.Chat.multiplayer_ScriptedChatMessageReceived(message, author, font, colour);
                } 
                catch (Exception e)
                {
                    
                }
                
            }
        }
        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (var value in list)
            {
                await func(value);
            }
        }
        public static string GetPlayerName(ulong steamId)
        {
            MyIdentity id = GetIdentityByNameOrId(steamId.ToString());
            if (id != null && id.DisplayName != null)
            {
                return id.DisplayName;
            }
            else
            {
                return steamId.ToString();
            }
        }
        public static MyIdentity GetIdentityByNameOrId(string playerNameOrSteamId)
        {
            foreach (var identity in MySession.Static.Players.GetAllIdentities())
            {
                if (identity.DisplayName == playerNameOrSteamId)
                    return identity;
                if (ulong.TryParse(playerNameOrSteamId, out ulong steamId))
                {
                    ulong id = MySession.Static.Players.TryGetSteamId(identity.IdentityId);
                    if (id == steamId)
                        return identity;
                    if (identity.IdentityId == (long)steamId)
                        return identity;
                }
            }
            return null;
        }
        public static MyIdentity GetIdenityFromIdentityID(long ID)
        {
                MyIdentity id = MySession.Static.Players.TryGetIdentity(ID);
                return id;
        }
        public static async void SpawnLootBag(long playerId, Dictionary<MyDefinitionId,int> d)
        {


            MyIdentity id = GetIdentityFromPlayerId(playerId);
            MyInventory i = new MyInventory();
            Task AddToInventory(KeyValuePair<MyDefinitionId,int> k)
            {
                if (k.Key.SubtypeId.ToString() == "SpaceCredit")
                {
                    _ = Economy.ModifyPlayerBalance(playerId, k.Value);
                    return Task.CompletedTask;
                }
                MyObjectBuilder_PhysicalObject objectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(k.Key);
                MyFixedPoint myFixedPoint = default(MyFixedPoint);
                myFixedPoint = k.Value;
                i.AddItems(myFixedPoint, objectBuilder);
                return Task.CompletedTask;
            }
            List< KeyValuePair<MyDefinitionId, int> > e = new List<KeyValuePair<MyDefinitionId, int>>();
            e.AddRange(d);
            await e.ForEachAsync(AddToInventory);

            MyGps gps = new MyGps
            {
                ShowOnHud = true,
                Name = "Reward Loot",
                DisplayName = "Reward Loot",
                DiscardAt = null,
                Coords = id.Character.PositionComp.GetPosition(),
                Description = "",
                AlwaysVisible = true,
                GPSColor = new Color(117, 201, 241),
                IsContainerGPS = true
            };
            MySession.Static.Gpss.SendAddGps(playerId,gps,false);
            SpawnInventoryContainer(i,id.Character);
        }
        public static long SpawnInventoryContainer( MyInventory inventory, MyEntity character)
        {
            if (MySession.Static == null || !MySession.Static.Ready)
            {
                return 0L;
            }
            bool spawnAboveCharacter = true;
            long ownerIdentityId = 0L;
            MatrixD worldMatrix = character.WorldMatrix;
            MyDefinitionId bagDefinition = ((MyCharacter)character).Definition.InventorySpawnContainerId.Value;
            if (spawnAboveCharacter)
            {
                worldMatrix.Translation += worldMatrix.Up + worldMatrix.Forward;
            }
            else
            {
                worldMatrix.Translation = character.PositionComp.WorldAABB.Center + worldMatrix.Backward * 0.40000000596046448;
            }
            if (!MyComponentContainerExtension.TryGetContainerDefinition(bagDefinition.TypeId, bagDefinition.SubtypeId, out var definition))
            {
                return 0L;
            }
            MyEntity myEntity = MyEntities.CreateFromComponentContainerDefinitionAndAdd(definition.Id, fadeIn: false);
            if (myEntity == null)
            {
                return 0L;
            }
            if (myEntity is MyInventoryBagEntity myInventoryBagEntity)
            {
                myInventoryBagEntity.OwnerIdentityId = ownerIdentityId;
                if (myInventoryBagEntity.Components.TryGet<MyTimerComponent>(out var component))
                {
                    component.ChangeTimerTick((uint)(MySession.Static.Settings.BackpackDespawnTimer * 3600f));
                }
            }
            myEntity.PositionComp.SetWorldMatrix(ref worldMatrix);
            myEntity.Physics.LinearVelocity = character.Physics.LinearVelocity;
            myEntity.Physics.AngularVelocity = character.Physics.AngularVelocity;
            myEntity.Render.EnableColorMaskHsv = true;
            myEntity.Render.ColorMaskHsv = character.Render.ColorMaskHsv;
            inventory.RemoveEntityOnEmpty = true;
            myEntity.Components.Add((MyInventoryBase)inventory);
            return myEntity.EntityId;
        }
        private static MyIdentity GetIdentityFromPlayerId(long playerId = 0L)
        {
            if (playerId != 0L)
            {
                return MySession.Static.Players.TryGetIdentity(playerId);
            }
            if (MySession.Static.LocalHumanPlayer != null)
            {
                return MySession.Static.LocalHumanPlayer.Identity;
            }
            return null;
        }
    }
    public static class VRageUtils
    {
        public static bool IsAllActionAllowed(this MyEntity self)
        {
            return MySessionComponentSafeZones.IsActionAllowed(self, (MySafeZoneAction)511, 0L, 0uL);
        }
        public static bool IsConcealed(this IMyEntity entity)
        {
            return (long)(entity.Flags & (EntityFlags)4) != 0;
        }
        public static bool IsNormalPlayer(this IMyPlayer onlinePlayer)
        {
            return onlinePlayer.PromoteLevel == MyPromoteLevel.None;
        }

        public static bool IsAdmin(this IMyPlayer onlinePlayer)
        {
            return onlinePlayer.PromoteLevel == MyPromoteLevel.Admin;
        }

        public static ulong GetAdminSteamId()
        {
            string foundValue;
            if ((foundValue = MySandboxGame.ConfigDedicated.Administrators.FirstOrDefault()) == null)
            {
                return 0uL;
            }
            if (!ulong.TryParse(foundValue, out var result))
            {
                return 0uL;
            }
            return result;
        }
        public static T GetParentEntityOfType<T>(this IMyEntity entity) where T : class, IMyEntity
        {
            while (entity != null)
            {
                T val = entity as T;
                if (val != null)
                {
                    return val;
                }
                entity = entity.Parent;
            }
            return null;
        }

        public static void SendAddGps(this MyGpsCollection self, long identityId, MyGps gps, bool playSound)
        {
            self.SendAddGps(identityId, ref gps, gps.EntityId, playSound);
        }
    }

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDictionary(int capacity) : base(capacity) { }
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }

}
