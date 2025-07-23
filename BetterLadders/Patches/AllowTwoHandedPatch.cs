using HarmonyLib;

namespace BetterLadders.Patches
{
    internal sealed class AllowTwoHandedPatch
    {
        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        private static void LadderTwoHandedAccessPatch(ref InteractTrigger ___hoveringOverTrigger, ref bool ___twoHanded)
        {
            if (BetterLadders.Settings.AllowTwoHanded.Value && ___hoveringOverTrigger != null && ___hoveringOverTrigger.isLadder && ___twoHanded)
            {
                ___hoveringOverTrigger.twoHandedItemAllowed = true;
                ___hoveringOverTrigger.specialCharacterAnimation = false;
                ___hoveringOverTrigger.hidePlayerItem = true;
            }
        }
    }
}