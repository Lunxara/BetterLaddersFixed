# CHANGELOG

## [2.1.0]

- Updated networking for `v73` support!
  - **NOTE:** Downgrading is required if intending to play on `v72` and below.

## [2.0.2]

- Made `allowTwoHanded` apply regardless of `InteractTrigger` configuration.
- Removed leftover log from testing stuff.

## [2.0.1]

- Fixed animation speed scaling not actually working.
  - Should now be properly synced across clients, too!

## [2.0.0]

- Readded networking to sync the host's config with clients, done differently than how BetterLadders `v1.2.3` originally implemented it.
  - Opted for using a `NetworkVariable` and `RPCs` in a spawned `NetworkHandler` to sync stuff, instead of `CustomMessagingManager` messages.
  - Config changes done in-game in the middle of a round should sync properly without having to restart.
- Readded Extension Ladder timer and kill trigger configuration, with different implementations as well.
  - Extension ladder hold to pickup, as well as ladder enter/exit animation speed configuration, are still missing for the moment.
- Ladder climb speed and sprinting patches now use `InputAction` events to apply climb speed multipliers, instead of patching the player's `Update()` loop.
  - Player animation speed scaling now syncs across clients, too.
- Fixed `NullReferenceException` when attempting to hide an item held in a reserved slot while climbing.
  - Held items are now also properly hidden for other clients, too.

## [1.0.7]

- Structural Update to the Readme to list future plans for the mod now that pacoito is a maintainer.
- Updated the Github link, for now you likely won't notice any changes until the next release releases however.

## [1.0.6]

- Rolled back the mod to `v1.2.2` for now due to some networking issues, a proper fix is in the works.

## [1.0.5]

- Updated the mod to use `v1.2.3` as a base instead of `v1.2.2` as that was the last version of the mod before the transpilers and networking stuff got added.
  - Config will now be synced across clients, and this will also fixes a compatibility issue with [ReservedItemSlots](https://thunderstore.io/c/lethal-company/p/FlipMods/ReservedItemSlotCore).

## [1.0.4]

- Updated description in the manifest.

## [1.0.3]

- Updated the mod's icon to be the version used in `v1.4.3` since it ultimately looks better :)

## [1.0.2]

- Inverted the changelog.

## [1.0.1]

- Changed the mod page URL to point to the original mod's [MIT License](https://github.com/e3s1/BetterLadders/blob/main/LICENSE.txt).

## [1.0.0]

- Initial reupload of BetterLadders `v1.2.2`.
