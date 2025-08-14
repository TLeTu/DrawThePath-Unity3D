### Gameplay Description
#### Camera:
- The camera uses a landscape mode isometric view.
- The camera should always see the whole generated map.
#### Control:
- Players can tap (mobile) or use left mouse button (PC) on any tile to move the character there
(use path-finding algorithm for moving, see Reference path finding).
- Extra points are given for using and improving the pathfinding algorithm effectively.
- Players can change the character's destination by tapping or clicking on a different tile. The
character will then move to the new tile instead
#### Character
- The main character moves toward the tile the player selects. The character can either keep
moving at a set speed or stop when it reaches the chosen tile.
- The character can only move to the four tiles directly next to it—up, down, left, or right—and
cannot move diagonally.
- Enemies (NPCs) will stay / patrol on the map
#### Gameplay
- The player has 3 HP at the beginning
- When a level starts, the MC/character appears at the Start cell
- The player must guide the MC by tapping on any tiles. The character will automatically find a path
and move to the selected tile using a pathfinding algorithm.
- Players should complete the level within a limited duration.
- If the MC collides with non-path tiles or patrol enemies:
    - The environmental cell is destroyed, and the enemies may play an animation.
    - MC’s position is reset to the Start cell.
    - Player’s HP is reduced by 1.
    - Effect should be played upon MC’s death.
- When the MC reaches the End cell:
    - Winning animation/effect should be played
    - The score is calculated on how fast the player completes the level
    - The game loads the next level with a bigger or trickier map.
- When the player runs out of HP:
    - Ending animation/effect should be played
    - A UI (User Interface) should be displayed to show the accumulated score across levels
    and provide options for the player to replay.
- Level Design:
    - The game should start at a very easy level to guide users on how to play. (small map, wider path, no enemies)
    - The next levels should be more difficult with larger map, narrower path and increasing
enemies

### Level Design Description
#### Language:
- Language: the level design system should be implemented using Python.
#### Procedural Generation via Parameters:
- Designers don’t manually build levels. Instead, levels are automatically generated based on input parameters (e.g., map size, path width, number of enemies,
number of path turns (to make the path less linear)).
- Designers can easily generate multiple variations by just changing parameters—making the system scalable and repeatable.
#### Difficulty levels:
- Easy: Wide paths 2-3 cells wide, fewer enemies, large open areas (easy navigation), slow enemy patrol speed, fewer path turns.
- Medium: Several paths, 1-2 cells wide, more enemies with varied patrol speeds, fewer obstacles, more path turns.
- Hard: Paths 1 cell wide, minimal open space, faster enemies, more obstacles, more turns.
#### Output format:
- Each generated level is exported in JSON format, making it easy to import into game engines.
#### Map Basics:
- The map is a grid of size W x H.
- A Start (S) and End (E) cell are randomly placed along the edges of the map.
- A walkable Path (P) is generated from Start to End.
- The path:
    - Must connect S and E
    - Includes turns to make the path less linear
    - Has randomized width (1 to 3 cells wide), adding variety to the layout
    - The map may include Alternative paths (shortcuts) or large open areas like 2x2, 3x3 walkable spaces.
#### Filling the Rest of the Map (non-path cells):
- Environment cells:
    - With decorations (varied visuals and sizes, like rocks, trees, ruins)
- Enemies:
    - Start outside the main path
    - Patrol across the path (without camping or blocking it)
    - Have different sizes and patrol speeds
    - Never fully block the path (ensuring it stays navigable)