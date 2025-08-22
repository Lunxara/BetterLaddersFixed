using BepInEx.Configuration;
using BetterLadders.Networking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BetterLadders
{
    /// <summary>
    ///     Class containing and defining various configuration options.
    /// </summary>
    public class Config
    {
        public static BetterLaddersData LocalData { get; internal set; }

        /// <summary>
        ///     Ladder climb speed multiplier.
        /// </summary>
        public ConfigEntry<float> ClimbSpeedMultiplier { get; private set; }

        /// <summary>
        ///     Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier.
        /// </summary>
        public ConfigEntry<float> ClimbSprintSpeedMultiplier { get; private set; }

        /// <summary>
        ///     Whether to allow using ladders while carrying a two-handed object.
        /// </summary>
        public ConfigEntry<bool> AllowTwoHanded { get; private set; }

        /// <summary>
        ///     Whether to scale the speed of the climbing animation to the climbing speed.
        /// </summary>
        public ConfigEntry<bool> ScaleAnimationSpeed { get; private set; }

        /// <summary>
        ///     Whether to hide one-handed items while climbing a ladder.
        /// </summary>
        public ConfigEntry<bool> HideOneHanded { get; private set; }

        /// <summary>
        ///     Whether to hide two-handed items while climbing a ladder.
        /// </summary>
        public ConfigEntry<bool> HideTwoHanded { get; private set; }

        /// <summary>
        ///     How long (in seconds) extension ladders remain deployed. Set to 0 for permanent.
        /// </summary>
        public ConfigEntry<float> ExtensionTimer { get; private set; }

        /// <summary>
        ///     Whether extension ladders should kill players they land on.
        /// </summary>
        public ConfigEntry<bool> EnableKillTrigger { get; private set; }

        /* /// <summary>
        ///     Whether the interact key needs to be held to pick up an activated extension ladder.
        /// </summary>
        public ConfigEntry<bool> HoldToPickup { get; private set; } */

        /* /// <summary>
        ///     How long (in seconds) the interact key must be held, if holdToPickup is true.
        /// </summary>
        public ConfigEntry<float> HoldTime { get; private set; } */

        /// <summary>
        ///     Constructor for initializing plugin configuration.
        /// </summary>
        /// <param name="cfg">BepInEx configuration file.</param>
        public Config(ConfigFile cfg)
        {
            // Disable saving config after a call to 'Bind()' is made.
            cfg.SaveOnConfigSet = false;

            // Bind config entries to the config file.
            ClimbSpeedMultiplier = cfg.Bind("Speed", "climbSpeedMultiplier", 1.0f, new ConfigDescription(
                "Ladder climb speed multiplier.", new AcceptableValueRange<float>(0.01f, 10.0f)));
            ClimbSprintSpeedMultiplier = cfg.Bind("Speed", "sprintingClimbSpeedMultiplier", 1.5f, new ConfigDescription(
                "Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier.", new AcceptableValueRange<float>(0.01f, 10.0f)));
            ScaleAnimationSpeed = cfg.Bind("Speed", "scaleAnimationSpeed", true, "Whether to scale the speed of the "
                + "climbing animation to the climbing speed.");

            AllowTwoHanded = cfg.Bind("Items", "allowTwoHanded", true, "Whether to allow using ladders while carrying a "
                + "two-handed object.");
            HideOneHanded = cfg.Bind("Items", "hideOneHanded", true, "Whether to hide one-handed items while climbing a ladder.");
            HideTwoHanded = cfg.Bind("Items", "hideTwoHanded", true, "Whether to hide two-handed items while climbing a ladder.");

            ExtensionTimer = cfg.Bind("Extension Ladder", "extensionTimer", 20.0f, new ConfigDescription("How long (in seconds) "
                + "extension ladders remain deployed. Set to 0 for permanent.", new AcceptableValueRange<float>(0.0f, 700.0f)));
            EnableKillTrigger = cfg.Bind("Extension Ladder", "enableKillTrigger", true, "Whether extension ladders should kill "
                + "players they land on.");
            /* HoldToPickup = cfg.Bind("Extension Ladder", "holdToPickup", true, "Whether the interact key needs to be held to pick "
                + "up an activated extension ladder.");
            HoldTime = cfg.Bind("Extension Ladder", "holdTime", 0.5f, new ConfigDescription("How long (in seconds) the "
                + "interact key must be held, if holdToPickup is true.", new AcceptableValueRange<float>(0.0f, 10.0f))); */
            // ...

            // Request config sync after any change.
            ClimbSpeedMultiplier.SettingChanged += OnSettingChanged;
            ClimbSprintSpeedMultiplier.SettingChanged += OnSettingChanged;
            AllowTwoHanded.SettingChanged += OnSettingChanged;

            ScaleAnimationSpeed.SettingChanged += OnSettingChanged;
            HideOneHanded.SettingChanged += OnSettingChanged;
            HideTwoHanded.SettingChanged += OnSettingChanged;

            ExtensionTimer.SettingChanged += OnSettingChanged;
            EnableKillTrigger.SettingChanged += OnSettingChanged;
            /* HoldToPickup.SettingChanged += OnSettingChanged;
            HoldTime.SettingChanged += OnSettingChanged; */
            // ...

            // Remove old config settings.
            ClearOrphanedEntries(cfg);

            // Re-enable saving and save config.
            cfg.SaveOnConfigSet = true;
            cfg.Save();
        }

        private void OnSettingChanged(object sender, EventArgs args)
        {
            if (BetterLaddersNetworker.Instance != null && BetterLaddersNetworker.Instance.IsSpawned && BetterLaddersNetworker.Instance.IsHost)
            {
                BetterLaddersNetworker.Instance.RefreshSyncedData();
            }
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