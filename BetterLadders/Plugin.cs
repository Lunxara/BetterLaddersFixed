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
        public static Plugin Instance;

        internal readonly Harmony harmony = new(GUID);

        public const string GUID = "e3s1.BetterLadders";
        public const string NAME = "BetterLadders";
        public const string VERSION = "1.4.3";

        internal static new ManualLogSource Logger { get; private set; }
        public static new Config Config { get; internal set; }
        private void Awake()
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
        //Synced transpilers are called inside of SyncedInstance, because the transpilers reference synced config values.
        //These patches have to be executed in the lobby, so they are done at lever pull for simplicity
        //probably breaks with late joining if players dont join as spectators

        internal void PatchSyncedTranspilers()
        {
            harmony.PatchAll(typeof(ExtLadderTime));
            harmony.PatchAll(typeof(KillTrigger));
            harmony.PatchAll(typeof(TransitionSpeed));
        }
        internal void UnpatchSyncedTranspilers()
        {
            // ExtLadderTime
            harmony.Unpatch(AccessTools.Method(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.Update)),           HarmonyPatchType.Transpiler, harmony.Id);
            // KillTrigger
            harmony.Unpatch(AccessTools.Method(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.LadderAnimation)),  HarmonyPatchType.Transpiler, harmony.Id);
            // TransitionSpeed
            harmony.Unpatch(AccessTools.Method(typeof(InteractTrigger),     nameof(InteractTrigger.ladderClimbAnimation)), HarmonyPatchType.Transpiler, harmony.Id);
        }

        internal void TranspilerLogger(List<CodeInstruction> code, int i, int startRelative, int endRelative, string name)
        {
            if (!Config.Instance.debugMode) return;
            Plugin.Logger.LogInfo($"Found code match in {name}");
            for (int j = startRelative; j < endRelative; j++) {
                Logger.LogInfo(code[i + j]);
            }
            Plugin.Logger.LogInfo("============================================================");
        }

        //TODO
        //Fix reserved flashlight not being hidden
        //Disable walkie glow, LethalThings' rocket launcher laser, flashlight on hide
        //Ext ladder length - scale or by segments? seems like a lot of work to remake the extension ladder to work with segments
        //Ext ladder extend, fall, retract speed
        //Stackable extension ladders
        //Fix vanilla bug where player isn't alligned to ladder when interacting from behind - effect gets worse the higher transitionSpeedMultiplier is
        //Invisible walkway at top so player doesn't get stuck on ladder - not sure if possible while keeping mod clientside unless it can be attached in game rather than modifying the class
    }
}