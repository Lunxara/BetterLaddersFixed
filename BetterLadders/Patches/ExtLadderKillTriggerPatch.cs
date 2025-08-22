using HarmonyLib;
using UnityEngine;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class ExtLadderKillTriggerPatch
    {
        [HarmonyPatch(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.StartLadderAnimation))]
        [HarmonyPrefix]
        private static void StartLadderAnimation_Pre(Collider ___killTrigger)
        {
            if (___killTrigger != null && LocalData.enableKillTrigger != ___killTrigger.gameObject.activeInHierarchy)
            {
                ___killTrigger.gameObject.SetActive(LocalData.enableKillTrigger);
            }
        }
    }
}