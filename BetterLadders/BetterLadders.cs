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
    [BepInPlugin(PLUGIN_GUID, NAME, VERSION)]
    public class BetterLadders : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "e3s1.BetterLadders", NAME = "BetterLadders", VERSION = "2.0.0";
        internal static ManualLogSource StaticLogger { get; private set; } = null!;

        internal static Config Settings { get; private set; } = null!;

        internal static Harmony Harmony { get; private set; } = null!;

        private void Awake()
        {
            // Initialize 'Config' and 'Harmony' instances.
            Settings = new(Config);
            Harmony = new(PLUGIN_GUID);
            // ...

            NetcodePatcher(); // Patches your netcode, patches your netcode, patches your netcode...

            // Apply all patches.
            Harmony.PatchAll(typeof(AllowTwoHandedPatch));
            Harmony.PatchAll(typeof(ClimbSpeedPatch));
            Harmony.PatchAll(typeof(HideItemsPatches));
            // ...

            StaticLogger.LogInfo($"{NAME} v{VERSION} loaded!");
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