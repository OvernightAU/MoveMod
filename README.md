# MoveMod

Drag players around the map with your mouse or finger.

## Features

- **Drag players** - Click and drag any character to move them around
- **Camera zoom** - Adjust your view to see more or less of the map
- **Cross-platform** - Works on PC and mobile

## Controls

### PC
- **Right-click and hold** on a player to grab them
- **Mouse wheel** to zoom in/out

### Mobile
- **Touch and hold** a player to drag them
- **Three-finger pinch** to zoom

## Zoom Settings

- Default zoom: 3x
- Max zoom: 18x
- Zoom resets after meetings and won't work during meetings or while stunned

## Installation

1. Install [BepInEx](https://builds.bepinex.dev/projects/bepinex_be)
2. Install [Reactor](https://github.com/NuclearPowered/Reactor)
3. Drop `MoveMod.dll` into `BepInEx/plugins/`
4. Launch the game

**Note:** All players in the lobby need MoveMod installed.

## Permissions

You can only drag players if you're:
- The host
- The player's owner
- In tutorial mode

This prevents trolling in public lobbies.
