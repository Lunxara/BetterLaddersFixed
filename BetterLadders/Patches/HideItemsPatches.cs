using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class HideItemsPatches
    {
        [HarmonyPatch(typeof(PlayerControllerB), "SwitchToItemSlot")]
        [HarmonyPostfix]
        private static void LadderHeldItemVisibilityPatch(ref PlayerControllerB __instance)
        {
            SetVisibility(ref __instance.isClimbingLadder);
            // This doesn't affect anything in vanilla since you can't switch slots while climbing a ladder.
            // If a mod that allows switching slots via keybinds is installed, this prevents items from appearing when using them.
        }

        [HarmonyPatch(typeof(InteractTrigger), "SetUsingLadderOnLocalClient")]
        [HarmonyPostfix]
        private static void LadderHeldItemVisibilityPatch(ref bool ___usingLadder)
        {
            SetVisibility(ref ___usingLadder);
        }

        private static void SetVisibility(ref bool ___usingLadder)
        {
            PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

            if (playerController.isHoldingObject)
            {
                if ((BetterLadders.Settings.AllowTwoHanded.Value && BetterLadders.Settings.HideTwoHanded.Value && playerController.twoHanded)
                    || (BetterLadders.Settings.HideOneHanded.Value && !playerController.twoHanded))
                {
                    playerController.currentlyHeldObjectServer.EnableItemMeshes(enable: !___usingLadder);
                }
            }
        }
    }
}