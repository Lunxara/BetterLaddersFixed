using HarmonyLib;
using UnityEngine;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class AllowTwoHandedPatch
    {
        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.Start))]
        [HarmonyPostfix]
        private static void LadderStart_Post(InteractTrigger __instance)
        {
            if (__instance.isLadder && !__instance.twoHandedItemAllowed && __instance.specialCharacterAnimation)
            {
                ModifyLadder(__instance, LocalData.allowTwoHanded);
            }
        }

        internal static void RefreshAllLadders(bool allowTwoHanded)
        {
            InteractTrigger[] allTriggers = Object.FindObjectsByType<InteractTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < allTriggers.Length; i++)
            {
                if (allTriggers[i].isLadder)
                {
                    ModifyLadder(allTriggers[i], allowTwoHanded);
                }
            }
        }

        private static void ModifyLadder(InteractTrigger ladderTrigger, bool allowTwoHanded)
        {
            ladderTrigger.twoHandedItemAllowed = allowTwoHanded;
            ladderTrigger.specialCharacterAnimation = !allowTwoHanded;
        }
    }
}