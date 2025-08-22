using BetterLadders.Networking;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class ClimbSpeedPatch
    {
        internal static readonly int animationSpeedID = Animator.StringToHash("animationSpeed");
        private static float initialClimbSpeed = 3.0f, initialAnimationSpeed = 1.0f;

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.SetUsingLadderOnLocalClient))]
        [HarmonyPostfix]
        private static void SetUsingLadderOnLocalClient_Post(bool isUsing)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            if (!isUsing)
            {
                ResetPlayerAnimationSpeed(player);

                return;
            }

            initialClimbSpeed = player.climbSpeed;
            initialAnimationSpeed = player.playerBodyAnimator.GetFloat(animationSpeedID);

            InputAction sprintAction = player.playerActions.m_Movement_Sprint;
            sprintAction.started += OnPlayerBeginSprint;
            sprintAction.canceled += OnPlayerStopSprint;

            SetPlayerAnimationSpeed(player, player.isSprinting);
        }

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.CancelLadderAnimation))]
        [HarmonyPostfix]
        private static void CancelLadderAnimation_Post()
        {
            ResetPlayerAnimationSpeed(GameNetworkManager.Instance.localPlayerController);
        }

        private static void OnPlayerBeginSprint(InputAction.CallbackContext context)
        {
            SetPlayerAnimationSpeed(GameNetworkManager.Instance.localPlayerController, isSprinting: true);
        }

        private static void OnPlayerStopSprint(InputAction.CallbackContext context)
        {
            SetPlayerAnimationSpeed(GameNetworkManager.Instance.localPlayerController, isSprinting: false);
        }

        private static void SetPlayerAnimationSpeed(PlayerControllerB player, bool isSprinting)
        {
            player.climbSpeed = initialClimbSpeed * LocalData.climbSpeedMultiplier * (isSprinting ? LocalData.climbSprintSpeedMultiplier : 1.0f);

            if (LocalData.scaleAnimationSpeed)
            {
                float animationSpeed = player.climbSpeed / initialClimbSpeed;
                player.playerBodyAnimator.SetFloat(animationSpeedID, animationSpeed);

                if (BetterLaddersNetworker.Instance != null && BetterLaddersNetworker.Instance.IsSpawned)
                {
                    BetterLaddersNetworker.Instance.SetAnimationSpeedServerRpc(player, animationSpeed);
                }
            }
        }

        private static void ResetPlayerAnimationSpeed(PlayerControllerB player)
        {
            player.climbSpeed = initialClimbSpeed;
            player.playerBodyAnimator.SetFloat(animationSpeedID, initialAnimationSpeed);

            if (BetterLaddersNetworker.Instance != null && BetterLaddersNetworker.Instance.IsSpawned)
            {
                BetterLaddersNetworker.Instance.SetAnimationSpeedServerRpc(player, initialAnimationSpeed);
            }

            InputAction sprintAction = player.playerActions.m_Movement_Sprint;
            sprintAction.started -= OnPlayerBeginSprint;
            sprintAction.canceled -= OnPlayerStopSprint;
        }
    }
}