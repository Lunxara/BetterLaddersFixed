using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.Bindings;

namespace BetterLadders
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BetterLadders : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        public static BetterLadders Instance;

        private ConfigEntry<float> configClimbSpeedMultiplier;
        private ConfigEntry<float> configClimbSprintSpeedMultiplier;
        private ConfigEntry<bool> configAllowTwoHanded;
        private ConfigEntry<bool> configScaleAnimationSpeed;

        void Awake()
        {
            Instance = this;
            configClimbSpeedMultiplier = Config.Bind("General", "climbSpeedMultipler", 1.0f, "Ladder climb speed multiplier");
            configClimbSprintSpeedMultiplier = Config.Bind("General", "sprintingClimbSpeedMultiplier", 1.0f, "Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier");
            configAllowTwoHanded = Config.Bind("General", "allowTwoHanded", true, "Whether to allow using ladders while carrying a two-handed object");
            configScaleAnimationSpeed = Config.Bind("General", "scaleAnimationSpeed", true, "Whether to scale the speed of the climbing animation to the climbing speed");

            // Plugin startup logic
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded!");

            harmony.PatchAll(typeof(BetterLadders));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(InteractTriggerPatch));
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class PlayerControllerBPatch
        {
            [HarmonyPatch("Update")]
            [HarmonyPostfix]
            private static void LadderClimbSpeedPatch(ref bool ___isSprinting, ref float ___climbSpeed, ref bool ___isClimbingLadder, ref PlayerControllerB __instance)
            {
                if (___isSprinting && ___isClimbingLadder)
                {
                    ___climbSpeed = 4.0f * BetterLadders.Instance.configClimbSpeedMultiplier.Value * BetterLadders.Instance.configClimbSprintSpeedMultiplier.Value;
                    if (BetterLadders.Instance.configScaleAnimationSpeed.Value)
                    {
                        __instance.playerBodyAnimator.SetFloat("animationSpeed", BetterLadders.Instance.configClimbSprintSpeedMultiplier.Value);
                    }
                }
                else if (!___isSprinting && ___isClimbingLadder)
                {
                    ___climbSpeed = 4.0f * BetterLadders.Instance.configClimbSpeedMultiplier.Value;
                    if (BetterLadders.Instance.configScaleAnimationSpeed.Value)
                    {
                        __instance.playerBodyAnimator.SetFloat("animationSpeed", BetterLadders.Instance.configClimbSpeedMultiplier.Value);
                    }
                }
            }
            
            [HarmonyPatch("Interact_performed")]
            [HarmonyPrefix]
            private static void LadderTwoHandedAccessPatch(ref InteractTrigger ___hoveringOverTrigger, ref bool ___twoHanded)
            {
                if (BetterLadders.Instance.configAllowTwoHanded.Value && ___hoveringOverTrigger != null)
                {
                    if (___hoveringOverTrigger.isLadder)
                    {
                        if (___twoHanded) {
                            ___hoveringOverTrigger.twoHandedItemAllowed = true;
                            ___hoveringOverTrigger.specialCharacterAnimation = false;
                            ___hoveringOverTrigger.hidePlayerItem = true;
                        }
                    }
                }
            }

            [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
            [HarmonyPostfix]
            private static void LadderHandsFullTipPatch(ref PlayerControllerB __instance, ref InteractTrigger ___hoveringOverTrigger, ref bool ___isHoldingInteract, ref bool ___twoHanded)
            {
                if (BetterLadders.Instance.configAllowTwoHanded.Value && ___hoveringOverTrigger != null && ___isHoldingInteract && ___twoHanded && ___hoveringOverTrigger.isLadder)
                {
                    __instance.cursorTip.text = ___hoveringOverTrigger.hoverTip.Replace("[LMB]", "[E]");
                }
            }
        }
        [HarmonyPatch(typeof(InteractTrigger))]
        internal class InteractTriggerPatch
        {
            [HarmonyPatch("SetUsingLadderOnLocalClient")]
            [HarmonyPostfix]
            private static void LadderTwoHandedRevealItemPatch(ref bool ___usingLadder)
            {
                if (BetterLadders.Instance.configAllowTwoHanded.Value)
                {
                    PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
                    if (playerController.isHoldingObject && playerController.twoHanded)
                    {
                        playerController.currentlyHeldObjectServer.EnableItemMeshes(enable: !___usingLadder);
                    }
                }
            }
        }
    }
}