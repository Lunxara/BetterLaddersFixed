using BepInEx.Configuration;
using System;

namespace BetterLadders
{
    [Serializable]
    public class Config : SyncedInstance<Config>
    {
        public float climbSpeedMultiplier { get; private set; }
        public float sprintingClimbSpeedMultiplier { get; private set; }
        public bool allowTwoHanded { get; private set; }
        public bool scaleAnimationSpeed { get; private set; }
        public bool hideOneHanded { get; private set; }
        public bool hideTwoHanded { get; private set; }
        public Config(ConfigFile cfg)
        {
            InitInstance(this);
            climbSpeedMultiplier = cfg.Bind("General", "climbSpeedMultipler", 1.0f, "Ladder climb speed multiplier").Value;
            sprintingClimbSpeedMultiplier = cfg.Bind("General", "sprintingClimbSpeedMultiplier", 1.5f, "Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier").Value;
            allowTwoHanded = cfg.Bind("General", "allowTwoHanded", true, "Whether to allow using ladders while carrying a two-handed object").Value;
            scaleAnimationSpeed = cfg.Bind("General", "scaleAnimationSpeed", true, "Whether to scale the speed of the climbing animation to the climbing speed").Value;
            hideOneHanded = cfg.Bind("General", "hideOneHanded", true, "Whether to hide one-handed items while climbing a ladder - false in vanilla").Value;
            hideTwoHanded = cfg.Bind("General", "hideTwoHanded", true, "Whether to hide two-handed items while climbing a ladder").Value;
        }
    }
}
