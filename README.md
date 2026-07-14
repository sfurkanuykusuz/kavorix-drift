# Kavorix Drift

![Unity](https://img.shields.io/badge/Engine-Unity-black?logo=unity)
![C#](https://img.shields.io/badge/Language-C%23-68217A?logo=csharp)
![Platform](https://img.shields.io/badge/Platform-WebGL%20%7C%20Android%20%7C%20Windows-blue)
![Status](https://img.shields.io/badge/Status-Playable-brightgreen)

**Kavorix Drift** is a polished 2D space survival game built with Unity.

The project started as part of Unity Learn's **Game Development Pathway** and was expanded into a complete playable prototype with custom gameplay systems, power-ups, responsive UI, particles, sound effects, camera feedback, high score tracking, and custom visual presentation.

The goal is simple: control your spaceship, avoid obstacles, collect power-ups, survive as long as possible, and beat your high score.

---

## Play the Game

You can play **Kavorix Drift** on Unity Play:

[Play Kavorix Drift on Unity Play](https://play.unity.com/en/games/5c8a72da-99cc-4458-9b56-0598dc00b2bd/kavorix-drift)

---

## Gameplay

The player controls a spaceship drifting through a 2D space arena.

Obstacles move around the play area, and the player must avoid colliding with them. The score increases over time, so surviving longer results in a higher score.

During gameplay, power-ups can appear. When collected, the player can choose between two abilities:

- **Guided Missile** — select and destroy an obstacle.
- **Shield** — temporarily protect the player from obstacle and border collisions.

If all obstacles are destroyed, the game ends with an **all-clear bonus**, multiplying the final score by `x10`.

---

## Controls

Kavorix Drift uses pointer-based movement. The spaceship rotates toward the current pointer or touch position, and thrust is applied only while the input is held.

### Desktop

| Action | Control |
| --- | --- |
| Aim / rotate ship | Move the mouse pointer |
| Thrust | Hold the primary mouse button |
| Collect power-up | Move the ship into the pickup |
| Choose power-up | Click **Missile** or **Shield** |
| Select missile target | Click an obstacle after choosing the guided missile |

### Mobile

| Action | Control |
| --- | --- |
| Aim / rotate ship | Move your finger on the screen |
| Thrust | Touch and hold |
| Collect power-up | Move the ship into the pickup |
| Choose power-up | Tap **Missile** or **Shield** |
| Select missile target | Tap an obstacle after choosing the guided missile |

Landscape orientation is recommended for mobile gameplay.

---

## Features

- 2D physics-based spaceship movement
- Pointer and touch-based thrust direction
- Pre-game countdown
- Randomized obstacle size, speed, force, and spin
- Score and high score system using `PlayerPrefs`
- Missile and shield power-up system
- Guided missile target selection
- Target indicator for selected obstacles
- Temporary shield protection
- Obstacle tracking and remaining obstacle count UI
- All-clear `x10` completion bonus
- UI built with Unity UI Toolkit
- Start menu, power-up choice UI, and game over UI
- New high score and completion bonus feedback
- Responsive orthographic camera layout
- Dynamic background and border scaling
- Mobile portrait orientation warning
- Boost, explosion, bounce, impact, and camera shake feedback
- Obstacle-border and obstacle-obstacle impact audio
- WebGL, Android, and Windows build profile setup

---

## Project Structure

```text
Assets/
├── Art/
├── Audio/
├── Fonts/
├── Materials/
├── Prefabs/
├── Scenes/
├── Scripts/
├── Settings/
├── UI/
└── UI Toolkit/

Packages/
ProjectSettings/
```

| Folder | Description |
| --- | --- |
| `Assets/Art` | Sprites, icons, splash images, particle textures, and visual assets. |
| `Assets/Audio` | Background music and sound effects. |
| `Assets/Fonts` | UI font assets. |
| `Assets/Materials` | Particle materials and 2D physics materials. |
| `Assets/Prefabs` | Reusable game objects such as obstacles, effects, power-ups, and environment prefabs. |
| `Assets/Scenes` | Main Unity scene. |
| `Assets/Scripts` | Gameplay, UI, scoring, movement, collision, power-up, feedback, and layout scripts. |
| `Assets/UI` | UI Toolkit USS and UXML files. |
| `Packages` | Unity package dependency information. |
| `ProjectSettings` | Unity project configuration files. |

---

## Main Scripts

The project uses a modular script structure instead of keeping all gameplay logic in a single large controller.

| Script | Purpose |
| --- | --- |
| `PlayerController.cs` | Coordinates game flow, countdown, gameplay, game over, restart, exit behavior, camera shake triggers, and completion bonus flow. |
| `PlayerMovement2D.cs` | Handles player input, rotation, thrust movement, pointer tracking, and speed clamping. |
| `PlayerBoostEffect.cs` | Controls boost particles and boost audio. |
| `PlayerCollisionHandler2D.cs` | Detects player collisions and notifies the main controller. |
| `PlayerDeathHandler2D.cs` | Handles explosion, physics disabling, and player visibility after death. |
| `ScoreManager.cs` | Manages score, high score, score milestones, and final score multiplier logic. |
| `GameUIController.cs` | Controls UI Toolkit menus, score labels, power-up choice UI, high score feedback, completion bonus feedback, and button events. |
| `Obstacle.cs` | Handles obstacle randomization, movement, spin, velocity limiting, bounce effects, impulse behavior, and destruction. |
| `ObstacleTracker2D.cs` | Tracks remaining obstacles and triggers the all-clear completion bonus. |
| `PowerUpSpawner2D.cs` | Spawns power-up pickups inside the safe play area. |
| `PowerUpPickup2D.cs` | Handles pickup collection, rotation, lifetime, and pickup sound. |
| `PlayerPowerUpController.cs` | Handles power-up collection, selection flow, and activation. |
| `PlayerShield2D.cs` | Controls shield activation, duration, visual state, obstacle bounce behavior, and shield impact audio. |
| `GuidedMissile2D.cs` | Controls missile movement, target tracking, obstacle destruction, effects, audio, and camera shake. |
| `MissileTargetSelector2D.cs` | Handles obstacle selection for the guided missile. |
| `TargetIndicatorFollower2D.cs` | Keeps the target indicator centered on the selected obstacle. |
| `CameraShake2D.cs` | Provides camera shake feedback for impacts and explosions. |
| `BorderImpactAudio2D.cs` | Plays impact sound effects when obstacles hit borders. |
| `ObstacleCollisionAudio2D.cs` | Plays impact sound effects when obstacles collide with each other. |
| `OrientationWarningUI.cs` | Shows a portrait mode warning overlay on mobile. |
| `ResponsiveGameArea.cs` | Adjusts the camera, background, and borders for different screen sizes. |

---

## Requirements

Recommended setup:

- Unity 6 or newer
- Universal Render Pipeline 2D
- Unity Input System
- Unity UI Toolkit
- Git

The Unity version used by the project is listed in:

```text
ProjectSettings/ProjectVersion.txt
```

---

## Setup

Clone the repository:

```bash
git clone https://github.com/sfurkanuykusuz/kavorix-drift.git
```

Open the project with the Unity version specified in `ProjectSettings/ProjectVersion.txt`.

Main scene:

```text
Assets/Scenes/Game.unity
```

---

## Build Targets

The project includes build profile setup for:

- Web Desktop
- Web Mobile
- Android
- Windows

Build outputs are intentionally excluded from version control.

For Android, landscape orientation is recommended. The project also includes a portrait warning UI for unsupported orientation flow.

---

## Version Control Notes

This repository uses a Unity-specific `.gitignore`.

The following folders are intentionally included:

```text
Assets/
Packages/
ProjectSettings/
```

Generated folders such as `Library/`, `Temp/`, `Build/`, `Builds/`, `Logs/`, and Unity backup/debug folders are excluded from version control.

Unity `.meta` files are included because they are required for asset references, prefab links, scene references, import settings, and GUID consistency.

---

## Asset Notes

This project includes custom visual and audio assets created for **Kavorix Drift**.

Some visual assets and music were created with AI assistance and then integrated into the Unity project as part of the game's presentation.

Asset categories include:

- Player sprite
- Obstacle sprites
- Power-up visuals
- Missile visual
- Shield visual
- Target indicator
- Background image
- Particle textures
- Background music
- Sound effects

---

## Credits

Created by following Unity Learn's **Game Development Pathway** and expanded into a custom personal game project.

Additional work includes custom gameplay scripting, modular code refactoring, missile and shield power-ups, UI Toolkit interface, responsive layout systems, camera shake, impact feedback, custom visuals, audio integration, and WebGL/Android build preparation.

---

## Project Status

**Kavorix Drift** is a playable Unity learning project.

Implemented systems include:

- Core gameplay loop
- Pre-game countdown
- High score system
- Missile and shield power-ups
- Obstacle tracking and all-clear bonus
- Camera shake and impact feedback
- Responsive UI and game area layout
- Mobile orientation warning
- Unity Play WebGL build