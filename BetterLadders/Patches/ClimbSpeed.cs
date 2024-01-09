using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;

namespace BetterLadders.Patches
{
    internal class ClimbSpeed
    {
        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        static void LadderClimbSpeedPatch(ref bool ___isSprinting, ref float ___climbSpeed, ref bool ___isClimbingLadder, ref PlayerControllerB __instance)
        {
            if (!___isClimbingLadder) return;
            // vanilla climb speed is 4.0f
            float finalClimbSpeed = 4.0f * Config.Instance.climbSpeedMultiplier * (___isSprinting ? Config.Instance.sprintingClimbSpeedMultiplier : 1f);
            ___climbSpeed = finalClimbSpeed;

            if (__instance.playerBodyAnimator.GetFloat("animationSpeed") != 0f) // animationSpeed is set to 0f when the player stops moving
            {
                // Checking if moveInputVector.y != 0 before setting animation speed fixes the vanilla bug where holding move left/right will still play the climbing animation
                if (__instance.moveInputVector.y != 0f && Config.Default.scaleAnimationSpeed)
                {
                    __instance.playerBodyAnimator.SetFloat("animationSpeed", finalClimbSpeed / 4.0f);
                } else if (__instance.moveInputVector.y == 0f)
                {   
                    __instance.playerBodyAnimator.SetFloat("animationSpeed", 0f);
                }
            }
        }
    }
}
