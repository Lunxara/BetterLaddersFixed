using BepInEx.Configuration;
using System;

namespace BetterLadders
{
    [Serializable]
    public class Config : SyncedInstance<Config>
    {
        //General (synced)
        public float climbSpeedMultiplier { get; internal set; }
        public float sprintingClimbSpeedMultiplier { get; internal set; }
        public float transitionSpeedMultiplier { get; internal set; }
        public bool allowTwoHanded { get; internal set; }
        //General (not synced)
        public bool scaleAnimationSpeed { get; internal set; }
        public bool hideOneHanded { get; internal set; }
        public bool hideTwoHanded { get; internal set; }
        //Extension Ladders (synced)
        public float timeMultiplier { get; internal set; }
        //public float lengthMultiplier { get; internal set; }
        public bool enableKillTrigger { get; internal set; }
        //Extension Ladders (not synced)
        public bool holdToPickup { get; internal set; }
        public float holdTime { get; internal set; }
        //Debug
        public bool debugMode { get; internal set; }
        //Not in config
        public bool hostMissingMod { get; internal set; }
        internal Config(ConfigFile cfg)
        {
            InitInstance(this);
            //General (synced)
            climbSpeedMultiplier = cfg.Bind("General", "climbSpeedMultipler", 1.0f, "Ladder climb speed multiplier").Value;
            sprintingClimbSpeedMultiplier = cfg.Bind("General", "sprintingClimbSpeedMultiplier", 1.5f, "Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier").Value;
            transitionSpeedMultiplier = cfg.Bind("General", "transitionSpeedMultiplier", 1.0f, "Ladder entrance animation speed multiplier").Value;
            allowTwoHanded = cfg.Bind("General", "allowTwoHanded", true, "Whether to allow using ladders while carrying a two-handed object").Value;
            //General (not synced)
            scaleAnimationSpeed = cfg.Bind("General", "scaleAnimationSpeed", true, "Whether to scale the speed of the climbing animation to the climbing speed").Value;
            hideOneHanded = cfg.Bind("General", "hideOneHanded", true, "Whether to hide one-handed items while climbing a ladder - false in vanilla").Value;
            hideTwoHanded = cfg.Bind("General", "hideTwoHanded", true, "Whether to hide two-handed items while climbing a ladder").Value;
            //Extension Ladders (synced)
            timeMultiplier = cfg.Bind("Extension Ladders", "timeMultiplier", 0f, "Extension ladder time multiplier (0 for permanent) - lasts 20 seconds in vanilla").Value;
            enableKillTrigger = cfg.Bind("Extension Ladders", "enableKillTrigger", true, "Whether extension ladders should kill players they land on").Value;
            //lengthMultiplier = cfg.Bind("Extension Ladders", "lengthMultiplier", 1f, "Extension ladder length multiplier").Value;
            //Extension Ladders (not synced)
            holdToPickup = cfg.Bind("Extension Ladders", "holdToPickup", true, "Whether the interact key needs to be held to pick up an activated extension ladder").Value;
            holdTime = cfg.Bind("Extension Ladders", "holdTime", 0.5f, "How long, in seconds, the interact key must be held if holdToPickup is true").Value;
            //Debug
            debugMode = cfg.Bind("Debug", "debugMode", false, "Displays debug messages in the BepInEx console if true").Value;
            //Not in config
            hostMissingMod = false;
        }
        internal void SetVanillaDefaults()
        {
            Config.Instance.climbSpeedMultiplier = 1.0f;
            Config.Instance.sprintingClimbSpeedMultiplier = 1.0f;
            Config.Instance.transitionSpeedMultiplier = 1.0f;
            Config.Instance.allowTwoHanded = false;
            Config.Instance.timeMultiplier = 1.0f;
            //Config.Instance.lengthMultiplier = 1.0f;
            Config.Instance.enableKillTrigger = true;
            hostMissingMod = true;
        }

    }
}
