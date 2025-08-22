using Unity.Netcode;

namespace BetterLadders.Networking
{
    public struct BetterLaddersData : INetworkSerializable
    {
        public float climbSpeedMultiplier = 1.0f,
            climbSprintSpeedMultiplier = 1.0f,
            extensionTimer = 20.0f;
        // holdTime;

        public bool allowTwoHanded,
            scaleAnimationSpeed,
            hideOneHanded,
            hideTwoHanded,
            enableKillTrigger = true;
        // holdToPickup;

        public BetterLaddersData() { }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref climbSpeedMultiplier);
            serializer.SerializeValue(ref climbSprintSpeedMultiplier);

            serializer.SerializeValue(ref extensionTimer);
            // serializer.SerializeValue(ref holdTime);

            serializer.SerializeValue(ref allowTwoHanded);
            serializer.SerializeValue(ref scaleAnimationSpeed);
            serializer.SerializeValue(ref hideOneHanded);
            serializer.SerializeValue(ref hideTwoHanded);

            serializer.SerializeValue(ref enableKillTrigger);
            // serializer.SerializeValue(ref holdToPickup);
        }
    }
}