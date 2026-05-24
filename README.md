# Tower Defense v2

A 2D Tower Defense game built in Unity, designed with performance and WebGL compatibility in mind. 

## Features
- **Multiple Game Modes:**
  - **Finite Mode:** Survive a fixed set of 10 increasingly difficult waves.
  - **Endless Mode:** Survive as long as possible against endless waves of enemies.
  - **PvP Hot-Seat:** A local multiplayer mode where Player 1 (Attacker) uses a budget to build custom waves of enemies, and Player 2 (Defender) attempts to survive them!
- **Optimized for WebGL:** Extensive use of Object Pooling for enemies, projectiles, particle effects, and sound effects to ensure smooth gameplay and prevent garbage collection spikes on the web.
- **Custom Placement System:** Towers snap to a tilemap grid. Placements avoid decorations (rocks, bushes) via dynamic cell bound checking.
- **Dynamic Difficulty:** Enemies scale in health, speed, and reward value dynamically per wave.

## Controls
- **Left Click:** Place towers (Defender Turn).
- **Right Click:** Delete placed towers (Refunds 50% of the cost).
- **Shift + Click:** (PvP Mode) Add 10 enemies to the queue at once.

## Credits & Assets
- **Graphics:** Kenney Tower Defense Top-Down ([Kenney.nl](https://kenney.nl/assets/tower-defense-top-down))
- **Base Asset:** AI-generated.
- **Audio:** Assorted sounds from the Unity Asset Store and web.

## Project Structure
- `Assets/Scripts/`: Contains all core game logic (WaveManagerV2, PvPManager, TowerPlacer, Object Pooling systems).
- `Assets/Scenes/`: Contains the Main Menu and Map scenes.
