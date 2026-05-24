# Tower Defense v2

A 2D Tower Defense game built in Unity, designed with performance and WebGL compatibility in mind. 

## Features
- **Multiple Game Modes:**
  - **Finite Mode:** Survive a fixed set of 10 increasingly difficult waves.
  - **Endless Mode:** Survive as long as possible against endless waves of enemies.
  - **PvP Hot-Seat:** A local multiplayer mode where Player 1 (Attacker) uses a budget to build custom waves of enemies, and Player 2 (Defender) attempts to survive them!
- **State Machine Architecture:** Global state management via a central `GameManager` ensuring bug-free transitions between menus, building phases, and wave execution.
- **Optimized for WebGL:** Extensive use of Object Pooling for enemies, projectiles, particle effects, and sound effects to prevent garbage collection spikes on the web.
- **Custom Placement System:** Towers snap to a tilemap grid. Placements avoid decorations (rocks, bushes) via dynamic cell bound checking.
- **Separated Economy:** Distinct variables for enemy kill rewards vs. PvP purchase costs (`attackCost`).

## Controls
- **Left Click:** Place towers (Defender).
- **Right Click:** Delete placed towers (Refunds 50% of the cost).
- **Shift + Click / X10 Button:** (PvP Mode) Add 10 enemies to the queue at once.

## GameManager & State Machine
The game architecture relies on a centralized `GameManager.cs` script acting as a State Machine. This prevents various UI scripts from fighting over control and removes hardcoded logic from the gameplay scripts. 

### Game States (`GameState` Enum)
- `Preparation`: The default state for single-player games and the Defender's turn in PvP. Towers can only be built or destroyed when the game is in `Preparation` or `WaveRunning`.
- `AttackerTurn`: Exclusive to PvP mode. Shows the Attacker UI and prevents the defender from placing towers while the attacker queues up their wave.
- `WaveRunning`: Enemies are currently spawning and moving. The Attacker UI is hidden, but the Defender UI remains active so players can use the pause menu and place towers dynamically.
- `GameOver`: The base's health has reached zero. All UI elements (except the Game Over screen) are disabled, and time is paused.
- `GameWin`: The final wave has been survived. Time is paused.

### Pause Management
`GameManager` also centralizes the pause state (`IsPaused`). This allows gameplay scripts like `TowerPlacer` to easily check if the game is paused (preventing accidental tower placement while the pause menu is open), instead of relying solely on `Time.timeScale`.

## Recent Updates & Diffs
- **GameManager Implementation:** Replaced ad-hoc `waveRunning` booleans and manual UI toggles across multiple scripts with an event-driven `GameManager.Instance.OnStateChanged` architecture.
- **Pause Menu Refactoring:** Moved `isPaused` state from `GameUIManager` to `GameManager` to prevent tower placement exploits while the game is paused.
- **Economy Separation:** Separated the PvP attacker cost and PvE generation cost into a new `attackCost` field inside `EnemyData.cs`, distinct from the `reward` field (which dictates coins given to the Defender for kills).
- **Particle Pooling Fixes:** Refactored `DestroyAfterTime.cs` to use `gameObject.SetActive(false)` via a Coroutine instead of a hard `Destroy()`, fully repairing the particle pooling system for Tower build effects and Projectile impact effects.
- **PvP UX Enhancements:** Added an **X10 Toggle Button** and **Shift-Click** support to the PvP menu, allowing the Attacker to quickly queue up large numbers of enemies.
- **Dynamic Defender Actions:** Allowed the Defender to place towers and access the pause menu even while `GameState.WaveRunning` is active.

## Object Pooling Implementation
To achieve maximum performance and prevent garbage collection (GC) stuttering, the game uses extensive Object Pooling instead of `Instantiate()` and `Destroy()`.

- **Enemies (`WaveManagerV2.cs`):** Enemies are spawned from a `List<GameObject> pool`. When an enemy dies or reaches the base, it is set to inactive rather than destroyed.
- **Projectiles (`Tower.cs`):** Each tower manages its own `Dictionary<GameObject, List<GameObject>>` for projectile pooling.
- **Particles (`Projectile.cs` & `Tower.cs`):** Impact effects, death visuals, and Tower construction cloud effects use a static `Dictionary` pool. 
- **Audio (`AudioManager.cs`):** Sound effects are played using a pool of `AudioSource` components.

## Credits & Assets
- **Graphics:** Kenney Tower Defense Top-Down ([Kenney.nl](https://kenney.nl/assets/tower-defense-top-down))
- **Base Asset:** AI-generated.
- **Audio:** Assorted sounds from the Unity Asset Store and web.
