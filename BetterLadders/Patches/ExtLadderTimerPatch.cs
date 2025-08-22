using HarmonyLib;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class ExtLadderTimerPatch
    {
        [HarmonyPatch(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.StartLadderAnimation))]
        [HarmonyPostfix]
        private static void StartLadderAnimation_Post(ref float ___ladderTimer)
        {
            ___ladderTimer = (LocalData.extensionTimer > 0.0f) ? (20.0f - LocalData.extensionTimer) : int.MinValue;
        }
    }
}