# Draw-The-Path – Implementation Guideline

This document turns PROJECT.md into a concrete plan to build the game in Unity with a Python level generator that outputs JSON.

## 1) Technical Overview
- Engine: Unity (3D; isometric camera over a flat grid).
- Input: Unity Input System (already present: `Assets/InputSystem_Actions.inputactions`).
- Movement & Pathfinding: Grid-based A* (4-neighbors; Manhattan heuristic).
- Procedural Levels: External Python script generates JSON; Unity imports and builds scenes at runtime.
- Platforms: PC and Mobile (landscape). Touch/click to set destination.

## 2) Project Structure (Unity)
- `Assets/Scripts/Core` – GameManager, SceneLoader, Config.
- `Assets/Scripts/Level` – LevelLoader (JSON -> scene), GridBuilder, Tile, CellCoords.
- `Assets/Scripts/Pathfinding` – GridGraph, AStar, PathfinderService.
- `Assets/Scripts/Player` – PlayerController, InputController, Health, Death/Win effects.
- `Assets/Scripts/Enemy` – EnemyController, PatrolRoute, Spawner.
- `Assets/Scripts/UI` – HUD (HP, timer, score), EndScreen, Pause.
- `Assets/Prefabs` – 3D Tiles/Floor (Path, Start, End), Env/Obstacle/Decoration props, Player (CharacterController), Enemy.
- `Assets/Art` or `Assets/Models` – 3D models, materials, VFX.
- `Assets/Data/Levels` – JSON level files (TextAssets for builds). Also allow StreamingAssets for external.
- `Assets/Settings` – Layers/Tags presets (Path, Obstacle, Enemy, Player).

## 3) Level JSON Contract
Unity expects the following schema (all coords in grid units; (0,0) = bottom-left):
- `width:int`, `height:int`
- `start:{x:int,y:int}`, `end:{x:int,y:int}`
- `path:[{x:int,y:int}]` – cells forming the guaranteed main walkable path from S to E.
- `pathWidth:int|[min,max]` – effective width used during generation (optional; informative).
- `openAreas:[{x:int,y:int,w:int,h:int}]` – large walkable patches (optional).
- `altPaths:[[{x,y}...]]` – optional shortcuts/branches.
- `decorations:[{x:int,y:int,type:string,variant?:string,blocking?:bool}]`
- `enemies:[{id?:string,size:1|2, speed:float, patrol:[{x:int,y:int}], loop:boolean}]`
- `seed:int` (optional)

Example (trimmed):
{
  "width": 18,
  "height": 12,
  "start": {"x": 0, "y": 6},
  "end": {"x": 17, "y": 5},
  "path": [{"x":0,"y":6}, {"x":1,"y":6}, ...],
  "openAreas": [{"x":6,"y":7,"w":3,"h":3}],
  "enemies": [{"size":1,"speed":1.2,"patrol":[{"x":4,"y":2},{"x":10,"y":8}],"loop":true}]
}

Rules enforced by importer:
- Path cells are walkable; altPaths and openAreas add additional walkables.
- Decorations with `blocking=true` become obstacles; otherwise visuals only.
- Enemies must never occupy walkable cells permanently; patrol crosses path without camping.

## 4) Python Level Generator (outside Unity)
- Location suggestion: `/Tools/levelgen` (kept out of `Assets`).
- Inputs (CLI args): width, height, turns, path_width_min/max, branches, open_areas, enemies_count, enemy_speed_range, seed.
- Steps:
  1. Place Start/End randomly on different edges.
  2. Generate a main path S→E:
     - Use biased random walk or A* to target E with penalties that encourage turns.
     - Guarantee connectivity; backtrack if stuck.
  3. Widen the path (dilate by 0–2 cells) to get width 1–3 randomly along segments.
  4. Add branches/alt paths and 2x2/3x3 open areas.
  5. Scatter decorations; mark some as blocking obstacles off the main path.
  6. Create enemy patrols that cross the path (waypoints on opposite sides), ensure no full blockage and min clearance of 1 cell.
  7. Validate: path exists; enemy patrols don’t block; S/E on edges.
  8. Emit JSON per schema.
- Difficulty presets map to parameters:
  - Easy: small `turns`, width 2–3, few enemies slow, more open areas.
  - Medium: width 1–2, more turns, moderate enemies varied speeds.
  - Hard: width 1, high turns, minimal open space, faster enemies.

## 5) Unity Level Import & Build
- LevelLoader
  - Input: TextAsset (JSON) or file from StreamingAssets.
  - Parse to model; build a boolean walkable grid.
  - Instantiate 3D tiles/meshes:
    - Path/Alt/OpenArea -> floor tile prefab(s) at y=0 with colliders as needed.
    - Start/End -> special markers/meshes.
    - Decorations -> visual prefabs; if blocking, add BoxCollider on Obstacle layer.
  - Build GridGraph from walkables for pathfinding.
