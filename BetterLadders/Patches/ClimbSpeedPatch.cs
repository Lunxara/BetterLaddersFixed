using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class ClimbSpeedPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void LadderClimbSpeedPatch(ref bool ___isSprinting, ref float ___climbSpeed, ref bool ___isClimbingLadder, ref PlayerControllerB __instance)
        {
            if (___isClimbingLadder)
            {
                float finalClimbSpeed;
                float animationMultiplier;
                if (___isSprinting)
                {
                    // Vanilla climb speed is 4.0f
                    finalClimbSpeed = 4.0f * BetterLadders.Settings.ClimbSpeedMultiplier.Value * BetterLadders.Settings.ClimbSprintSpeedMultiplier.Value;
                }
                else
                {
                    finalClimbSpeed = 4.0f * BetterLadders.Settings.ClimbSpeedMultiplier.Value;
                }
                animationMultiplier = finalClimbSpeed / 4.0f;
                ___climbSpeed = finalClimbSpeed;
                if (BetterLadders.Settings.ScaleAnimationSpeed.Value && __instance.playerBodyAnimator.GetFloat("animationSpeed") != 0f) // animationSpeed is set to 0f when the player stops moving
                {
                    __instance.playerBodyAnimator.SetFloat("animationSpeed", animationMultiplier);
                }
            }
        }
    }
}