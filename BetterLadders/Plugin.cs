using BepInEx;
using BepInEx.Logging;
using BetterLadders.Patches;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace BetterLadders
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal readonly Harmony harmony = new(GUID);

        public static Plugin Instance;

        public const string GUID = "e3s1.BetterLadders";
        public const string NAME = "BetterLadders";
        public const string VERSION = "1.4.0";
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
        //All transpilers are patched inside of SyncedInstance, because they reference synced config values.
        //These patches have to be executed after the game starts
        internal bool transpilersArePatching;
        internal void StartSyncedPatches(bool patching)
        {
            Plugin.Instance.transpilersArePatching = patching;
            //Don't need to run transpilers if they are at vanilla values
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

        //HIGH PRIORITY
        //Only send needed config values during sync
        //Check performance vs vanilla
        //Fix other players' ladder animation not playing
        //If a player dies/disconnects while holding an object, it should be set to visible again for all clients
        //Sync animation speed - transpiler to reduce amount of messages?

        //NEW FEATURES
        //Fix reserved flashlight not being hidden
        //Option to, instead of hiding items, move them to hip for one handed, and to back for two handed
        //Disable walkie glow, LethalThings' rocket launcher laser, flashlight on hide/move
        //Press interact once to teleport to the top of the ladder?
        //Ext ladder length - scale or by segments? seems like a lot of work to remake the extension ladder to work with segments
        //Ext ladder deploy speed
        //Stackable extension ladders
        //Hitting with shovel instantly retracts ladder
        //Fix vanilla bug where player isn't alligned to ladder
        //Invisible walkway at top so player doesn't get stuck on ladder - not sure if possible while keeping mod clientside, since ExtensionLadder : GrabbableObject : NetworkBehaviour

        //DONE
        //Make sure synced patches are only run once per session
        //Fix bar still filling up when trying to pick up ladder with two handed object
        //Transition speed multiplier
        //Sync item visibility
        //Dont play animation when holding strafe keys
        //Ext ladder killTrigger toggle
        //Unpatch synced patches on disconnect
    }
}