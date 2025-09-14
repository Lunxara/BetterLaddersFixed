using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;

using static BetterLadders.Config;

namespace BetterLadders.Patches
{
    [HarmonyPatch]
    internal sealed class ClimbSpeedPatch
    {
        internal static float initialClimbSpeed = 4.0f;

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.SetUsingLadderOnLocalClient))]
        [HarmonyPostfix]
        private static void SetUsingLadderOnLocalClient_Post(bool isUsing)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            if (!isUsing)
            {
                ResetPlayerClimbSpeed(player);

                return;
            }

            initialClimbSpeed = player.climbSpeed;

            InputAction sprintAction = player.playerActions.m_Movement_Sprint;
            sprintAction.started += OnPlayerBeginSprint;
            sprintAction.canceled += OnPlayerStopSprint;

            SetPlayerClimbSpeed(player, player.isSprinting);
        }

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.CancelLadderAnimation))]
        [HarmonyPostfix]
        private static void CancelLadderAnimation_Post()
        {
            ResetPlayerClimbSpeed(GameNetworkManager.Instance.localPlayerController);
        }

        private static void OnPlayerBeginSprint(InputAction.CallbackContext context)
        {
            SetPlayerClimbSpeed(GameNetworkManager.Instance.localPlayerController, isSprinting: true);
        }

        private static void OnPlayerStopSprint(InputAction.CallbackContext context)
        {
            SetPlayerClimbSpeed(GameNetworkManager.Instance.localPlayerController, isSprinting: false);
        }

        private static void SetPlayerClimbSpeed(PlayerControllerB player, bool isSprinting)
        {
            player.climbSpeed = initialClimbSpeed * LocalData.climbSpeedMultiplier * (isSprinting ? LocalData.climbSprintSpeedMultiplier : 1.0f);
        }

        private static void ResetPlayerClimbSpeed(PlayerControllerB player)
        {
            player.climbSpeed = initialClimbSpeed;

            InputAction sprintAction = player.playerActions.m_Movement_Sprint;
            sprintAction.started -= OnPlayerBeginSprint;
            sprintAction.canceled -= OnPlayerStopSprint;
        }
    }
}