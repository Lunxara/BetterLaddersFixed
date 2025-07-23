using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace BetterLadders
{
    /// <summary>
    ///     Class containing and defining various configuration options.
    /// </summary>
    public class Config
    {
        public ConfigEntry<float> ClimbSpeedMultiplier { get; private set; }
        public ConfigEntry<float> ClimbSprintSpeedMultiplier { get; private set; }
        public ConfigEntry<bool> AllowTwoHanded { get; private set; }
        public ConfigEntry<bool> ScaleAnimationSpeed { get; private set; }
        public ConfigEntry<bool> HideOneHanded { get; private set; }
        public ConfigEntry<bool> HideTwoHanded { get; private set; }

        /// <summary>
        ///     Constructor for initializing plugin configuration.
        /// </summary>
        /// <param name="cfg">BepInEx configuration file.</param>
        public Config(ConfigFile cfg)
        {
            // Disable saving config after a call to 'Bind()' is made.
            cfg.SaveOnConfigSet = false;

            // Bind config entries to the config file.
            ClimbSpeedMultiplier = cfg.Bind("General", "climbSpeedMultiplier", 1.0f, "Ladder climb speed multiplier");
            ClimbSprintSpeedMultiplier = cfg.Bind("General", "sprintingClimbSpeedMultiplier", 1.5f, "Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier");
            AllowTwoHanded = cfg.Bind("General", "allowTwoHanded", true, "Whether to allow using ladders while carrying a two-handed object");
            ScaleAnimationSpeed = cfg.Bind("General", "scaleAnimationSpeed", true, "Whether to scale the speed of the climbing animation to the climbing speed");
            HideOneHanded = cfg.Bind("General", "hideOneHanded", true, "Whether to hide one-handed items while climbing a ladder - false in vanilla");
            HideTwoHanded = cfg.Bind("General", "hideTwoHanded", true, "Whether to hide two-handed items while climbing a ladder");
            // ...

            // Remove old config settings.
            ClearOrphanedEntries(cfg);

            // Re-enable saving and save config.
            cfg.SaveOnConfigSet = true;
            cfg.Save();
        }

        /// <summary>
        ///     Remove old (orphaned) configuration entries.
        /// </summary>
        /// <remarks>Obtained from: https://lethal.wiki/dev/intermediate/custom-configs#better-configuration</remarks>
        /// <param name="config">The config file to clear orphaned entries from.</param>
        private static void ClearOrphanedEntries(ConfigFile config)
        {
            // Obtain 'OrphanedEntries' dictionary from ConfigFile through reflection.
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            Dictionary<ConfigDefinition, string>? orphanedEntries = (Dictionary<ConfigDefinition, string>?)orphanedEntriesProp.GetValue(config);

            // Clear orphaned entries.
            orphanedEntries?.Clear();
        }
    }
}