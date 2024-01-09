using GameNetcodeStuff;
using HarmonyLib;

namespace BetterLadders.Patches
{
    internal class AllowTwoHanded
    {
        [HarmonyPrefix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Interact_performed))]
        static void LadderTwoHandedAccessPatch(ref InteractTrigger ___hoveringOverTrigger, ref bool ___twoHanded)
        {
            if (Config.Instance.allowTwoHanded && ___hoveringOverTrigger != null && ___hoveringOverTrigger.isLadder && ___twoHanded)
            {
                ___hoveringOverTrigger.twoHandedItemAllowed = true;
                ___hoveringOverTrigger.specialCharacterAnimation = false;
            }
        }
    }
}
