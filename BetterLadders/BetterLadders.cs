using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BetterLadders : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(GUID);

        public static BetterLadders Instance;

        public const string GUID = "e3s1.BetterLadders";
        public const string NAME = "BetterLadders";
        public const string VERSION = "1.2.2";

        private ConfigEntry<float> configClimbSpeedMultiplier;
        private ConfigEntry<float> configClimbSprintSpeedMultiplier;
        private ConfigEntry<bool> configAllowTwoHanded;
        private ConfigEntry<bool> configScaleAnimationSpeed;
        private ConfigEntry<bool> configHideOneHanded;
        private ConfigEntry<bool> configHideTwoHanded;

        void Awake()
        {
            Instance = this;
            configClimbSpeedMultiplier = Config.Bind("General", "climbSpeedMultipler", 1.0f, "Ladder climb speed multiplier");
            configClimbSprintSpeedMultiplier = Config.Bind("General", "sprintingClimbSpeedMultiplier", 1.5f, "Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier");
            configAllowTwoHanded = Config.Bind("General", "allowTwoHanded", true, "Whether to allow using ladders while carrying a two-handed object");
            configScaleAnimationSpeed = Config.Bind("General", "scaleAnimationSpeed", true, "Whether to scale the speed of the climbing animation to the climbing speed");
            configHideOneHanded = Config.Bind("General", "hideOneHanded", true, "Whether to hide one-handed items while climbing a ladder - false in vanilla");
            configHideTwoHanded = Config.Bind("General", "hideTwoHanded", true, "Whether to hide two-handed items while climbing a ladder");

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
                if (___isClimbingLadder)
                {
                    float finalClimbSpeed;
                    float animationMultiplier;
                    if (___isSprinting)
                    {
                        // vanilla climb speed is 4.0f
                        finalClimbSpeed = 4.0f * BetterLadders.Instance.configClimbSpeedMultiplier.Value * BetterLadders.Instance.configClimbSprintSpeedMultiplier.Value;
                    }
                    else
                    {
                        finalClimbSpeed = 4.0f * BetterLadders.Instance.configClimbSpeedMultiplier.Value;
                    }
                    animationMultiplier = finalClimbSpeed / 4.0f;
                    ___climbSpeed = finalClimbSpeed;
                    if (BetterLadders.Instance.configScaleAnimationSpeed.Value && __instance.playerBodyAnimator.GetFloat("animationSpeed") != 0f) // animationSpeed is set to 0f when the player stops moving
                    {
                        __instance.playerBodyAnimator.SetFloat("animationSpeed", animationMultiplier);
                    }
                }
            }
            
            [HarmonyPatch("Interact_performed")]
            [HarmonyPrefix]
            private static void LadderTwoHandedAccessPatch(ref InteractTrigger ___hoveringOverTrigger, ref bool ___twoHanded)
            {
                if (BetterLadders.Instance.configAllowTwoHanded.Value && ___hoveringOverTrigger != null && ___hoveringOverTrigger.isLadder && ___twoHanded)
                {
                            ___hoveringOverTrigger.twoHandedItemAllowed = true;
                            ___hoveringOverTrigger.specialCharacterAnimation = false;
                            ___hoveringOverTrigger.hidePlayerItem = true;
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
                if (playerController.isHoldingObject)
                {
                    if ((BetterLadders.Instance.configAllowTwoHanded.Value && BetterLadders.Instance.configHideTwoHanded.Value && playerController.twoHanded) || (BetterLadders.Instance.configHideOneHanded.Value && !playerController.twoHanded))
                    {
                        playerController.currentlyHeldObjectServer.EnableItemMeshes(enable: !___usingLadder);
                    }
                }
            }
        }
    }
}