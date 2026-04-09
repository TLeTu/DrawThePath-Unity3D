import json
import random
import heapq
import argparse

class LevelConfig:
    def __init__(self, level_number, seed, max_size=16):
        self.level_number = level_number
        self.seed = f"{seed}_{level_number}"
        random.seed(self.seed)
        
        # Difficulty curve (0.0 to 1.0)
        self.curve = min(1.0, level_number / 50.0)
        
        # Grid Size (Caps at max_size)
        self.size = min(max_size, int(8 + (level_number * 0.3)))
        self.width = self.size
        self.height = self.size
        
        # Path Width & Features
        if self.curve < 0.3:    # Easy
            self.path_width = 2
            self.open_areas = 2
            self.shortcuts = 0  
            self.dead_ends = 1  # Add 1 little branch
        elif self.curve < 0.7:  # Medium
            self.path_width = 1
            self.open_areas = 1
            self.shortcuts = 2  
            self.dead_ends = 3  # Add a few confusing dead ends
        else:                   # Hard
            self.path_width = 1
            self.open_areas = 0 
            self.shortcuts = 0  
            self.dead_ends = 0  # Linear, brutal path
        
        # Enemies
        self.enemy_count = int(1 + (self.curve * 5))
        
        # Topography (Higher = more winding paths)
        self.max_terrain_cost = int(10 + (self.curve * 90))

class Node:
    def __init__(self, row, col, cost):
        self.row = row
        self.col = col
        self.cost = cost
        self.g = float('inf')
        self.h = 0
        self.parent = None

    def __lt__(self, other):
        return (self.g + self.h) < (other.g + other.h)

