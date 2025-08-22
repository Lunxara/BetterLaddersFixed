using HarmonyLib;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Unity.Netcode;
using BetterLadders.Networking;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class NetworkingInitPatches
    {
        public static GameObject? NetworkerPrefab
        {
            get
            {
                if (field == null)
                {
                    field = new("BetterLaddersNetworker")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };

                    NetworkObject networkObject = field.AddComponent<NetworkObject>();

                    try
                    {
                        byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().GetName().Name));
                        networkObject.GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);
                    }
                    catch (Exception e)
                    {
                        BetterLadders.StaticLogger.LogError($"Could not override default networker hash: {e}");
                    }

                    _ = field.AddComponent<BetterLaddersNetworker>();
                }

                return field;
            }
            private set;
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        [HarmonyPrefix]
        private static void StartOfRoundAwake_Pre()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (NetworkerPrefab == null)
            {
                BetterLadders.StaticLogger.LogWarning("Networker prefab is missing and could not be created; mod won't be able to do anything.");

                return;
            }

            GameObject prefabInstance = UnityEngine.Object.Instantiate(NetworkerPrefab);
            prefabInstance.hideFlags = HideFlags.None;

            prefabInstance.GetComponent<NetworkObject>().Spawn(true);
        }

        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        [HarmonyPrefix]
        private static void GameNetworkManagerStart_Pre()
        {
            if (NetworkerPrefab == null)
            {
                BetterLadders.StaticLogger.LogWarning("Networker prefab is missing and could not be created; mod won't be able to do anything.");

                return;
            }

            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(NetworkerPrefab))
            {
                NetworkManager.Singleton.AddNetworkPrefab(NetworkerPrefab);
            }
        }
    }
}