using BepInEx;
using BepInEx.Logging;
using BetterLadders.Patches;
using HarmonyLib;
using System.Collections.Generic;

namespace BetterLadders
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal readonly Harmony harmony = new(GUID);

        public static Plugin Instance;

        public const string GUID = "e3s1.BetterLadders";
        public const string NAME = "BetterLadders";
        public const string VERSION = "1.4.2";
        internal static new ManualLogSource Logger { get; private set; }
        public static new Config Config { get; internal set; }
        void Awake()
        {
            Instance = this;

            Logger = base.Logger;
            Config = new(base.Config);

            harmony.PatchAll(typeof(SyncedInstance<Config>));
            harmony.PatchAll(typeof(AllowTwoHanded));
            harmony.PatchAll(typeof(ClimbSpeed));
            harmony.PatchAll(typeof(HideItems));
            harmony.PatchAll(typeof(HoverTip));
            if (Config.Default.holdToPickup) harmony.PatchAll(typeof(HoldToPickup)); //Overwrites original, so only patch if necessary
            Plugin.Logger.LogInfo($"{NAME} loaded");
        }
        //StartSyncedPatches is called inside of SyncedInstance, because the transpilers reference synced config values.
        //These patches have to be executed after the game starts
        internal bool transpilersArePatching;
        internal void StartSyncedPatches(bool patching)
        {
            Plugin.Instance.transpilersArePatching = patching;
            harmony.PatchAll(typeof(ExtLadderTime));
            harmony.PatchAll(typeof(KillTrigger));
            harmony.PatchAll(typeof(TransitionSpeed));
            SyncedInstance<Config>.StartedSyncedPatches = Plugin.Instance.transpilersArePatching;
        }

        internal void TranspilerLogger(List<CodeInstruction> code, int i, int startRelative, int endRelative, string name)
        {
            Plugin.Logger.LogInfo($"Found code match in {name}");
            for (int j = startRelative; j < endRelative; j++) {
                Logger.LogInfo(code[i + j]);
            }
            Plugin.Logger.LogInfo("============================================================");
        }

        //TODO

        //Only send needed config values during sync

        //NEW FEATURES
        //Fix reserved flashlight not being hidden
        //Disable walkie glow, LethalThings' rocket launcher laser, flashlight on hide
        //Press interact once to teleport to the top of the ladder?
        //Ext ladder length - scale or by segments? seems like a lot of work to remake the extension ladder to work with segments
        //Ext ladder deploy speed
        //Stackable extension ladders
        //Hitting with shovel instantly retracts ladder
        //Fix vanilla bug where player isn't alligned to ladder when interacting from behind
        //Invisible walkway at top so player doesn't get stuck on ladder - not sure if possible while keeping mod clientside, since ExtensionLadder : GrabbableObject : NetworkBehaviour
    }
}