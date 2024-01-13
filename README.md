# BetterLadders
Configurable climbing speed, extension ladder time, and climbing with two-handed items

# Config options
## General
entry | default | description | synced with host
----- | :-----: | ----------- | :--------------:
climbSpeedMultiplier | 1.0 | Ladder climb speed multiplier | `✓`
sprintingClimbSpeedMultiplier | 1.5 | Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier | `✓`
transitionSpeedMultiplier | 1.0 | Ladder enter/exit animation speed multiplier | `✓`
allowTwoHanded | true | Whether to allow using ladders while carrying a two-handed object | `✓`
scaleAnimationSpeed | true | Whether to scale the speed of the climbing animation to the climbing speed | `✓`
hideOneHanded | true | Whether to hide one-handed items while climbing a ladder | `✗`
hideTwoHanded | true | Whether to hide two-handed items while climbing a ladder | `✗`
## Extension Ladders
entry | default | description | synced with host
----- | :-----: | ----------- | :--------------:
timeMultiplier | 0.0 | Extension ladder time multiplier (0 for permanent) - lasts 20 seconds in vanilla | `✓`
enableKillTrigger | true | Whether extension ladders should kill players they land on | `✓`
holdToPickup | true | Whether the interact key needs to be held to pick up an activated extension ladder | `✗`
holdTime | 0.5 | How long, in seconds, the interact key must be held if holdToPickup is true | `✗`