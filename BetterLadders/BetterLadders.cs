using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

namespace BetterLadders
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BetterLadders : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        public static BetterLadders Instance;

        private ConfigEntry<float> configClimbSpeedMultiplier;
        private ConfigEntry<bool> configAllowTwoHanded;

        private void Awake()
        {
            Instance = this;
            configClimbSpeedMultiplier = Config.Bind("General", "climbSpeedMultipler", 1.0f, "Ladder climb speed multiplier");
            configAllowTwoHanded = Config.Bind("General", "allowTwoHanded", true, "Whether to allow using ladders while carrying a two-handed object");


            // Plugin startup logic
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded!");

            harmony.PatchAll(typeof(BetterLadders));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(InteractTriggerPatch));
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class PlayerControllerBPatch
        {
            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            private static void LadderClimbSpeedPatch(ref float ___climbSpeed)
            {
                ___climbSpeed *= BetterLadders.Instance.configClimbSpeedMultiplier.Value;
            }
            
            [HarmonyPatch("Interact_performed")]
            [HarmonyPrefix]
            private static void LadderTwoHandedAccessPatch(ref InteractTrigger ___hoveringOverTrigger, ref bool ___twoHanded)
            {
                if (BetterLadders.Instance.configAllowTwoHanded.Value && ___hoveringOverTrigger != null)
                {
                    if (___hoveringOverTrigger.isLadder && ___twoHanded)
                    {
                        ___hoveringOverTrigger.twoHandedItemAllowed = true;
                        ___hoveringOverTrigger.specialCharacterAnimation = false;
                        ___hoveringOverTrigger.hidePlayerItem = true;
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
                    GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.EnableItemMeshes(enable: !___usingLadder);
                }
            }
        }
    }
}