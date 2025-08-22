using BetterLadders.Networking;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class HideItemsPatches
    {
        /* [HarmonyPatch(typeof(PlayerControllerB), "SwitchToItemSlot")]
        [HarmonyPostfix]
        private static void LadderHeldItemVisibilityPatch(ref PlayerControllerB __instance)
        {
            SetVisibility(ref __instance.isClimbingLadder);
            // This doesn't affect anything in vanilla since you can't switch slots while climbing a ladder.
            // If a mod that allows switching slots via keybinds is installed, this prevents items from appearing when using them.
        } */

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.SetUsingLadderOnLocalClient))]
        [HarmonyPostfix]
        private static void LadderHeldItemVisibilityPatch(bool isUsing)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            if (player.isHoldingObject && player.currentlyHeldObjectServer != null)
            {
                if ((LocalData.hideOneHanded && !player.twoHanded) || (LocalData.allowTwoHanded && LocalData.hideTwoHanded && player.twoHanded))
                {
                    player.currentlyHeldObjectServer.EnableItemMeshes(!isUsing);

                    if (BetterLaddersNetworker.Instance != null && BetterLaddersNetworker.Instance.IsSpawned)
                    {
                        BetterLaddersNetworker.Instance.HideItemServerRpc(player.currentlyHeldObjectServer, !isUsing);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.Start))]
        [HarmonyPostfix]
        private static void LadderStart_Post(InteractTrigger __instance)
        {
            if (__instance.isLadder)
            {
                __instance.hidePlayerItem = LocalData.hideOneHanded && LocalData.hideTwoHanded;
            }
        }

        internal static void RefreshAllLadders(bool hideItem)
        {
            InteractTrigger[] allTriggers = Object.FindObjectsByType<InteractTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < allTriggers.Length; i++)
            {
                if (allTriggers[i].isLadder)
                {
                    allTriggers[i].hidePlayerItem = hideItem;
                }
            }
        }
    }
}