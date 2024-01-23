using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BetterLadders.Patches
{
    internal class HoldToPickup
    {
        private static bool canPickupLadder = false;

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.BeginGrabObject))]
        private static bool ControlExtLadderPickup(ref PlayerControllerB __instance)
        {
            if (LookingAtGrabbableExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
            {
                if (extLadderObj != null && extLadderObj.ladderActivated)
                {
                    if (canPickupLadder)
                    {
                        canPickupLadder = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ClickHoldInteraction))]
        private static void ShowHoldInteractHUD(ref PlayerControllerB __instance)
        {
            if (!__instance.hoveringOverTrigger)
            {
                bool isHoldingInteract = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").IsPressed();
                if (!isHoldingInteract)
                {
                    HUDManager.Instance.holdFillAmount = 0f;
                    return;
                }
                if (LookingAtGrabbableExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
                {
                    if (extLadderObj != null && extLadderObj.ladderActivated)
                    {
                        if (!HUDManager.Instance.HoldInteractionFill(Config.Default.holdTime))
                        {
                            return;
                        }
                        canPickupLadder = true;
                        __instance.BeginGrabObject();
                    }
                }
            }
        }
        [HarmonyPrefix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StopHoldInteractionOnTrigger))]
        private static bool StopHoldInteractionOnTrigger(ref PlayerControllerB __instance)
        {
            if (LookingAtGrabbableExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
            {
                if (extLadderObj != null && extLadderObj.ladderActivated)
                {
                    bool isHoldingInteract = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").IsPressed();
                    if (isHoldingInteract) return false;
                    HUDManager.Instance.holdFillAmount = 0f;
                }
                else
                {
                    HUDManager.Instance.holdFillAmount = 0f;
                }
            }
            else
            {
                HUDManager.Instance.holdFillAmount = 0f;
            }
            if (__instance.previousHoveringOverTrigger != null)
            {
                __instance.previousHoveringOverTrigger.StopInteraction();
            }
            if (__instance.hoveringOverTrigger != null)
            {
                __instance.hoveringOverTrigger.StopInteraction();
            }
            return false;
        }
        private static bool LookingAtGrabbableExtLadder(ref PlayerControllerB __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj)
        {
            var interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
            bool success = (Physics.Raycast(interactRay, out hit, __instance.grabDistance, __instance.interactableObjectsMask) && hit.collider.gameObject.layer != 8);
            if (success)
            {
                extLadderObj = hit.collider.gameObject.GetComponent<ExtensionLadderItem>();
                if (__instance.twoHanded || (extLadderObj != null && !extLadderObj.grabbable)) return false;
            }
            else
            {
                extLadderObj = null;
            }
            return success;
        }
    }
}
