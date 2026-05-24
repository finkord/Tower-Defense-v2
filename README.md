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

## Object Pooling Implementation
To achieve maximum performance and prevent garbage collection (GC) stuttering, especially on WebGL, the game uses extensive Object Pooling instead of `Instantiate()` and `Destroy()`.

- **Enemies (`WaveManagerV2.cs`):** Enemies are spawned from a `List<GameObject> pool` within their `EnemySpawnConfig`. When an enemy dies or reaches the base, it is set to inactive rather than destroyed.
- **Projectiles (`Tower.cs`):** Each tower manages its own `Dictionary<GameObject, List<GameObject>>` for projectile pooling. Projectiles are deactivated on impact and reused for the next shot.
- **Particles (`Projectile.cs`):** Impact and death visual effects use a static `Dictionary` pool. Particle Systems have their "Stop Action" set to `Disable` in the Unity Editor so they automatically return to the pool when finished playing.
- **Audio (`AudioManager.cs`):** Sound effects are played using a pool of `AudioSource` components, preventing the overhead of creating and destroying audio objects constantly.

## Credits & Assets
- **Graphics:** Kenney Tower Defense Top-Down ([Kenney.nl](https://kenney.nl/assets/tower-defense-top-down))
- **Base Asset:** AI-generated.
- **Audio:** Assorted sounds from the Unity Asset Store and web.

## Project Structure
- `Assets/Scripts/`: Contains all core game logic (WaveManagerV2, PvPManager, TowerPlacer, Object Pooling systems).
- `Assets/Scenes/`: Contains the Main Menu and Map scenes.
