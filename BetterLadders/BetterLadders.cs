using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace BetterLadders
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(GUID);

        public static Plugin Instance;

        public AssetBundle networkhandler;

        public const string GUID = "e3s1.BetterLadders";
        public const string NAME = "BetterLadders";
        public const string VERSION = "1.2.3";
        internal static bool IsHost => NetworkManager.Singleton.IsHost;
        internal static bool IsServer => NetworkManager.Singleton.IsServer;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static new ManualLogSource Logger { get; private set; }
        public static new Config Config { get; internal set; }
        void Awake()
        {
            Instance = this;

            Logger = base.Logger;
            Config = new(base.Config);

            // Plugin startup logic
            Plugin.Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");

            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(StartMatchLever))]
        internal class StartGamePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("PullLeverAnim")]
            private static void SetClientConfigWhenHostMissingMod()
            {
                if (!(IsHost || IsServer) && !Config.Synced && !Config.Default.defaultsSet)
                {
                    Logger.LogInfo("Config wasn't synced with host (likely doesn't have mod), setting vanilla config defaults");
                    Config.SetVanillaDefaults();
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class PlayerControllerBPatch
        {
            [HarmonyPatch("ConnectClientToPlayerObject")]
            [HarmonyPostfix]
            public static void InitializeLocalPlayer()
            {
                if (IsHost)
                {
                    Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnRequestConfigSync", Config.OnRequestSync);
                    Config.Synced = true;

                    return;
                }

                Config.Synced = false;
                Config.MessageManager.RegisterNamedMessageHandler("BetterLadders_OnReceiveConfigSync", Config.OnReceiveSync);
                Config.RequestSync();
            }

            [HarmonyPatch("Update")] // should probably not use update, find method for sprinting toggle instead
            [HarmonyPostfix]
            private static void LadderClimbSpeedPatch(ref bool ___isSprinting, ref float ___climbSpeed, ref bool ___isClimbingLadder, ref PlayerControllerB __instance)
            {
                if (___isClimbingLadder)
                {
                    float finalClimbSpeed;
                    float animationMultiplier;
                    if (___isSprinting)
                    {
                        // vanilla climb speed is 4.0f
                        finalClimbSpeed = 4.0f * Config.Instance.climbSpeedMultiplier * Config.Instance.sprintingClimbSpeedMultiplier;
                    }
                    else
                    {
                        finalClimbSpeed = 4.0f * Config.Instance.climbSpeedMultiplier;
                    }
                    animationMultiplier = finalClimbSpeed / 4.0f;
                    ___climbSpeed = finalClimbSpeed;
                    if (Config.Default.scaleAnimationSpeed && __instance.playerBodyAnimator.GetFloat("animationSpeed") != 0f) // animationSpeed is set to 0f when the player stops moving
                    {
                        __instance.playerBodyAnimator.SetFloat("animationSpeed", animationMultiplier);
                    }
                }
            }
            
            [HarmonyPatch("Interact_performed")]
            [HarmonyPrefix]
            private static void LadderTwoHandedAccessPatch(ref InteractTrigger ___hoveringOverTrigger, ref bool ___twoHanded)
            {
                if (Config.Instance.allowTwoHanded && ___hoveringOverTrigger != null && ___hoveringOverTrigger.isLadder && ___twoHanded)
                {
                        ___hoveringOverTrigger.twoHandedItemAllowed = true;
                        ___hoveringOverTrigger.specialCharacterAnimation = false;
                }
            }

            [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
            [HarmonyPostfix]
            private static void LadderHandsFullTipPatch(ref PlayerControllerB __instance, ref InteractTrigger ___hoveringOverTrigger, ref bool ___isHoldingInteract, ref bool ___twoHanded)
            {
                if (Config.Instance.allowTwoHanded && ___hoveringOverTrigger != null && ___isHoldingInteract && ___twoHanded && ___hoveringOverTrigger.isLadder)
                {
                    __instance.cursorTip.text = ___hoveringOverTrigger.hoverTip.Replace("[LMB]", "[E]"); // game doesnt replace interact tooltips properly so i wont either
                }
            }

            [HarmonyPatch("SwitchToItemSlot")]
            [HarmonyPostfix]
            private static void LadderHeldItemVisibilityPatch(ref PlayerControllerB __instance)
            {
                LadderItemVisibility.Set(ref __instance.isClimbingLadder);
                // This doesn't affect anything in vanilla since you can't switch slots while climbing a ladder.
                // If a mod that allows switching slots via keybinds is installed, this prevents items from appearing when using them.
            }
        }
        [HarmonyPatch(typeof(InteractTrigger))]
        internal class InteractTriggerPatch
        {
            [HarmonyPatch("SetUsingLadderOnLocalClient")]
            [HarmonyPostfix]
            private static void LadderHeldItemVisibilityPatch(ref bool ___usingLadder)
            {
                LadderItemVisibility.Set(ref ___usingLadder);
            }
        }

        internal static class LadderItemVisibility
        {
            public static void Set(ref bool ___usingLadder)
            {
                PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
                if (playerController.isHoldingObject && playerController.currentlyHeldObjectServer != null)
                {
                    if ((Config.Instance.allowTwoHanded && Config.Default.hideTwoHanded && playerController.twoHanded) || (Config.Default.hideOneHanded && !playerController.twoHanded))
                    {
                        playerController.currentlyHeldObjectServer.EnableItemMeshes(enable: !___usingLadder);
                    }
                }
            }
        }
    }
}