# BetterLaddersFixed

This is a fork of BetterLadders `v1.2.2`, with completely redone networking to fix the `SyncedInstance` errors and client ID warning spam present in the latest versions.

## Features

- Change climbing speed and sprint-climbing speed.
  - Player animation speed can be scaled to match the climbing speed.
- Allow using ladders while holding a two-handed item.
- Hide held item while climbing a ladder.
- Change the time extension ladders stay up for (or make them permanent).
- Enable or disable extension ladder kill trigger.
<!-- - Change the speed of entering/exiting a ladder -->
<!-- - Require holding the interact button to pick up an extended extension ladder.
  - Hold time can be adjusted or disabled. -->

## Currently Missing Features

- Ladder enter/exit animation speed multiplier.
- Hold to pick up extension ladder.

## Config

### Speed

Entry Name | Default value | Description
---------- | :-----------: | -----------
`climbSpeedMultiplier` | 1.0 | Ladder climb speed multiplier.
`sprintingClimbSpeedMultiplier` | 1.5 | Ladder climb speed multiplier while sprinting, stacks with `climbSpeedMultiplier`.
`scaleAnimationSpeed` | true | Whether to scale the speed of the climbing animation to the climbing speed.
<!-- `transitionSpeedMultiplier` | 1.0 | Ladder enter/exit animation speed multiplier. -->

### Items

Entry Name | Default value | Description
---------- | :-----------: | -----------
`allowTwoHanded` | true | Whether to allow using ladders while carrying a two-handed object.
`hideOneHanded` | true | Whether to hide one-handed items while climbing a ladder.
`hideTwoHanded` | true | Whether to hide two-handed items while climbing a ladder.

### Extension Ladder

Entry Name | Default value | Description
---------- | :-----------: | -----------
`extensionTimer` | 20.0 | How long (in seconds) extension ladders remain deployed. Set to 0 for permanent.
`enableKillTrigger` | true | Whether extension ladders should kill players they land on.
<!-- `holdToPickup` | true | Whether the interact key needs to be held to pick up an activated extension ladder.
`holdTime` | 0.5 | How long (in seconds) the interact key must be held, if `holdToPickup` is true. -->

## Credits

- [e3s1](https://github.com/e3s1) — For the original [BetterLadders](https://thunderstore.io/c/lethal-company/p/e3s1/BetterLadders), licensed under [MIT License](https://github.com/e3s1/BetterLadders/blob/main/LICENSE.txt).
- [pacoito](https://github.com/pacoito123) — Current fork maintainer.
