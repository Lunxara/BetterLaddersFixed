# BetterLadders
## Features
- Change climbing speed and sprint-climbing speed
	- The climbing animation speed can be scaled to speed
- Change the speed of entering/exiting a ladder
- Allow using ladders while holding a two-handed item
- Hide held item while climbing a ladder
	- Visibility is synced with other clients, as long as the host has this mod
- Change the time extension ladders stay up for (or make them permanent)
- Require holding the interact button to pick up an extended extension ladder
	- Hold time can be adjusted or disabled

## Config
### General
entry | default | description | synced with host
----- | :-----: | ----------- | :--------------:
climbSpeedMultiplier | 1.0 | Ladder climb speed multiplier | `✓`
sprintingClimbSpeedMultiplier | 1.5 | Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier | `✓`
transitionSpeedMultiplier | 1.0 | Ladder enter/exit animation speed multiplier | `✓`
allowTwoHanded | true | Whether to allow using ladders while carrying a two-handed object | `✓`
scaleAnimationSpeed | true | Whether to scale the speed of the climbing animation to the climbing speed | `✓`
hideOneHanded | true | Whether to hide one-handed items while climbing a ladder | `✗`
hideTwoHanded | true | Whether to hide two-handed items while climbing a ladder | `✗`
### Extension Ladders
entry | default | description | synced with host
----- | :-----: | ----------- | :--------------:
timeMultiplier | 0.0 | Extension ladder time multiplier (0 for permanent) - lasts 20 seconds in vanilla | `✓`
enableKillTrigger | true | Whether extension ladders should kill players they land on | `✓`
holdToPickup | true | Whether the interact key needs to be held to pick up an activated extension ladder | `✗`
holdTime | 0.5 | How long, in seconds, the interact key must be held if holdToPickup is true | `✗`
### Debug
entry | default | description
----- | :-----: | -----------
debugMode | false | Logs IL code to console when patching or unpatching transpilers