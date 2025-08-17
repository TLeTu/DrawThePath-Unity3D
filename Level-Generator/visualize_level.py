#!/usr/bin/env python3
"""
Level Visualizer - A simple script to visualize generated levels in the terminal
"""

import json
import argparse


def visualize_level(filename: str):
    """Load and visualize a level from JSON file."""
    try:
        with open(filename, 'r') as f:
            level_data = json.load(f)
    except FileNotFoundError:
        print(f"Error: File '{filename}' not found.")
        return
    except json.JSONDecodeError:
        print(f"Error: Invalid JSON in file '{filename}'.")
        return
    
    width = level_data["width"]
    height = level_data["height"]
    tiles = level_data["tiles"]
    start_x = level_data["startTileX"]
    start_y = level_data["startTileY"]
    end_x = level_data["endTileX"]
    end_y = level_data["endTileY"]
    enemies = level_data["enemies"]
    
    print(f"Level: {width}x{height}")
    print(f"Start: ({start_x}, {start_y}) | End: ({end_x}, {end_y})")
    print(f"Enemies: {len(enemies)}")
    print()
    
    # Create enemy position sets for visualization
    enemy_starts = {(enemy["startX"], enemy["startY"]) for enemy in enemies}
    enemy_ends = {(enemy["endX"], enemy["endY"]) for enemy in enemies}
    
    # Print column numbers header
    print("   ", end="")
    for x in range(width):
        print(f"{x % 10}", end="")
    print()
    
    # Print the grid
    for y in range(height):
        # Print row number
        print(f"{y:2d} ", end="")
        
        for x in range(width):
            tile_value = tiles[y]["row"][x]
            
            # Determine what character to display
            if x == start_x and y == start_y:
                char = "S"  # Start
            elif x == end_x and y == end_y:
                char = "E"  # End
            elif (x, y) in enemy_starts:
                char = "1" if tile_value == 1 else "1"  # Enemy start (red)
            elif (x, y) in enemy_ends:
                char = "2" if tile_value == 1 else "2"  # Enemy end (blue)
            elif tile_value == 1:
                char = "."  # Walkable
            else:
                char = "#"  # Unwalkable
            
            print(char, end="")
        print()
    
    print()
    print("Legend:")
    print("  S = Start position")
    print("  E = End position")
    print("  . = Walkable tile")
    print("  # = Unwalkable tile")
    print("  1 = Enemy start position")
    print("  2 = Enemy end position")
    print()
    
    # Print enemy patrol routes
    print("Enemy Patrol Routes:")
    for i, enemy in enumerate(enemies):
        print(f"  Enemy {i+1}: ({enemy['startX']}, {enemy['startY']}) -> ({enemy['endX']}, {enemy['endY']})")


def main():
    parser = argparse.ArgumentParser(description='Visualize generated level files')
    parser.add_argument('filename', nargs='?', default='generated_level.json', 
                       help='JSON file to visualize (default: generated_level.json)')
    
    args = parser.parse_args()
    visualize_level(args.filename)


if __name__ == "__main__":
    main()
