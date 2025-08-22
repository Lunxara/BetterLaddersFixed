using BepInEx;
using BepInEx.Logging;
using BetterLadders.Patches;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BetterLadders
{
    /// <summary>
    ///     Configurable climbing speed, extension ladder time, and climbing with two-handed items.
    /// </summary>
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class BetterLadders : BaseUnityPlugin
    {
        /// <summary>
        ///     General plugin properties.
        /// </summary>
        public const string PLUGIN_GUID = "e3s1.BetterLadders", PLUGIN_NAME = "BetterLadders", PLUGIN_VERSION = "2.0.0";

        internal static ManualLogSource StaticLogger { get; private set; } = null!;
        internal static Config Settings { get; private set; } = null!;
        internal static Harmony Harmony { get; private set; } = null!;

        private void Awake()
        {
            StaticLogger = Logger;

            try
            {
                // Initialize 'Config' and 'Harmony' instances.
                Settings = new(Config);
                Harmony = new(PLUGIN_GUID);
                // ...

                NetcodePatcher(); // Patches your netcode, patches your netcode, patches your netcode...
                Harmony.PatchAll(typeof(NetworkingInitPatches));

                // Apply all other patches.
                Harmony.PatchAll(typeof(AllowTwoHandedPatch));
                Harmony.PatchAll(typeof(ClimbSpeedPatch));
                // Harmony.PatchAll(typeof(ExtLadderHoldPatch));
                Harmony.PatchAll(typeof(ExtLadderKillTriggerPatch));
                Harmony.PatchAll(typeof(ExtLadderTimerPatch));
                Harmony.PatchAll(typeof(HideItemsPatches));
                // ...

                StaticLogger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded!");
            }
            catch (Exception e)
            {
                StaticLogger.LogError($"Error while initializing '{PLUGIN_NAME}': {e}");
            }
        }

        private static void NetcodePatcher()
        {
            Type[] types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = [.. e.Types.Where(type => type != null)];
            }

            foreach (Type type in types)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length > 0)
                    {
                        _ = method.Invoke(null, null);
                    }
                }
            }
        }
    }
}