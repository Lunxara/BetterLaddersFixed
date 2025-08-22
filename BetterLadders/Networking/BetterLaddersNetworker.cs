using BetterLadders.Patches;
using GameNetcodeStuff;
using Unity.Netcode;

namespace BetterLadders.Networking
{
    public sealed class BetterLaddersNetworker : NetworkBehaviour
    {
        public static BetterLaddersNetworker? Instance { get; private set; }

        public NetworkVariable<BetterLaddersData> SyncedData { get; private set; } = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Instance = this;

            SyncedData.OnValueChanged += (previousData, newData) =>
            {
                Config.LocalData = newData;

                if (previousData.allowTwoHanded != newData.allowTwoHanded)
                {
                    AllowTwoHandedPatch.RefreshAllLadders(newData.allowTwoHanded);
                }

                if ((previousData.hideOneHanded != newData.hideOneHanded) || (previousData.hideTwoHanded != newData.hideTwoHanded))
                {
                    HideItemsPatches.RefreshAllLadders(newData.hideOneHanded && newData.hideTwoHanded);
                }
            };

            RefreshSyncedData();

            Config.LocalData = SyncedData.Value;
        }

        public override void OnNetworkDespawn()
        {
            Instance = null;

            base.OnNetworkDespawn();
        }

        public void RefreshSyncedData()
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            SyncedData.Value = new()
            {
                climbSpeedMultiplier = BetterLadders.Settings.ClimbSpeedMultiplier.Value,
                climbSprintSpeedMultiplier = BetterLadders.Settings.ClimbSprintSpeedMultiplier.Value,

                extensionTimer = BetterLadders.Settings.ExtensionTimer.Value,
                // holdTime = BetterLadders.Settings.HoldTime.Value,

                allowTwoHanded = BetterLadders.Settings.AllowTwoHanded.Value,
                scaleAnimationSpeed = BetterLadders.Settings.ScaleAnimationSpeed.Value,
                hideOneHanded = BetterLadders.Settings.HideOneHanded.Value,
                hideTwoHanded = BetterLadders.Settings.HideTwoHanded.Value,

                enableKillTrigger = BetterLadders.Settings.EnableKillTrigger.Value,
                // holdToPickup = BetterLadders.Settings.HoldToPickup.Value
            };
        }

        [ServerRpc(RequireOwnership = false)]
        public void HideItemServerRpc(NetworkBehaviourReference itemReference, bool hide)
        {
            HideItemClientRpc(itemReference, hide);
        }

        [ClientRpc]
        public void HideItemClientRpc(NetworkBehaviourReference itemReference, bool hide)
        {
            if (itemReference.TryGet(out GrabbableObject item) && item.playerHeldBy != null
                && item.playerHeldBy.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                item.EnableItemMeshes(hide);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetAnimationSpeedServerRpc(NetworkBehaviourReference playerReference, float animationSpeed)
        {
            SetAnimationSpeedClientRpc(playerReference, animationSpeed);
        }

        [ClientRpc]
        public void SetAnimationSpeedClientRpc(NetworkBehaviourReference playerReference, float animationSpeed)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && player.playerBodyAnimator.GetFloat(ClimbSpeedPatch.animationSpeedID)
                != animationSpeed && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                player.playerBodyAnimator.SetFloat(ClimbSpeedPatch.animationSpeedID, animationSpeed);
            }
        }
    }
}