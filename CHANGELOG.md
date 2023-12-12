# Changelog

All notable changes to this project will be documented in this file.<br>
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Changed "Do-or-die" effect to no longer include "Sleep" challenge.
- Changed "Enable Spawn Protection" option to also increase respawn immunity time if spawn is deemed too dangerous.
- Fixed "Spawn Fake Guardian" effect so that it no longer spawns projectiles. This happens in Fargo's Mod Eternity Mode
  and Calamity Mod Revengeance Mode.

## [2.0.10] - 2023-12-06

- Added an easter egg.

## [2.0.9] - 2023-12-02

- Changed stinky easter to include chat messages (and other minor changes).

## [2.0.8] - 2023-12-02

- Added a stinky easter egg.
- Changed "Hiccup" effect to be more volatile if "Increase Knockback" is active.
- Fixed named items so that they no longer stack with other items unless they have the same name.

## [2.0.7] - 2023-11-30

- Changed "Fling Upwards" effect to force the player to stop grappling.
- Changed Challenge effects to give 5 Gold Coins when completed successfully.
- Changed "Noclip" effect to prevent item use whilst being shimmered.
- Changed "Noclip" effect to continue shimmering whilst in lava.
- Changed "Random Teleport" effect to ignore the Demon Conch if the world's spawn is in Hell.
- Changed "Catch Critter" challenge to spawn 2 critters nearby, closer than before.
- Changed "Catch Critter" challenge to highlight on-screen critters.
- Changed "Craft Item" challenge to accept any torch or wooden sword.
- Changed "Craft Item" challenge to include more options.
- Changed "Stand On Block" challenge to accept any wood.
- Changed "Rainbow Feet" effect to spawn paint balls when striking NPCs also (single player).

## [2.0.6] - 2023-09-29

- Changed "Reforge Item" effect to be able to reforge any valid item.
- Changed "Increase Spawn Rate" effect to allow enemies to spawn in towns.
- Changed "Spawn Honey Trap" effect to spawn a smaller amount of honey.
- Fixed "Catch Critter" challenge not removing bug net from hand in some cases.

## [2.0.5] - 2023-09-20

- Added "Take a Seat" challenge.
- Added "Catch a Critter" challenge.
- Added whips to "Give Summon Weapon" effect.
- Changed "Critter Takeover" effect to temporarily morph the player into a critter.
- Changed some challenges to wait if they would be completed instantly.
- Fixed player morphs not rendering mounts.
- Fixed player morphs not rendering on the minimap or fullscreen map.

## [2.0.4] - 2023-09-02

- Fixed "Give Item" effects not working in multiplayer.
- Fixed player morphs not using correct alpha in some cases.

## [2.0.3] - 2023-08-26

- Added "Give Magic Weapon", "Give Summon Weapon", and "Give Ranged Weapon" effects.
- Added option in config to modify the volume for the "Shuffle Sfx" effect.
- Changed audio-related effects to fail if game sounds are muted.
- Fixed player morphs causing the player to appear invisible on the minimap or big map.
- Fixed items being renamed improperly in some cases. "Reforge Item" effect no longer renames the item.

## [2.0.2] - 2023-08-18

- Changed "Explode Inventory" effect to scale with inventory size.
- Fixed some issues with player morphs.

## [2.0.1] - 2023-08-12

- Added support for custom effect durations; configure via the Crowd Control app.
- Changed some effect descriptions to be more descriptive.
- Removed "Challenge Duration Factor" setting from mod config.

## [2.0.0] - 2023-08-08

- Added support for Terraria 1.4.4: Labour of Love, superseding 1.4.3.
- Added timed effect support in the Crowd Control browser source overlay.
- Added "Use Moon-dial" effect; which fast forwards the time to dusk.
- Added "Word Puzzle" challenge; which asks the user to complete a small word puzzle.
- Added "Noclip" effect; which shimmers the player so that they fall through blocks.
- Added monolith-related effects; which temporarily change the background in-game.
- Added "Switch Loadout" effect.
- Added "Hiccup" effect.
- Added "Increase Knockback" effect.
- Changed "Fart Effect" to spawn a poo item when the player is well-fed.
- Changed Dungeon Guardian effects to hide the chat message if the player is in the dungeon pre-skeletron.
- Changed "Money Boost" effect to only detect NPC strikes from the affected player.
- Fixed "Clear/Explode/Shuffle Inventory" effects not failing when there are no items.
- Fixed "Screen Shake" effect not pausing when game is paused.

## [1.1.4] - 2023-08-08
- Added message notifying 1.4.3 users about Crowd Control for Terraria 1.4.4.
- Changed "Drunk Mode" effect to shuffle items' tooltips.
- Changed "Spawn Critters" effect to no longer spawn Empress Butterfly.
- Changed "Spawn Town NPC" effect to name the spawned NPC after the redeeming viewer.
- Fixed "Shuffle Sfx" effect not finishing properly.

