using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders.Patches
{
    internal class HideItems
    {
        private static bool hasUsedLadder = false; // This exists to prevent sending network messages unnecessarily
        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SwitchToItemSlot))]
        private static void SetVisibilityOnItemSwitch(ref PlayerControllerB __instance)
        {
            SetVisibility(ref __instance.isClimbingLadder);
            // This doesn't affect anything in vanilla since you can't switch slots while climbing a ladder.
            // If a mod that allows switching slots via keybinds is installed, this prevents items from appearing when using them.
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.SetUsingLadderOnLocalClient))]
        private static void SetVisibilityOnStartClimb(ref bool ___usingLadder)
        {
            hasUsedLadder = true;
            SetVisibility(ref ___usingLadder);
        }

        internal static void SetVisibility(ref bool ___usingLadder)
        {
            if (!hasUsedLadder) return;
            PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
            if (playerController.currentlyHeldObjectServer != null) // null check is for reserved slot mods
            {
                if ((Config.Instance.allowTwoHanded && playerController.twoHanded) || (!playerController.twoHanded))
                {
                    Config.RequestHideItem(!___usingLadder); // If the local player has hideOneHanded/hideTwoHanded disabled, that shouldn't affect visibility on other clients
                    Plugin.Logger.LogInfo("Sending request to other clients to hide held item");
                    if ((playerController.twoHanded && Config.Default.hideTwoHanded) || (!playerController.twoHanded && Config.Default.hideOneHanded))
                    {
                        Plugin.Logger.LogInfo("Hiding held item");
                        playerController.currentlyHeldObjectServer.EnableItemMeshes(!___usingLadder);
                        hasUsedLadder = false;
                    }
                }
            }
        }
    }
}
