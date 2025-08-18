import json
import random
from typing import List, Tuple, Dict, Set

class LevelGenerator:
    def __init__(self, width: int, height: int, path_width: int, num_enemies: int, num_turns: int):
        self.width = width
        self.height = height
        self.path_width = path_width
        self.num_enemies = num_enemies
        self.num_turns = num_turns
        self.grid = [[0 for _ in range(width)] for _ in range(height)]
        self.main_path_cells = set()
        
    def generate_level(self) -> Dict:
        """Main function to generate a complete level"""
        # Step 1: Generate enemy coordinates first (as requested)
        enemies = self._generate_enemies()
        
        # Step 2: Create the main path with turns
        self._create_main_path()
        
        # Step 3: Create enemy paths that cross the main path
        self._create_enemy_paths(enemies)
        
        # Step 4: Add open areas (2x2, 3x3 squares)
        self._add_open_areas()
        
        # Step 5: Add shortcuts and loops
        self._add_shortcuts()
        
        # Step 6: Ensure everything is connected
        self._ensure_connectivity()
        
        return self._format_output(enemies)
    
    def _generate_enemies(self) -> List[Dict]:
        """Generate enemy coordinates first, before building paths"""
        enemies = []
        
        for i in range(self.num_enemies):
            # Random enemy type: horizontal (along row), vertical (along column), or diagonal
            enemy_type = random.choice(['horizontal', 'vertical', 'diagonal', 'circular'])
            
            if enemy_type == 'horizontal':
                # Enemy moves horizontally across a row
                row = random.randint(2, self.height - 3)
                enemies.append({
                    'startX': row,
                    'startY': 1,  # Start slightly inward from edge
                    'endX': row,
                    'endY': self.width - 2  # End slightly inward from edge
                })
            
            elif enemy_type == 'vertical':
                # Enemy moves vertically down a column
                col = random.randint(2, self.width - 3)
                enemies.append({
                    'startX': 1,  # Start slightly inward from edge
                    'startY': col,
                    'endX': self.height - 2,  # End slightly inward from edge
                    'endY': col
                })
            
            elif enemy_type == 'diagonal':
                # Enemy moves diagonally - ensure reasonable distance
                start_x = random.randint(1, self.height // 2)
                start_y = random.randint(1, self.width // 2)
                end_x = random.randint(self.height // 2, self.height - 2)
                end_y = random.randint(self.width // 2, self.width - 2)
                enemies.append({
                    'startX': start_x,
                    'startY': start_y,
                    'endX': end_x,
                    'endY': end_y
                })
            
            else:  # circular
                # Enemy moves in a circular/rectangular pattern
                center_x = random.randint(4, self.height - 5)
                center_y = random.randint(4, self.width - 5)
                radius = random.randint(2, 3)
                enemies.append({
                    'startX': max(1, center_x - radius),
                    'startY': max(1, center_y - radius),
                    'endX': min(self.height - 2, center_x + radius),
                    'endY': min(self.width - 2, center_y + radius)
                })
        
        return enemies
    
    def _create_main_path(self):
        """Create main path from (0,0) to (height-1, width-1) with specified turns"""
        start_x, start_y = 0, 0
        end_x, end_y = self.height - 1, self.width - 1
        
        # Create waypoints for turns
        waypoints = [(start_x, start_y)]
        
        # Generate intermediate waypoints for turns
        for turn in range(self.num_turns):
            # Distribute turns evenly along the path
            progress = (turn + 1) / (self.num_turns + 1)
            
            # Calculate intermediate position with some randomness
            mid_x = int(start_x + (end_x - start_x) * progress)
            mid_y = int(start_y + (end_y - start_y) * progress)
            
            # Add randomness for more interesting paths
            mid_x += random.randint(-3, 3)
            mid_y += random.randint(-3, 3)
            
            # Keep within bounds
            mid_x = max(1, min(self.height - 2, mid_x))
            mid_y = max(1, min(self.width - 2, mid_y))
            
            waypoints.append((mid_x, mid_y))
        
        waypoints.append((end_x, end_y))
        
        # Connect waypoints with L-shaped segments (for 90-degree turns)
        for i in range(len(waypoints) - 1):
            x1, y1 = waypoints[i]
            x2, y2 = waypoints[i + 1]
            
            # Create L-shaped path (horizontal first, then vertical)
            self._draw_path_segment(x1, y1, x2, y1)  # Horizontal
            self._draw_path_segment(x2, y1, x2, y2)  # Vertical
    
    def _draw_path_segment(self, x1: int, y1: int, x2: int, y2: int):
        """Draw a path segment between two points"""
        # Determine direction and draw line
        if x1 == x2:  # Vertical line
            start_y, end_y = min(y1, y2), max(y1, y2)
            for y in range(start_y, end_y + 1):
                self._place_path_tiles(x1, y)
        else:  # Horizontal line
            start_x, end_x = min(x1, x2), max(x1, x2)
            for x in range(start_x, end_x + 1):
                self._place_path_tiles(x, y1)
    
    def _place_path_tiles(self, center_x: int, center_y: int):
        """Place path tiles with specified width around center point"""
        half_width = self.path_width // 2
        
        for dx in range(-half_width, half_width + 1):
            for dy in range(-half_width, half_width + 1):
                x, y = center_x + dx, center_y + dy
                if 0 <= x < self.height and 0 <= y < self.width:
                    self.grid[x][y] = 1
                    self.main_path_cells.add((x, y))
    
    def _create_enemy_paths(self, enemies: List[Dict]):
        """Create paths for enemies that cross the main path"""
        for enemy in enemies:
            start_x, start_y = enemy['startX'], enemy['startY']
            end_x, end_y = enemy['endX'], enemy['endY']
            
            # Ensure start and end points are walkable
            self.grid[start_x][start_y] = 1
            self.grid[end_x][end_y] = 1
            
            # Decide if enemy path should have turns (30% chance)
            if random.random() < 0.3:
                # Create path with turns
                self._create_enemy_path_with_turns(start_x, start_y, end_x, end_y)
            else:
                # Create straight path
                self._create_straight_enemy_path(start_x, start_y, end_x, end_y)
            
            # Validate that path is walkable, if not create emergency path
            if not self._is_path_walkable(start_x, start_y, end_x, end_y):
                print(f"Warning: Enemy path not walkable, creating emergency path from ({start_x},{start_y}) to ({end_x},{end_y})")
                self._create_straight_enemy_path(start_x, start_y, end_x, end_y)
    
    def _create_straight_enemy_path(self, x1: int, y1: int, x2: int, y2: int):
        """Create a straight enemy path"""
        # Use Bresenham's line algorithm for straight line
        dx = abs(x2 - x1)
        dy = abs(y2 - y1)
        sx = 1 if x1 < x2 else -1
        sy = 1 if y1 < y2 else -1
        err = dx - dy
        
        x, y = x1, y1
        
        while True:
            if 0 <= x < self.height and 0 <= y < self.width:
                self.grid[x][y] = 1
            
            if x == x2 and y == y2:
                break
                
            e2 = 2 * err
            if e2 > -dy:
                err -= dy
                x += sx
            if e2 < dx:
                err += dx
                y += sy
    
    def _create_enemy_path_with_turns(self, x1: int, y1: int, x2: int, y2: int):
        """Create enemy path with 1-2 turns"""
        num_turns = random.randint(1, 2)
        
        # Create intermediate waypoints
        waypoints = [(x1, y1)]
        for turn in range(num_turns):
            progress = (turn + 1) / (num_turns + 1)
            mid_x = int(x1 + (x2 - x1) * progress) + random.randint(-2, 2)
            mid_y = int(y1 + (y2 - y1) * progress) + random.randint(-2, 2)
            mid_x = max(0, min(self.height - 1, mid_x))
            mid_y = max(0, min(self.width - 1, mid_y))
            waypoints.append((mid_x, mid_y))
        waypoints.append((x2, y2))
        
        # Connect waypoints
        for i in range(len(waypoints) - 1):
            self._create_straight_enemy_path(waypoints[i][0], waypoints[i][1], 
                                           waypoints[i + 1][0], waypoints[i + 1][1])
    
    def _add_open_areas(self):
        """Add random open areas (2x2, 3x3, etc.) connected to main path"""
        num_areas = random.randint(2, 5)
        
        for _ in range(num_areas):
            # Pick a random point on the main path as anchor
            if not self.main_path_cells:
                continue
                
            anchor_x, anchor_y = random.choice(list(self.main_path_cells))
            
            # Create open area of random size
            area_size = random.choice([2, 3, 4])
            
            # Offset the area from the anchor point
            offset_x = random.randint(-2, 2)
            offset_y = random.randint(-2, 2)
            
            start_x = max(0, anchor_x + offset_x)
            start_y = max(0, anchor_y + offset_y)
            end_x = min(self.height, start_x + area_size)
            end_y = min(self.width, start_y + area_size)
            
            # Fill the area with walkable tiles
            for x in range(start_x, end_x):
                for y in range(start_y, end_y):
                    if 0 <= x < self.height and 0 <= y < self.width:
                        self.grid[x][y] = 1
    
    def _add_shortcuts(self):
        """Add shortcuts and loops that connect back to main path"""
        num_shortcuts = random.randint(1, 4)
        
        for _ in range(num_shortcuts):
            if len(self.main_path_cells) < 2:
                continue
                
            # Pick two random points on the main path
            path_list = list(self.main_path_cells)
            point1 = random.choice(path_list)
            point2 = random.choice(path_list)
            
            # Make sure points are reasonably far apart
            if abs(point1[0] - point2[0]) + abs(point1[1] - point2[1]) < 4:
                continue
            
            # Create shortcut
            if random.random() < 0.6:
                # Direct shortcut
                self._create_straight_enemy_path(point1[0], point1[1], point2[0], point2[1])
            else:
                # Loop shortcut (with intermediate point)
                mid_x = (point1[0] + point2[0]) // 2 + random.randint(-3, 3)
                mid_y = (point1[1] + point2[1]) // 2 + random.randint(-3, 3)
                mid_x = max(0, min(self.height - 1, mid_x))
                mid_y = max(0, min(self.width - 1, mid_y))
                
                self._create_straight_enemy_path(point1[0], point1[1], mid_x, mid_y)
                self._create_straight_enemy_path(mid_x, mid_y, point2[0], point2[1])
    
    def _is_path_walkable(self, x1: int, y1: int, x2: int, y2: int) -> bool:
        """Check if there's a walkable path between two points using BFS"""
        if not (0 <= x1 < self.height and 0 <= y1 < self.width and 
                0 <= x2 < self.height and 0 <= y2 < self.width):
            return False
        
        if self.grid[x1][y1] == 0 or self.grid[x2][y2] == 0:
            return False
        
        # BFS to find path
        visited = set()
        queue = [(x1, y1)]
        
        while queue:
            x, y = queue.pop(0)
            
            if (x, y) in visited:
                continue
            if x < 0 or x >= self.height or y < 0 or y >= self.width:
                continue
            if self.grid[x][y] == 0:
                continue
                
            visited.add((x, y))
            
            # Found target
            if x == x2 and y == y2:
                return True
            
            # Add neighbors to queue
            for dx, dy in [(0, 1), (0, -1), (1, 0), (-1, 0)]:
                queue.append((x + dx, y + dy))
        
        return False

    def _ensure_connectivity(self):
        """Ensure start and end points are connected using flood fill"""
        # Flood fill from start position
        visited = set()
        stack = [(0, 0)]
        
        while stack:
            x, y = stack.pop()
            
            if (x, y) in visited:
                continue
            if x < 0 or x >= self.height or y < 0 or y >= self.width:
                continue
            if self.grid[x][y] == 0:
                continue
                
            visited.add((x, y))
            
            # Add neighbors to stack
            for dx, dy in [(0, 1), (0, -1), (1, 0), (-1, 0)]:
                stack.append((x + dx, y + dy))
        
        # If end point is not reachable, create emergency path
        if (self.height - 1, self.width - 1) not in visited:
            print("Warning: End point not reachable, creating emergency path...")
            self._create_straight_enemy_path(0, 0, self.height - 1, self.width - 1)
    
    def _format_output(self, enemies: List[Dict]) -> Dict:
        """Format output to match the level file structure"""
        return {
            "width": self.width,
            "height": self.height,
            "tiles": [{"row": row} for row in self.grid],
            "startTileX": 0,
            "startTileY": 0,
            "endTileX": self.height - 1,
            "endTileY": self.width - 1,
            "enemies": enemies
        }

def generate_level_file(width: int, height: int, path_width: int, num_enemies: int, num_turns: int, output_file: str):
    """Generate a level and save it to a JSON file"""
    print(f"Generating level: {width}x{height}, path_width={path_width}, enemies={num_enemies}, turns={num_turns}")
    
    generator = LevelGenerator(width, height, path_width, num_enemies, num_turns)
    level_data = generator.generate_level()
    
    with open(output_file, 'w') as f:
        json.dump(level_data, f, indent=4)
    
    print(f"Level successfully generated and saved to: {output_file}")
    
    # Print some stats
    total_walkable = sum(tile['row'].count(1) for tile in level_data['tiles'])
    total_tiles = width * height
    walkable_percentage = (total_walkable / total_tiles) * 100
    print(f"Stats: {total_walkable}/{total_tiles} tiles walkable ({walkable_percentage:.1f}%)")

def main():
    """Main function with user input"""
    print("=== Level Generator ===")
    print("This generator creates levels similar to Level1.json format")
    print()
    
    try:
        width = int(input("Enter map width (e.g., 12): "))
        height = int(input("Enter map height (e.g., 12): "))
        path_width = int(input("Enter main path width (e.g., 1): "))
        num_enemies = int(input("Enter number of enemies (e.g., 3): "))
        num_turns = int(input("Enter number of main path turns (e.g., 2): "))
        output_file = input("Enter output filename (e.g., Level_Generated.json): ")
        
        if not output_file.endswith('.json'):
            output_file += '.json'
        
        generate_level_file(width, height, path_width, num_enemies, num_turns, output_file)
        
    except ValueError:
        print("Error: Please enter valid numbers for numeric inputs")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
