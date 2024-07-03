using BetterLadders;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public class SyncedInstance<T>
{
    internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
    internal static bool IsClient => NetworkManager.Singleton.IsClient;
    internal static bool IsHost => NetworkManager.Singleton.IsHost;
    internal static Harmony harmony => Plugin.Instance.harmony;

    [NonSerialized]
    protected static int IntSize = 4;

    public static T Default { get; private set; }
    public static T Instance { get; private set; }
    public static bool SyncedPatchesApplied { get; private set; }

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
        Plugin.Logger.LogInfo("Requested sync from server");
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
    public static void RequestHideItem(bool enabled)
    {
        //                                  enable?,       default
        using FastBufferWriter stream = new(sizeof(bool) + IntSize, Allocator.Temp);
        stream.WriteValueSafe(in enabled);
        MessageManager.SendNamedMessage("BetterLadders_OnRequestHideItem", 0uL, stream);
        if (GameNetworkManager.Instance.localPlayerController.playerClientId != 0uL)
            Plugin.Logger.LogInfo($"Requested server to {(enabled ? "show" : "hide")} this client's held item");
    }
    public static void OnRequestHideItem(ulong clientId, FastBufferReader reader)
    {
        if (!IsHost) return;

        if (!reader.TryBeginRead(sizeof(bool)))
        {
            Plugin.Logger.LogError("Hide item error: Could not begin reading buffer.");
            return;
        }
        reader.ReadValueSafe(out bool enabled);

        if (clientId != 0uL)
            Plugin.Logger.LogInfo($"Hide item request received from client: {clientId}");

        using FastBufferWriter stream = new(sizeof(ulong) + sizeof(bool) + IntSize, Allocator.Temp);
        stream.WriteValueSafe(in clientId);
        stream.WriteValueSafe(in enabled);
        MessageManager.SendNamedMessageToAll("BetterLadders_OnReceiveHideItem", stream);
    }
    public static void OnReceiveHideItem(ulong _, FastBufferReader reader)
    {
        if (!reader.TryBeginRead(sizeof(ulong) + sizeof(bool)))
        {
            Plugin.Logger.LogError("Hide item error: Could not begin reading buffer.");
            return;
        }
        reader.ReadValueSafe(out ulong requestingClientId);
        reader.ReadValueSafe(out bool enabled);
        if (GameNetworkManager.Instance.localPlayerController.playerClientId == requestingClientId) return; // don't want to hide items twice, and doing it here would have more delay
        PlayerControllerB targetPlayer = UnityEngine.Object.FindObjectsOfType<PlayerControllerB>().FirstOrDefault(player => player.playerClientId == requestingClientId);
        if (targetPlayer != null)
        {
            if (targetPlayer.twoHanded && Config.Default.hideTwoHanded || !targetPlayer.twoHanded && Config.Default.hideOneHanded)
            {
                targetPlayer.currentlyHeldObjectServer.EnableItemMeshes(enabled);
                Plugin.Logger.LogInfo($"Successfully {(enabled ? "showed" : "hid")} client {requestingClientId}'s held item.");
            }
        }
        else
        {
            Plugin.Logger.LogError($"Failed to find client with id {requestingClientId}.");
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.StartDisconnect))]
    public static void PlayerLeave()
    {
        Config.RevertSync();
        if (SyncedPatchesApplied)
        {
            Plugin.Logger.LogInfo("Unpatching transpilers");
            Plugin.Instance.UnpatchSyncedTranspilers();
            SyncedPatchesApplied = false;
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    public static void InitializeLocalPlayer()
    {
        if (IsHost)
        {
            Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnRequestConfigSync", Config.OnRequestSync);
            Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnRequestHideItem", Config.OnRequestHideItem);
            Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnReceiveHideItem", Config.OnReceiveHideItem);
            Config.Synced = true;

            return;
        }

        Config.Synced = false;
        Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnReceiveConfigSync", Config.OnReceiveSync);
        Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnReceiveHideItem", Config.OnReceiveHideItem);
        Config.RequestSync();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.PullLeverAnim))] // PullLeverAnim is synced to all clients
    static void PostSyncOrNoResponse()
    {
        if (!IsHost && !Config.Synced && !Config.Default.hostMissingMod)
        {
            Plugin.Logger.LogInfo("Config wasn't synced with host (likely doesn't have mod), setting vanilla config defaults");
            Config.Instance.SetVanillaDefaults();
        }
        Plugin.Logger.LogInfo("All config syncing operations finished, starting synced patches");
        if (!SyncedPatchesApplied)
        {
            Plugin.Instance.PatchSyncedTranspilers();
            SyncedPatchesApplied = true;
        }
    }
}