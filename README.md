# BetterLadders
Configurable climbing speed, extension ladder time, and climbing with two-handed items

# Config options
## General
entry | default | description | synced with host
----- | ------- | ----------- | ----------------
climbSpeedMultiplier | 1.0 | Ladder climb speed multiplier | yes
sprintingClimbSpeedMultipler | 1.5 | Ladder climb speed multiplier while sprinting, stacks with climbSpeedMultiplier | yes
allowTwoHanded | true | Whether to allow using ladders while carrying a two-handed object | yes
scaleAnimationSpeed | true | Whether to scale the speed of the climbing animation to the climbing speed | no
hideOneHanded | true | Whether to hide one-handed items while climbing a ladder | no
hideTwoHanded | true | Whether to hide two-handed items while climbing a ladder | no
## Extension Ladders
entry | default | description | synced with host
----- | ------- | ----------- | ----------------
timeMultiplier | 0.0 | Extension ladder time multiplier (0 for permanent) - lasts 20 seconds in vanilla | yes
holdToPickup | true | Whether the interact key needs to be held to pick up an activated extension ladder | no
holdTime | 0.5 | How long, in seconds, the interact key must be held if holdToPickup is true | no

# Changelog
## 1.0.0
- Release
## 1.0.1
- Fixed NullReferenceException when climbing ladder without two-handed object
## 1.1.0
- Added new config option to scale ladder speed while sprinting
- Added new config option to scale ladder climbing animation speed
## 1.2.0
- Added new config option to hide one-handed held objects while climbing
- Added new config option to hide two-handed held objects while climbing
- Fixed ladder animation playing while not moving
## 1.2.1
- Fixed README
## 1.2.2
- When using a mod that adds hotkeys to switch item slots, items are properly hidden
	- Tested with [HotbarPlus](https://thunderstore.io/c/lethal-company/p/FlipMods/HotbarPlus/)
## 1.2.3
- Fixed NullReferenceException when climbing ladder with a ReservedItemSlot mod installed
	- Currently, the reserved item won't be hidden if climbing a ladder while holding it
- Added config syncing with host
	- If joining a host who doesn't have this mod, config options that can be synced will be set to their vanilla defaults
## 1.3.0
- Added new config options for extension ladders
	- timeMultiplier - Extension ladder time multiplier (0 for permanent)
	- holdToPickup - Whether the interact key needs to be held to pick up an activated extension ladder
	- holdTime - How long, in seconds, the interact key must be held if holdToPickup is true