using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BetterLadders.Patches
{
    internal class HoldToPickup
    {
        static bool canPickupLadder = false;

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.BeginGrabObject))]
        static bool ControlExtLadderPickup(ref PlayerControllerB __instance)
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

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ClickHoldInteraction))]
        static bool ShowHoldInteractHUD(ref PlayerControllerB __instance)
        {
            if (__instance.hoveringOverTrigger)
            {
                bool isHoldingInteract = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").IsPressed();
                __instance.isHoldingInteract = isHoldingInteract;
                if (!isHoldingInteract)
                {
                    __instance.StopHoldInteractionOnTrigger();
                    return false;
                }
                if (__instance.hoveringOverTrigger == null || !__instance.hoveringOverTrigger.interactable)
                {
                    __instance.StopHoldInteractionOnTrigger();
                    return false;
                }
                if (__instance.hoveringOverTrigger == null || !__instance.hoveringOverTrigger.gameObject.activeInHierarchy || !__instance.hoveringOverTrigger.holdInteraction || __instance.hoveringOverTrigger.currentCooldownValue > 0f || (__instance.isHoldingObject && !__instance.hoveringOverTrigger.oneHandedItemAllowed) || (__instance.twoHanded && !__instance.hoveringOverTrigger.twoHandedItemAllowed))
                {
                    __instance.StopHoldInteractionOnTrigger();
                    return false;
                }
                if (__instance.isGrabbingObjectAnimation || __instance.isTypingChat || __instance.inSpecialInteractAnimation || __instance.throwingObject)
                {
                    __instance.StopHoldInteractionOnTrigger();
                    return false;
                }
                if (!HUDManager.Instance.HoldInteractionFill(__instance.hoveringOverTrigger.timeToHold, __instance.hoveringOverTrigger.timeToHoldSpeedMultiplier))
                {
                    __instance.hoveringOverTrigger.HoldInteractNotFilled();
                    return false;
                }
                __instance.hoveringOverTrigger.Interact(__instance.thisPlayerBody);
                return false;
            }
            else
            {
                bool isHoldingInteract = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").IsPressed();
                if (!isHoldingInteract)
                {
                    HUDManager.Instance.holdFillAmount = 0f;
                    return false;
                }
                if (LookingAtGrabbableExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
                {
                    if (extLadderObj != null && extLadderObj.ladderActivated)
                    {
                        if (!HUDManager.Instance.HoldInteractionFill(Config.Default.holdTime))
                        {
                            return false;
                        }
                        canPickupLadder = true;
                        __instance.BeginGrabObject();
                    }
                }
                return false;
            }
        }
        [HarmonyPrefix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StopHoldInteractionOnTrigger))]
        static bool StopHoldInteractionOnTrigger(ref PlayerControllerB __instance)
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
        static bool LookingAtGrabbableExtLadder(ref PlayerControllerB __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj)
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
