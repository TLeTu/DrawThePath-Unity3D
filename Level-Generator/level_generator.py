#!/usr/bin/env python3
"""
Procedural Level Generator for Tile-based Game
Generates levels with main paths, enemies, and exports to JSON format.
"""

import random
import json
import argparse
from typing import List, Tuple, Dict, Any


class LevelGenerator:
    def __init__(self, width: int, height: int, path_width: int, num_enemies: int, num_path_turns: int):
        self.width = width
        self.height = height
        self.path_width = max(1, path_width)  # Ensure minimum width of 1
        self.num_enemies = num_enemies
        self.num_path_turns = num_path_turns
        
        # Initialize grid with all unwalkable tiles (0)
        self.grid = [[0 for _ in range(width)] for _ in range(height)]
        
        # Start and end positions
        self.start_x, self.start_y = 0, 0
        self.end_x, self.end_y = 0, 0
        
        # Track main path tiles for enemy placement
        self.main_path_tiles = set()
        
    def is_valid_position(self, x: int, y: int) -> bool:
        """Check if position is within grid bounds."""
        return 0 <= x < self.width and 0 <= y < self.height
    
    def set_walkable_area(self, x: int, y: int, width: int = 1, height: int = 1):
        """Set a rectangular area as walkable."""
        for dy in range(height):
            for dx in range(width):
                nx, ny = x + dx, y + dy
                if self.is_valid_position(nx, ny):
                    self.grid[ny][nx] = 1
    
    def generate_main_path(self):
        """Generate the main path from start to end with specified turns."""
        # Choose start position on one edge
        edge = random.choice(['top', 'bottom', 'left', 'right'])
        
        if edge == 'top':
            self.start_x = random.randint(0, self.width - 1)
            self.start_y = 0
        elif edge == 'bottom':
            self.start_x = random.randint(0, self.width - 1)
            self.start_y = self.height - 1
        elif edge == 'left':
            self.start_x = 0
            self.start_y = random.randint(0, self.height - 1)
        else:  # right
            self.start_x = self.width - 1
            self.start_y = random.randint(0, self.height - 1)
        
        # Choose end position on opposite edge
        if edge in ['top', 'bottom']:
            self.end_x = random.randint(0, self.width - 1)
            self.end_y = self.height - 1 if edge == 'top' else 0
        else:  # left or right
            self.end_x = self.width - 1 if edge == 'left' else 0
            self.end_y = random.randint(0, self.height - 1)
        
        # Generate path with turns
        current_x, current_y = self.start_x, self.start_y
        path_points = [(current_x, current_y)]
        
        # Create intermediate points for turns
        for _ in range(self.num_path_turns):
            # Generate a random intermediate point
            intermediate_x = random.randint(self.path_width, self.width - self.path_width - 1)
            intermediate_y = random.randint(self.path_width, self.height - self.path_width - 1)
            path_points.append((intermediate_x, intermediate_y))
        
        path_points.append((self.end_x, self.end_y))
        
        # Draw path segments between consecutive points
        for i in range(len(path_points) - 1):
            self._draw_path_segment(path_points[i], path_points[i + 1])
        
        # Add some alternative branches and open areas
        self._add_alternative_paths()
    
    def _draw_path_segment(self, start: Tuple[int, int], end: Tuple[int, int]):
        """Draw a path segment between two points."""
        x1, y1 = start
        x2, y2 = end
        
        # Use L-shaped path (horizontal then vertical or vice versa)
        if random.choice([True, False]):
            # Horizontal first, then vertical
            self._draw_horizontal_path(x1, y1, x2)
            self._draw_vertical_path(x2, y1, y2)
        else:
            # Vertical first, then horizontal
            self._draw_vertical_path(x1, y1, y2)
            self._draw_horizontal_path(x1, y2, x2)
    
    def _draw_horizontal_path(self, x1: int, y: int, x2: int):
        """Draw horizontal path with specified width."""
        start_x, end_x = min(x1, x2), max(x1, x2)
        for x in range(start_x, end_x + 1):
            for width_offset in range(-(self.path_width // 2), self.path_width - (self.path_width // 2)):
                ny = y + width_offset
                if self.is_valid_position(x, ny):
                    self.grid[ny][x] = 1
                    self.main_path_tiles.add((x, ny))
    
    def _draw_vertical_path(self, x: int, y1: int, y2: int):
        """Draw vertical path with specified width."""
        start_y, end_y = min(y1, y2), max(y1, y2)
        for y in range(start_y, end_y + 1):
            for width_offset in range(-(self.path_width // 2), self.path_width - (self.path_width // 2)):
                nx = x + width_offset
                if self.is_valid_position(nx, y):
                    self.grid[y][nx] = 1
                    self.main_path_tiles.add((nx, y))
    
    def _add_alternative_paths(self):
        """Add some alternative branches and open areas."""
        num_branches = random.randint(2, 4)
        
        for _ in range(num_branches):
            if self.main_path_tiles:
                # Pick a random point on the main path
                branch_start = random.choice(list(self.main_path_tiles))
                
                # Create a small branch or open area
                if random.choice([True, False]):
                    # Create a small branch
                    direction = random.choice([(1, 0), (-1, 0), (0, 1), (0, -1)])
                    length = random.randint(3, 6)
                    
                    x, y = branch_start
                    for i in range(length):
                        x += direction[0]
                        y += direction[1]
                        if self.is_valid_position(x, y):
                            self.grid[y][x] = 1
                        else:
                            break
                else:
                    # Create a small open area (2x2 or 3x3)
                    size = random.choice([2, 3])
                    x, y = branch_start
                    self.set_walkable_area(x - 1, y - 1, size, size)
    
    def generate_enemies(self) -> List[Dict[str, int]]:
        """Generate enemy patrol routes that intersect the main path."""
        enemies = []
        
        # Get all walkable tiles not on the main path for enemy spawning
        non_path_walkable = []
        for y in range(self.height):
            for x in range(self.width):
                if self.grid[y][x] == 1 and (x, y) not in self.main_path_tiles:
                    non_path_walkable.append((x, y))
        
        # If not enough non-path walkable tiles, create some additional areas
        if len(non_path_walkable) < self.num_enemies * 2:
            self._create_additional_walkable_areas()
            non_path_walkable = []
            for y in range(self.height):
                for x in range(self.width):
                    if self.grid[y][x] == 1 and (x, y) not in self.main_path_tiles:
                        non_path_walkable.append((x, y))
        
        for _ in range(self.num_enemies):
            # Find two points for enemy patrol that will intersect main path
            enemy = self._create_enemy_patrol_route(non_path_walkable)
            if enemy:
                enemies.append(enemy)
        
        return enemies
    
    def _create_additional_walkable_areas(self):
        """Create additional walkable areas for enemy placement."""
        for _ in range(self.num_enemies):
            # Create small walkable areas near the main path
            if self.main_path_tiles:
                main_tile = random.choice(list(self.main_path_tiles))
                
                # Create a small area nearby
                for _ in range(5):  # Try up to 5 times to find a good spot
                    offset_x = random.randint(-5, 5)
                    offset_y = random.randint(-5, 5)
                    x = main_tile[0] + offset_x
                    y = main_tile[1] + offset_y
                    
                    if self.is_valid_position(x, y) and (x, y) not in self.main_path_tiles:
                        # Create a small 2x2 walkable area
                        self.set_walkable_area(x, y, 2, 2)
                        break
    
    def _create_enemy_patrol_route(self, non_path_walkable: List[Tuple[int, int]]) -> Dict[str, int]:
        """Create an enemy patrol route that intersects the main path."""
        if len(non_path_walkable) < 2:
            return None
        
        # Try to find two points where a line between them crosses the main path
        max_attempts = 20
        for _ in range(max_attempts):
            start_pos = random.choice(non_path_walkable)
            end_pos = random.choice(non_path_walkable)
            
            if start_pos != end_pos:
                # Check if the line between these points intersects the main path
                if self._line_intersects_main_path(start_pos, end_pos):
                    return {
                        "startX": start_pos[0],
                        "startY": start_pos[1],
                        "endX": end_pos[0],
                        "endY": end_pos[1]
                    }
        
        # If no intersection found, create a simple patrol route and make sure it can reach main path
        if non_path_walkable:
            start_pos = random.choice(non_path_walkable)
            end_pos = random.choice(non_path_walkable)
            
            # Create a connection to the main path if needed
            nearest_main_tile = min(self.main_path_tiles, 
                                  key=lambda tile: abs(tile[0] - start_pos[0]) + abs(tile[1] - start_pos[1]))
            
            # Draw a simple connection
            self._draw_simple_connection(start_pos, nearest_main_tile)
            
            return {
                "startX": start_pos[0],
                "startY": start_pos[1],
                "endX": end_pos[0],
                "endY": end_pos[1]
            }
        
        return None
    
    def _line_intersects_main_path(self, start: Tuple[int, int], end: Tuple[int, int]) -> bool:
        """Check if a line between two points intersects the main path."""
        x1, y1 = start
        x2, y2 = end
        
        # Simple line traversal using Bresenham-like algorithm
        dx = abs(x2 - x1)
        dy = abs(y2 - y1)
        x, y = x1, y1
        
        x_inc = 1 if x1 < x2 else -1
        y_inc = 1 if y1 < y2 else -1
        
        error = dx - dy
        
        steps = max(dx, dy)
        for _ in range(steps + 1):
            if (x, y) in self.main_path_tiles:
                return True
            
            error2 = error * 2
            if error2 > -dy:
                error -= dy
                x += x_inc
            if error2 < dx:
                error += dx
                y += y_inc
        
        return False
    
    def _draw_simple_connection(self, start: Tuple[int, int], end: Tuple[int, int]):
        """Draw a simple walkable connection between two points."""
        x1, y1 = start
        x2, y2 = end
        
        # Draw L-shaped connection
        # Horizontal first
        for x in range(min(x1, x2), max(x1, x2) + 1):
            if self.is_valid_position(x, y1):
                self.grid[y1][x] = 1
        
        # Then vertical
        for y in range(min(y1, y2), max(y1, y2) + 1):
            if self.is_valid_position(x2, y):
                self.grid[y][x2] = 1
    
    def export_to_json(self, filename: str = "generated_level.json") -> Dict[str, Any]:
        """Export the generated level to JSON format."""
        level_data = {
            "width": self.width,
            "height": self.height,
            "tiles": [{"row": row} for row in self.grid],
            "startTileX": self.start_x,
            "startTileY": self.start_y,
            "endTileX": self.end_x,
            "endTileY": self.end_y,
            "enemies": self.generate_enemies()
        }
        
        with open(filename, 'w') as f:
            json.dump(level_data, f, indent=2)
        
        return level_data
    
    def generate_level(self) -> Dict[str, Any]:
        """Generate a complete level and return the data."""
        print(f"Generating level: {self.width}x{self.height}, path_width={self.path_width}, "
              f"enemies={self.num_enemies}, turns={self.num_path_turns}")
        
        # Generate main path
        self.generate_main_path()
        
        # Export to JSON
        level_data = self.export_to_json()
        
        print(f"Level generated successfully!")
        print(f"Start: ({self.start_x}, {self.start_y})")
        print(f"End: ({self.end_x}, {self.end_y})")
        print(f"Enemies placed: {len(level_data['enemies'])}")
        
        return level_data


def main():
    parser = argparse.ArgumentParser(description='Generate procedural levels for tile-based game')
    parser.add_argument('--map_width', type=int, default=20, help='Width of the grid')
    parser.add_argument('--map_height', type=int, default=15, help='Height of the grid')
    parser.add_argument('--path_width', type=int, default=2, help='Width of the main path')
    parser.add_argument('--num_enemies', type=int, default=3, help='Number of enemies to place')
    parser.add_argument('--num_path_turns', type=int, default=2, help='Number of turns in the main path')
    parser.add_argument('--output', type=str, default='generated_level.json', help='Output JSON file')
    
    args = parser.parse_args()
    
    # Validate input parameters
    if args.map_width < 5 or args.map_height < 5:
        print("Error: Map dimensions must be at least 5x5")
        return
    
    if args.path_width < 1:
        print("Error: Path width must be at least 1")
        return
    
    if args.num_enemies < 0:
        print("Error: Number of enemies cannot be negative")
        return
    
    if args.num_path_turns < 0:
        print("Error: Number of path turns cannot be negative")
        return
    
    # Generate the level
    generator = LevelGenerator(
        args.map_width, 
        args.map_height, 
        args.path_width, 
        args.num_enemies, 
        args.num_path_turns
    )
    
    level_data = generator.generate_level()
    
    # Save with custom filename if specified
    if args.output != 'generated_level.json':
        with open(args.output, 'w') as f:
            json.dump(level_data, f, indent=2)
        print(f"Level saved to {args.output}")


if __name__ == "__main__":
    main()
