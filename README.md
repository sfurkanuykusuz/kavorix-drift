# Kavorix Drift

**Kavorix Drift** is a 2D space survival game built with Unity.

The project was originally created while following Unity Learn's **Game Development Pathway**, specifically the **Create a Basic 2D Game** mission. It was later expanded with custom gameplay scripts, UI polish, responsive layout systems, particle effects, high score tracking, custom visuals, and music.

The goal is simple: control your spaceship, avoid obstacles, survive as long as possible, and beat your high score.

---

## Play the Game

You can play **Kavorix Drift** on Unity Play:

[Play Kavorix Drift on Unity Play](https://play.unity.com/en/games/5c8a72da-99cc-4458-9b56-0598dc00b2bd/kavorix-drift)

---

## About the Project

**Kavorix Drift** is a small polished Unity learning project focused on 2D physics-based movement, obstacle avoidance, responsive layout, and UI Toolkit-based menus.

The project started as a Unity Learn exercise and was expanded into a more complete playable prototype with additional gameplay systems, custom visual presentation, music, sound effects, and a cleaner modular script structure.

Main development goals:

- Build a complete small-scale 2D game prototype
- Practice Unity physics, input handling, UI Toolkit, particles, and scene organization
- Improve the original Unity Learn project with custom systems
- Refactor gameplay code into smaller and more maintainable components
- Prepare the project for public GitHub sharing

---

## Gameplay

The player controls a spaceship drifting through space.

Obstacles move around the game area, and the player must avoid colliding with them. The score increases over time, so surviving longer results in a higher score.

When the player collides with an obstacle or border, the game ends. The final score is compared with the saved high score, and a new high score message is shown when the player beats the previous record.

The main objective is to survive as long as possible.

---

## Features

- 2D physics-based spaceship movement
- Pointer-based thrust direction
- Randomized obstacle size, force, and spin
- Velocity limiting for player and obstacles
- Collision-based game over system
- Score and high score system using `PlayerPrefs`
- Start menu and game over menu
- New high score feedback animation
- UI built with Unity UI Toolkit
- Responsive orthographic camera layout
- Dynamic background scaling
- Responsive border positioning
- Mobile portrait orientation warning
- Optional pause behavior in portrait orientation
- Boost particle effect and boost audio
- Explosion effect on player death
- Bounce effect on obstacle collisions
- WebGL, Android, and Windows build profile setup

---

## Controls

### Desktop

- Hold the primary mouse button to thrust.
- Move the pointer to rotate the ship toward the pointer position.
- Avoid obstacles and survive as long as possible.

### Mobile

- Touch and hold to thrust.
- Move your finger to control the ship direction.
- Landscape orientation is recommended.

---

## Project Structure

```text
Assets/
├── Art/
│   ├── Particles/
│   └── Sprites/
│       └── Obstacles/
├── Audio/
│   └── sfx/
├── Fonts/
├── Materials/
├── Prefabs/
│   └── Environment/
├── Scenes/
├── Scripts/
├── Settings/
├── UI/
└── UI Toolkit/

Packages/
├── manifest.json
└── packages-lock.json

ProjectSettings/
```

### Important Folders

| Folder | Description |
| --- | --- |
| `Assets/Art` | Contains icons, logo, splash image, sprites, particle textures, and visual assets. |
| `Assets/Audio` | Contains background music and sound effects. |
| `Assets/Fonts` | Contains the font used by the UI. |
| `Assets/Materials` | Contains particle materials and the 2D physics material. |
| `Assets/Prefabs` | Contains reusable game objects such as obstacles, effects, and environment prefabs. |
| `Assets/Scenes` | Contains the main Unity scene. |
| `Assets/Scripts` | Contains gameplay, UI, scoring, movement, collision, and layout scripts. |
| `Assets/Settings` | Contains render pipeline, input system, scene template, and build profile settings. |
| `Assets/UI` | Contains UI Toolkit USS and UXML files. |
| `Assets/UI Toolkit` | Contains panel settings and UI Toolkit theme files. |
| `Packages` | Contains Unity package dependency information. |
| `ProjectSettings` | Contains Unity project configuration files. |

---

## Main Scripts

The project uses a modular script structure instead of keeping all gameplay logic inside a single large controller.

| Script | Purpose |
| --- | --- |
| `PlayerController.cs` | Coordinates the main game flow, including start, gameplay, game over, restart, and exit behavior. |
| `PlayerMovement2D.cs` | Handles player input, rotation, thrust movement, and speed clamping. |
| `PlayerBoostEffect.cs` | Controls boost particles and boost audio. |
| `PlayerCollisionHandler2D.cs` | Detects player collisions and notifies the main controller. |
| `PlayerDeathHandler2D.cs` | Handles explosion, physics disabling, and player visibility after death. |
| `ScoreManager.cs` | Manages score calculation, high score saving, and high score reset. |
| `GameUIController.cs` | Controls UI Toolkit menus, score labels, high score feedback, and button events. |
| `Obstacle.cs` | Handles obstacle size randomization, movement, spin, velocity limiting, and bounce effects. |
| `OrientationWarningUI.cs` | Shows a portrait mode warning overlay on mobile and can pause the game in portrait orientation. |
| `ResponsiveGameArea.cs` | Adjusts the camera, background, and borders for different screen sizes. |

---

## Requirements

Recommended setup:

- Unity 6 or newer
- Universal Render Pipeline 2D
- Unity Input System package
- Unity UI Toolkit
- Git for version control

To check the Unity version used by the project, open:

```text
ProjectSettings/ProjectVersion.txt
```

---

## How to Run the Project

1. Clone the repository:

```bash
git clone https://github.com/sfurkanuykusuz/kavorix-drift.git
```

2. Open Unity Hub.

3. Click **Add project from disk**.

4. Select the cloned project folder.

5. Open the project with the Unity version specified in:

```text
ProjectSettings/ProjectVersion.txt
```

6. Open the main scene:

```text
Assets/Scenes/Game.unity
```

7. Press **Play** in the Unity Editor.

---

## Build Targets

The project includes build profile settings for:

- Web Desktop
- Web Mobile
- Android
- Windows

Build outputs are intentionally excluded from version control.

The current playable WebGL version is available on Unity Play:

[Play Kavorix Drift on Unity Play](https://play.unity.com/en/games/5c8a72da-99cc-4458-9b56-0598dc00b2bd/kavorix-drift)

---

## Version Control Notes

This repository uses a Unity-specific `.gitignore`.

The following folders are intentionally included:

```text
Assets/
Packages/
ProjectSettings/
```

Generated folders and build outputs such as `Library/`, `Temp/`, `Build/`, `Builds/`, `Logs/`, and Unity build backup/debug folders are excluded from version control.

Unity `.meta` files are included because they are required for asset references, prefab links, scene references, import settings, and GUID consistency.

---

## Asset Notes

This project includes custom visual and audio assets created for **Kavorix Drift**.

Some visual assets and music were created with AI assistance and then integrated into the Unity project as part of the game's presentation.

Asset categories include:

- Player sprite
- Obstacle sprites
- Background image
- Particle texture
- Logo and icon assets
- Splash image
- Background music
- Sound effects

---

## Credits

Created by following Unity Learn's **Game Development Pathway** and expanded into a custom personal game project.

Additional work includes:

- Custom gameplay scripting
- Modular player controller refactor
- UI Toolkit interface
- Responsive game area system
- Custom visual presentation
- Music and sound integration
- WebGL and Android build preparation

---

## Project Status

**Kavorix Drift** is currently a small polished Unity learning project prepared for public GitHub sharing.

Current status:

- Core gameplay is playable
- UI flow is implemented
- High score system is implemented
- Responsive layout is implemented
- Mobile orientation warning is implemented
- Scripts have been refactored into smaller components
- Project is available on Unity Play