using BetterLadders.Networking;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class AnimationSpeedPatch
    {
        private static readonly int animationSpeedID = Animator.StringToHash("animationSpeed");

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.SetUsingLadderOnLocalClient))]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPostfix]
        private static void SetUsingLadderOnLocalClient_Post(bool isUsing)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            player.StopCoroutine(ScalePlayerAnimationSpeed(player));

            if (!isUsing)
            {
                if (BetterLaddersNetworker.Instance != null && BetterLaddersNetworker.Instance.IsSpawned)
                {
                    BetterLaddersNetworker.Instance.SetAnimationSpeedServerRpc(player, stop: true);
                }

                return;
            }

            _ = player.StartCoroutine(ScalePlayerAnimationSpeed(player));

            if (BetterLaddersNetworker.Instance != null && BetterLaddersNetworker.Instance.IsSpawned)
            {
                BetterLaddersNetworker.Instance.SetAnimationSpeedServerRpc(player, stop: false);
            }
        }

        internal static IEnumerator ScalePlayerAnimationSpeed(PlayerControllerB player)
        {
            bool isLocalPlayer = player.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId;

            float startTime = Time.realtimeSinceStartup;

            // Wait for player to begin climbing the ladder, or time out after 5 seconds.
            yield return new WaitUntil(() => (player != null && player.isClimbingLadder) || Time.realtimeSinceStartup - startTime > 5.0f);

            if (player == null || !player.isClimbingLadder)
            {
                BetterLadders.StaticLogger.LogWarning("Timed out when syncing ladder animation speed.");

                yield break;
            }

            yield return new WaitWhile(() =>
            {
                if (player == null || player.playerBodyAnimator == null)
                {
                    return false;
                }

                float animationSpeed = player.playerBodyAnimator.GetFloat(animationSpeedID);

                if (animationSpeed != 0.0f)
                {
                    bool isSprinting = isLocalPlayer ? player.isSprinting : player.playerBodyAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Sprinting");

                    animationSpeed = LocalData.climbSpeedMultiplier * (isSprinting ? LocalData.climbSprintSpeedMultiplier : 1.0f);

                    player.playerBodyAnimator.SetFloat(animationSpeedID, animationSpeed);
                }

                return player.isClimbingLadder;
            });
        }
    }
}