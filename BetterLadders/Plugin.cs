using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterLadders
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(GUID);

        public static Plugin Instance;
        
        public const string GUID = "e3s1.BetterLadders";
        public const string NAME = "BetterLadders";
        public const string VERSION = "1.3.0";
        internal static new ManualLogSource Logger { get; private set; }
        public static new Config Config { get; internal set; }
        void Awake()
        {
            Instance = this;

            Logger = base.Logger;
            Config = new(base.Config);

            Plugin.Logger.LogInfo($"{NAME} loaded");

            harmony.PatchAll();
        }

        //TODO
        //Transition speed multiplier
        //Extension ladder length
        //Extension ladder killTrigger
        //Sync animations

        [HarmonyPatch(typeof(ExtensionLadderItem))]
        public class ExtLadderUpdateTranspiler
        {
            [HarmonyPatch("Update")]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = new List<CodeInstruction>(instructions);
                if (Config.Instance.timeMultiplier > 0)
                {
                    for (int i = 0; i < code.Count; i++)
                    {
                        if (code[i].opcode == OpCodes.Ldc_R4 &&
                            ((float)code[i].operand == 15f || (float)code[i].operand == 20f))
                        {
                            float newLdcVal = (float)code[i].operand * Config.Instance.timeMultiplier;
                            code[i] = new CodeInstruction(OpCodes.Ldc_R4, newLdcVal);
                        }
                    }
                }
                else if (Config.Instance.timeMultiplier == 0) // if 0, permanent
                {
                    for (int i = 0; i < code.Count - 2; i++)
                    {
                        if (code[i].opcode == OpCodes.Ret &&
                            code[i + 2].opcode == OpCodes.Ldfld &&
                            code[i + 2].operand is FieldInfo fieldInfo &&
                            fieldInfo.DeclaringType == typeof(ExtensionLadderItem) &&
                            fieldInfo.Name == "ladderAnimationBegun" &&
                            fieldInfo.FieldType == typeof(bool) &&
                            code[i + 5].opcode == OpCodes.Ldarg_0)
                        {
                            code.Insert(i + 4, new CodeInstruction(OpCodes.Ret));
                        }
                    }
                }
                return code;
            }
        }

        static bool LookingAtExtLadder(ref PlayerControllerB __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj)
        {
            var interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
            bool success = (Physics.Raycast(interactRay, out hit, __instance.grabDistance, __instance.interactableObjectsMask) && hit.collider.gameObject.layer != 8);
            if (success)
            {
                extLadderObj = hit.collider.gameObject.GetComponent<ExtensionLadderItem>();
            } else
            {
                extLadderObj = null;
            }
            return success;
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class PlayerControllerBPatch
        {
            static bool canPickupLadder = false;

            [HarmonyPatch("BeginGrabObject")]
            [HarmonyPrefix]
            static bool ControlExtLadderPickup(ref PlayerControllerB __instance)
            {
                if (!Config.Default.holdToPickup) return true;
                if (LookingAtExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
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

            [HarmonyPatch("ClickHoldInteraction")]
            [HarmonyPrefix]
            static bool ShowHoldInteractHUDTest(ref PlayerControllerB __instance)
            {
                if (!Config.Default.holdToPickup) return true;
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
                    if (LookingAtExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
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
            [HarmonyPatch("StopHoldInteractionOnTrigger")]
            [HarmonyPrefix]
            static bool StopHoldInteractionOnTrigger(ref PlayerControllerB __instance)
            {
                if (!Config.Default.holdToPickup) return true;
                if (LookingAtExtLadder(ref __instance, out RaycastHit hit, out ExtensionLadderItem extLadderObj))
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