class LevelGenerator:
    def __init__(self, config):
        self.config = config
        self.grid = [[0 for _ in range(config.width)] for _ in range(config.height)]
        self.cost_map = []
        self.path_nodes = []
        self.enemies = []
        
        # Randomize Start along the top edge, End along the bottom edge
        self.start_pos = (0, random.randint(0, config.width - 1))
        self.end_pos = (config.height - 1, random.randint(0, config.width - 1))

    def generate(self):
        self._generate_topography()
        self._carve_path()
        self._thicken_path()
        self._add_open_areas()
        self._add_shortcuts()
        self._add_dead_ends()
        self._place_enemies()
        return self._export_json()

    def _generate_topography(self):
        raw_map = [[random.randint(1, self.config.max_terrain_cost) for _ in range(self.config.width)] for _ in range(self.config.height)]
        self.cost_map = [[1 for _ in range(self.config.width)] for _ in range(self.config.height)]
        
        for r in range(self.config.height):
            for c in range(self.config.width):
                neighbors = [raw_map[r][c]]
                if r > 0: neighbors.append(raw_map[r-1][c])
                if r < self.config.height - 1: neighbors.append(raw_map[r+1][c])
                if c > 0: neighbors.append(raw_map[r][c-1])
                if c < self.config.width - 1: neighbors.append(raw_map[r][c+1])
                self.cost_map[r][c] = sum(neighbors) // len(neighbors)

    def _carve_path(self):
        nodes = [[Node(r, c, self.cost_map[r][c]) for c in range(self.config.width)] for r in range(self.config.height)]
        start_node = nodes[self.start_pos[0]][self.start_pos[1]]
        end_node = nodes[self.end_pos[0]][self.end_pos[1]]
        
        start_node.g = 0
        open_set = []
        heapq.heappush(open_set, start_node)
        closed_set = set()

        while open_set:
            current = heapq.heappop(open_set)
            
            if (current.row, current.col) == (end_node.row, end_node.col):
                while current:
                    self.path_nodes.append((current.row, current.col))
                    self.grid[current.row][current.col] = 1
                    current = current.parent
                self.path_nodes.reverse() 
                return

            closed_set.add((current.row, current.col))

            dirs = [(-1, 0), (1, 0), (0, -1), (0, 1)]
            for dr, dc in dirs:
                nr, nc = current.row + dr, current.col + dc
                if 0 <= nr < self.config.height and 0 <= nc < self.config.width:
                    if (nr, nc) in closed_set: continue
                    
                    neighbor = nodes[nr][nc]
                    tentative_g = current.g + 1 + neighbor.cost
                    
                    if tentative_g < neighbor.g:
                        neighbor.parent = current
                        neighbor.g = tentative_g
                        neighbor.h = abs(nr - end_node.row) + abs(nc - end_node.col)
                        heapq.heappush(open_set, neighbor)

    def _thicken_path(self):
        if self.config.path_width <= 1: return
        
        new_grid = [row[:] for row in self.grid]
        for r, c in self.path_nodes:
            for dr, dc in [(-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (1, 1)]:
                nr, nc = r + dr, c + dc
                if 0 <= nr < self.config.height and 0 <= nc < self.config.width:
                    new_grid[nr][nc] = 1
        self.grid = new_grid

    def _add_open_areas(self):
        if self.config.open_areas <= 0 or len(self.path_nodes) < 10: return
        
        safe_spots = random.sample(self.path_nodes[5:-5], min(self.config.open_areas, len(self.path_nodes)-10))
        for r, c in safe_spots:
            for dr in [-1, 0, 1]:
                for dc in [-1, 0, 1]:
                    nr, nc = r + dr, c + dc
                    if 0 <= nr < self.config.height and 0 <= nc < self.config.width:
                        self.grid[nr][nc] = 1

    def _add_shortcuts(self):
        if self.config.shortcuts <= 0 or len(self.path_nodes) < 20: return
        
        for _ in range(self.config.shortcuts):
            idx1 = random.randint(5, len(self.path_nodes) // 3)
            idx2 = random.randint((len(self.path_nodes) // 3) * 2, len(self.path_nodes) - 5)
            
            curr_r, curr_c = self.path_nodes[idx1]
            target_r, target_c = self.path_nodes[idx2]
            
            while (curr_r, curr_c) != (target_r, target_c):
                if random.choice([True, False]) and curr_r != target_r:
                    curr_r += 1 if target_r > curr_r else -1
                elif curr_c != target_c:
                    curr_c += 1 if target_c > curr_c else -1
                else:
                    curr_r += 1 if target_r > curr_r else -1
                
                self.grid[curr_r][curr_c] = 1
    
    def _add_dead_ends(self):
        if self.config.dead_ends <= 0 or len(self.path_nodes) < 10: return
        
        for _ in range(self.config.dead_ends):
            # Pick a random starting point on the main path
            start_idx = random.randint(3, len(self.path_nodes) - 3)
            r, c = self.path_nodes[start_idx]
            
            # Decide how long the dead end branch will be (3 to 6 tiles long)
            branch_length = random.randint(3, 6)
            
            # Pick a random direction to start digging
            dr, dc = random.choice([(-1, 0), (1, 0), (0, -1), (0, 1)])
            
            for _ in range(branch_length):
                r += dr
                c += dc
                # Keep it within the map boundaries
                if 0 <= r < self.config.height and 0 <= c < self.config.width:
                    self.grid[r][c] = 1
                    
                    # 30% chance the branch changes direction to make it snake around
                    if random.random() < 0.3:
                        dr, dc = random.choice([(-1, 0), (1, 0), (0, -1), (0, 1)])

    def _place_enemies(self):
        if len(self.path_nodes) < 6: return
        valid_spots = self.path_nodes[3:-3] 
        random.shuffle(valid_spots)
        
        # Helper to ensure the tile is at least 2 steps away from Start and End
        def is_safe(pr, pc):
            dist_start = abs(pr - self.start_pos[0]) + abs(pc - self.start_pos[1])
            dist_end = abs(pr - self.end_pos[0]) + abs(pc - self.end_pos[1])
            return dist_start > 1 and dist_end > 1

        for _ in range(min(self.config.enemy_count, len(valid_spots))):
            r, c = valid_spots.pop()
            
            if not is_safe(r, c): 
                continue 
            
            if random.choice([True, False]):
                # Patrol Vertically, stopping if it hits a wall OR a safe zone
                start_r = r
                while start_r > 0 and self.grid[start_r - 1][c] == 1 and is_safe(start_r - 1, c):
                    start_r -= 1
                
                end_r = r
                while end_r < self.config.height - 1 and self.grid[end_r + 1][c] == 1 and is_safe(end_r + 1, c):
                    end_r += 1
                    
                if start_r != end_r:
                    self.enemies.append({"startX": start_r, "startY": c, "endX": end_r, "endY": c})
            else:
                # Patrol Horizontally, stopping if it hits a wall OR a safe zone
                start_c = c
                while start_c > 0 and self.grid[r][start_c - 1] == 1 and is_safe(r, start_c - 1):
                    start_c -= 1
                
                end_c = c
                while end_c < self.config.width - 1 and self.grid[r][end_c + 1] == 1 and is_safe(r, end_c + 1):
                    end_c += 1
                    
                if start_c != end_c:
                    self.enemies.append({"startX": r, "startY": start_c, "endX": r, "endY": end_c})

    def _export_json(self):
        output = {
            "width": self.config.width,
            "height": self.config.height,
            "tiles": [{"row": self.grid[r]} for r in range(self.config.height)],
            "startTileX": self.start_pos[0],
            "startTileY": self.start_pos[1],
            "endTileX": self.end_pos[0],
            "endTileY": self.end_pos[1],
            "enemies": self.enemies
        }
        return json.dumps(output, indent=4)

def main():
    parser = argparse.ArgumentParser(description="Unity Procedural Level Generator")
    parser.add_argument('--level', type=int, default=1, help='Difficulty level (1-100+)')
    parser.add_argument('--seed', type=str, default='my_game', help='Base seed for generation')
    
    parser.add_argument('--batch', action='store_true', help='Generate multiple levels at once')
    parser.add_argument('--start', type=int, default=1, help='Starting level for batch generation')
    parser.add_argument('--end', type=int, default=100, help='Ending level for batch generation')
    
    args = parser.parse_args()

    if args.batch:
        print(f"Batch generating levels {args.start} to {args.end}...")
        for i in range(args.start, args.end + 1):
            config = LevelConfig(i, args.seed)
            generator = LevelGenerator(config)
            json_output = generator.generate()
            filename = f"Level{i}.json"
            with open(filename, "w") as f:
                f.write(json_output)
        print(f"Success! Generated {args.end - args.start + 1} levels in the current directory.")
        
    else:
        print(f"Generating Level {args.level}...")
        config = LevelConfig(args.level, args.seed)
        generator = LevelGenerator(config)
        json_output = generator.generate()
        filename = f"Level{args.level}.json"
        with open(filename, "w") as f:
            f.write(json_output)
        print(f"Success! Saved to {filename}")
        print(f"Grid: {config.width}x{config.height} | Enemies: {config.enemy_count} | Open Areas: {config.open_areas} | Shortcuts: {config.shortcuts}")

if __name__ == "__main__":
    main()