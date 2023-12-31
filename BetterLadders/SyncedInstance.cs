using HarmonyLib;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Netcode;

namespace BetterLadders
{
    [Serializable]
    public class SyncedInstance<T>
    {
        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized]
        protected static int IntSize = 4;

        public static T Default { get; private set; }
        public static T Instance { get; private set; }

        public static bool Synced { get; internal set; }

        protected void InitInstance(T instance)
        {
            Default = instance;
            Instance = instance;

            // Makes sure the size of an integer is correct for the current system.
            // We use 4 by default as that's the size of an int on 32 and 64 bit systems.
            IntSize = sizeof(int);
        }

        internal static void SyncInstance(byte[] data)
        {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }

        internal static void RevertSync()
        {
            Instance = Default;
            Synced = false;
        }

        public static byte[] SerializeToBytes(T val)
        {
            BinaryFormatter bf = new();
            using MemoryStream stream = new();

            try
            {
                bf.Serialize(stream, val);
                return stream.ToArray();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error serializing instance: {e}");
                return null;
            }
        }

        public static T DeserializeFromBytes(byte[] data)
        {
            BinaryFormatter bf = new();
            using MemoryStream stream = new(data);

            try
            {
                return (T)bf.Deserialize(stream);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error deserializing instance: {e}");
                return default;
            }
        }
        public static void RequestSync()
        {
            if (!IsClient) return;

            using FastBufferWriter stream = new(IntSize, Allocator.Temp);
            MessageManager.SendNamedMessage("BetterLadders_OnRequestConfigSync", 0uL, stream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            Plugin.Logger.LogInfo($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

            try
            {
                stream.WriteValueSafe(in value, default);
                stream.WriteBytesSafe(array);

                MessageManager.SendNamedMessage("BetterLadders_OnReceiveConfigSync", clientId, stream);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(IntSize))
            {
                Plugin.Logger.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val))
            {
                Plugin.Logger.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            SyncInstance(data);

            Plugin.Logger.LogInfo("Successfully synced config with host.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave()
        {
            Config.RevertSync();
        }
    }
}
