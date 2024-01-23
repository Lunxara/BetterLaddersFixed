using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders.Patches
{
    internal class ClimbSpeed
    {
        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        private static void LadderClimbSpeedPatch(ref bool ___isSprinting, ref float ___climbSpeed, ref bool ___isClimbingLadder, ref PlayerControllerB __instance)
        {
            if (!___isClimbingLadder) return;
            // vanilla climb speed is 4.0f
            float finalClimbSpeed = 4.0f * Config.Instance.climbSpeedMultiplier * (___isSprinting ? Config.Instance.sprintingClimbSpeedMultiplier : 1f);
            ___climbSpeed = finalClimbSpeed;

            // Checking if moveInputVector.y != 0 before setting animation speed fixes the vanilla bug where holding move left/right will still play the climbing animation
            if (__instance.moveInputVector.y != 0f && Config.Instance.scaleAnimationSpeed)
            {
                float finalAnimationSpeed = finalClimbSpeed / 4.0f * (__instance.moveInputVector.y > 0f ? 1f : -1f);
                if (__instance.currentAnimationSpeed != finalAnimationSpeed)
                {
                    __instance.previousAnimationSpeed = finalAnimationSpeed;
                    __instance.currentAnimationSpeed = finalAnimationSpeed;
                    Plugin.Logger.LogInfo($"Setting animationSpeed to {finalAnimationSpeed}");
                    __instance.playerBodyAnimator.SetFloat("animationSpeed", finalAnimationSpeed);
                }
            }
            else if (__instance.moveInputVector.y == 0f)
            {
                if (__instance.currentAnimationSpeed != 0f)
                {
                    __instance.previousAnimationSpeed = 0f;
                    __instance.currentAnimationSpeed = 0f;
                    Plugin.Logger.LogInfo("Setting animationSpeed to 0");
                    __instance.playerBodyAnimator.SetFloat("animationSpeed", 0f);
                }
            }
        }
    }
}
