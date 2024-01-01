# BetterLadders
Change ladder climbing speed and allow climbing while holding two-handed objects.

# Config options
## Synced
climbSpeedMultiplier (default: 1.0)\
sprintingClimbSpeedMultipler (default: 1.5)\
allowTwoHanded (default: true)
## Not synced
scaleAnimationSpeed (default: true)\
hideOneHanded (default: true)\
hideTwoHanded (default: true)

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