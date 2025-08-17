# Procedural Level Generator for Tile-based Game

This Python script generates procedural levels for tile-based games with customizable parameters.

## Features

- **Configurable Grid Size**: Set custom width and height for your levels
- **Dynamic Path Generation**: Create main paths with specified width and number of turns
- **Enemy Placement**: Automatically place enemies with patrol routes that intersect the main path
- **JSON Export**: Export levels in a structured JSON format for easy integration
- **Visualization Tool**: Included visualizer to preview generated levels in the terminal

## Requirements

- Python 3.6 or higher
- No external dependencies required (uses only standard library)

## Usage

### Basic Usage

Generate a level with default parameters:

```bash
python3 level_generator.py
```

### Custom Parameters

```bash
python3 level_generator.py --map_width 20 --map_height 15 --path_width 2 --num_enemies 3 --num_path_turns 2
```

### All Available Parameters

- `--map_width`: Width of the grid (default: 20, minimum: 5)
- `--map_height`: Height of the grid (default: 15, minimum: 5)
- `--path_width`: Width of the main path (default: 2, minimum: 1)
- `--num_enemies`: Number of enemies to place (default: 3, minimum: 0)
- `--num_path_turns`: Number of turns in the main path (default: 2, minimum: 0)
- `--output`: Output JSON filename (default: 'generated_level.json')

### Examples

**Small level with narrow path:**
```bash
python3 level_generator.py --map_width 10 --map_height 8 --path_width 1 --num_enemies 1 --num_path_turns 1
```

**Large level with wide path and many enemies:**
```bash
python3 level_generator.py --map_width 30 --map_height 25 --path_width 3 --num_enemies 8 --num_path_turns 5
```

**Custom output file:**
```bash
python3 level_generator.py --output my_custom_level.json
```

## Level Visualization

Use the included visualizer to preview generated levels:

```bash
python3 visualize_level.py generated_level.json
```

The visualizer shows:
- `S` = Start position
- `E` = End position  
- `.` = Walkable tiles
- `#` = Unwalkable tiles
- `1` = Enemy start positions
- `2` = Enemy end positions

## Generated Level Structure

The output JSON file follows this format:

```json
{
  "width": 20,
  "height": 15,
  "tiles": [
    { "row": [0, 1, 1, 0, ...] },
    { "row": [1, 0, 0, 1, ...] }
  ],
  "startTileX": 5,
  "startTileY": 0,
  "endTileX": 12,
  "endTileY": 14,
  "enemies": [
    { "startX": 8, "startY": 3, "endX": 15, "endY": 8 }
  ]
}
```

### Tile Values
- `0` = Unwalkable tile
- `1` = Walkable tile

## Level Generation Rules

1. **Grid**: Represented as width × height with 0 (unwalkable) and 1 (walkable) tiles
2. **Main Path**: 
   - Connects start tile (on one edge) to end tile (on opposite edge)
   - Has specified width in tiles
   - Includes specified number of turns to avoid straight lines
   - May include alternative branches and open areas
3. **Enemies**:
   - Move between two patrol coordinates (startX, startY, endX, endY)
   - Do not spawn directly on the main path
   - Patrol routes intersect or cross the main path at least once
   - Ensures player encounters during gameplay
4. **Guaranteed Connectivity**: Always ensures a valid walkable path from start to end

## Algorithm Overview

1. **Path Generation**: Uses L-shaped segments between waypoints to create the main path
2. **Alternative Paths**: Adds random branches and open areas (2×2, 3×3) connected to main path
3. **Enemy Placement**: 
   - Finds walkable tiles not on main path for enemy spawning
   - Creates patrol routes that intersect the main path
   - Adds additional walkable areas if needed for enemy placement
4. **Validation**: Ensures level meets all constraints and connectivity requirements

## Files

- `level_generator.py` - Main level generation script
- `visualize_level.py` - Terminal-based level visualizer
- `generated_level.json` - Default output file (created after running generator)

## Integration

The generated JSON can be easily integrated into game engines:

- **Unity**: Import JSON and parse tiles array to instantiate prefabs
- **Godot**: Load JSON and create TileMap from tiles data
- **Custom Engines**: Parse JSON structure and convert to your tile format

## Troubleshooting

**Error: Map dimensions must be at least 5x5**
- Increase --map_width and --map_height to at least 5

**Error: Path width must be at least 1**
- Set --path_width to 1 or higher

**Few or no enemies placed**
- Increase map size or decrease number of enemies
- The algorithm creates additional walkable areas if needed

**Path too simple**
- Increase --num_path_turns for more complex paths
- Larger maps allow for more interesting path generation

## License

This project is open source and available under the MIT License.