- Coordinate system: grid cell size = 1 world unit; Y-up; use Z for forward. Tile centers at (x+0.5, 0, y+0.5).

## 6) Camera (Isometric + Fit-to-Map, 3D)
- Camera options:
  - Orthographic: rotate for isometric feel (e.g., x=35–45°, y=45°) and set orthographic size to fit.
  - Perspective: tilt similarly and position camera back so the whole grid fits.
- Fit whole map:
  - Compute world AABB of the grid (width=W, depth=H). Add margin.
  - Orthographic: size = max(W/2/aspect, H/2) + margin.
  - Perspective: choose FOV (vertical). Required distance d = max(H/2, W/2/aspect) / tan(FOV/2) + margin; place camera along its forward.
  - Keep aspect ratio; clamp min/max for readability on mobile.

## 7) Input & Movement
- Input System mapping: Point (mouse/touch), Click/Tap.
- Selection:
  - Physics.Raycast from screen point to a Ground/Path layer; snap hit point to nearest grid cell.
  - Ignore non-walkable cells.
- Player movement (3D):
  - Use CharacterController.Move (preferred) or kinematic Rigidbody to follow the computed path.
  - Constant speed; stop on last node. Face movement direction (rotate on Y only).
  - Interruptible: new tap recomputes path from current cell.
  - 4-directional only (no diagonals).

## 8) Pathfinding
- A* on GridGraph with Manhattan heuristic; neighbors = up/down/left/right.
- Early exit when reaching target.
- Optional: simple path compaction (remove straight-line intermediate nodes for smoother motion).
- Performance: reuse node arrays; pool lists; avoid GC.

## 9) Enemies
- EnemyController moves along `patrol` waypoints; ping-pong or loop.
- 3D Colliders (Capsule/Box) + trigger to detect MC crossing; kinematic Rigidbody or CharacterController.
- Must not idle on path center; add small wait offsets off-path if needed.
- Size affects collider bounds and visual scale; speed from JSON.

## 10) Gameplay Rules
- Player starts at Start cell with 3 HP.
- On collision with enemy or stepping into non-walkable:
  - Play effects; destroy affected env cell (if specified in JSON/design); reset MC to Start; HP–=1.
- Timer starts on level load; score = base + time bonus.
- Win when reaching End:
  - Play win effect; accumulate score; load next level (larger/trickier params).
- Lose when HP == 0:
  - Play lose effect; show End UI with score, Retry/Next options.

## 11) UI
- HUD: Hearts (3), timer, current score, level label.
- End Screen: Win/Lose message, total score, buttons (Retry, Next/Continue, Main Menu).
- Pause overlay (optional): Resume, Restart.

## 12) Effects & Audio (placeholders acceptable first)
- Player death burst; enemy alert; win confetti; tile spawn pop.
- SFX: click/tap, move, hit, win/lose.

## 13) Scenes & Boot Flow
- `Boot` scene: GameManager (DontDestroyOnLoad), loads first level.
- `Game` scene: camera, UI root, empty grid parent.
- Level progression list: array of TextAssets or file names mapped to difficulty.

## 14) Layers/Tags & Physics
- Layers: Player, Enemy, Path, Obstacle, Decoration, UI, Ground.
- 3D Physics: CharacterController or kinematic bodies for agents; BoxColliders on tiles/obstacles; triggers for detection.
- Collisions: Player x Enemy (trigger), Player x Obstacle (solid), Player x Path/Ground (none).
- Use tags to quickly identify Start/End tiles.

## 15) Testing & Validation
- EditMode: JSON schema validation; grid build tests.
- PlayMode: path exists S→E; tapping selects valid tiles; enemies don’t fully block; lose/win transitions.
- Automated level sanity check across many seeds (headless) logging failures.

## 16) Implementation Notes
- Keep all grid logic integer-based; convert to world once at render: (x+0.5, 0, y+0.5).
- Use object pooling for tiles/enemies if rebuilding levels at runtime.
- Separate data (JSON) from presentation (prefabs) for easy iteration.
- Mobile: ensure touch targets >= 64 px; add input debounce.
- Rendering: URP (3D) recommended; baked lighting optional; keep materials simple for mobile.

## 17) Deliverables
- Unity scene(s) and scripts per structure.
- Python generator with CLI, README, and sample JSON levels (easy/med/hard).
- At least 5 sample levels per difficulty under `Assets/Data/Levels`.
- Documentation: how to add new difficulty presets and parameters.