## [1.1.3] - 2023-06-10

- Added support for Crowd Control 2.
- Added easter eggs.
- Fixed mod not disposing properly.

## [1.1.2] - 2023-03-12

- Minor changes.

## [1.1.1] - 2023-03-05

- Minor changes.

## [1.1.0] - 2023-01-30

- Added "Curse" effect; temporarily preventing the streamer from using any items.
- Changed trap effects to retry if the player is already in the trap.
- Changed project file layout.
- Fixed crash during world save caused by incorrect cloning of ItemOwner Global Item.

## [1.0.9] - 2022-12-11

- Added "Spawn Random Town NPC" effect.
- Added "Provide Swimming Buffs" effect.
- Added "Shuffle Sfx" effect, inspired by Hollow Knight Crowd Control.
- Added IDs for content from 1.4.4 in preparation (pets, mounts, critters, etc.)
- Changed "Golden Slime Rain" to despawn healthy slimes when the effect is over.
- Changed "Give Item" effects to prefix the item name with the viewer's name.
- Changed "Teleport to Death" to fail if too close to the death point.
- Changed "Fart" effect to be audible by other players in multiplayer.
- Changed "Fart" effect to also show visual effects.

## [1.0.8] - 2022-10-25

- Changed "Spawn Structure" to retry if the player might already be in the chosen structure.
- Changed "Fling Upwards" effect to provide a short period of immunity.
- Changed "Spawn Trap" effect to alter a smaller area around the player.
- Changed "Spawn Structure" effect to check for an empty space around the player.
- Changed "Set Max Stat Effect" to match tModLoader API changes.
- Changed welcome & farewell messages to stop showing after a successful connection to Crowd Control (per player).

## [1.0.7] - 2022-10-21

- Added Localisation support (currently only en-US).

## [1.0.6] - 2022-08-03

- Added Configuration option to hide "Drop Item" message in chat.
- Changed many effect descriptions to be more descriptive.
- Removed "Touch Grass" challenge from the random challenge pool.

## [1.0.5] - 2022-07-30

- Added Configuration option to forcefully enable easter eggs (under Developer Settings).
- Changed Configuration to show icons next to each option, to improve readability.
- Changed "Drunk Mode" to shuffle item tooltips.
- Fixed "Stand On Block Challenge" choosing a block that the player is already standing on.

## [1.0.4] - 2022-07-29

- Changed Configuration layout to be more readable, by using headings and colours to separate sections.
- Changed "Golden Slime Rain" to increase spawn-rates if the "Increase Spawn Rate" effect is active.
- Changed "Drop Item" to increase the distance the item is thrown.
- Fixed "On Fire" de-buff not bypassing immunities.

## [1.0.3] - 2022-07-29

- Changed default prices on many effects to be cheaper.
- Changed "Increase/Decrease Max Stat" effect naming to "Max Stat (+/- 1)".
- Changed buff effect namings to reflect whether they are positive or negative.
- Changed "Boost Coin Drops" effect duration to 60 seconds.
- Changed "Set Time" effects to be slightly before noon and midnight.
- Changed "Spawn Critters" effect to spawn more critters and reduce the life of evil ones in pre-hardmode.
- Changed "Spawn Sand Trap" to not use Pearlsand in pre-hardmode.
- Changed "Fling Upwards" to only activate if there is suitable vertical space above the player.
- Changed "Explode Inventory" drop chance additive value from 15 to 20 so that more items are dropped.
- Changed "Explode Inventory" to drop items more violently, so they are launched further.
- Fixed TCP connection not detecting an incoming shutdown packet correctly.

## [1.0.2] - 2022-07-24

- Added temporary effects for "For the Worthy" and "Don't Starve".
- Added "Boost Mining Speed" effect.
- Added "Blind" effect that blinds the viewer for a short duration.
- Added "Give Food" effect.
- Added greeting and farewell message when using the mod on a new world.
- Added "Use Effect Hair Dyes" to configuration.
- Changed effect durations for many effects to be longer.
- Fixed broken images on Steam Workshop description.

## [1.0.1] - 2022-07-23

- Added "Force Despawn Bosses" to configuration, so that bosses spawned via effects can be forcibly despawned if all
  players are dead.
- Added unique look-up identifier to "Features".
- Added "Give Food" effect (disabled).
- Changed description of "Enable Spawn Protection" configuration so that it is easier to understand.
- Changed "Explode Inventory" drop chance additive value from 10 to 15 so that more items are dropped.
- Changed "Rainbow Feet" effect to also cause killed NPCs to spawn multiple paint-ball projectiles.
- Fixed "Give Item" effects not working in multi-player.

## [1.0.0] - 2022-07-23

- Initial major release.
- Added new icons and screenshots.