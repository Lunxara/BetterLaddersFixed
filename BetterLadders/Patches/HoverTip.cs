using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders.Patches
{
    internal class HoverTip
    {
        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SetHoverTipAndCurrentInteractTrigger))]
        private static void LadderHandsFullTipPatch(ref PlayerControllerB __instance, ref InteractTrigger ___hoveringOverTrigger, ref bool ___isHoldingInteract, ref bool ___twoHanded)
        {
            if (Config.Instance.allowTwoHanded && ___hoveringOverTrigger != null && ___isHoldingInteract && ___twoHanded && ___hoveringOverTrigger.isLadder)
            {
                __instance.cursorTip.text = ___hoveringOverTrigger.hoverTip.Replace("[LMB]", "[E]"); // game doesnt replace interact tooltips with the right keybind so i wont either
            }
        }
    }
}
