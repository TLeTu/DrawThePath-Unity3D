import json
import random
import heapq
import argparse

class LevelConfig:
    def __init__(self, level_number, seed, max_size=16):
        self.level_number = level_number
        self.seed = f"{seed}_{level_number}"
        random.seed(self.seed)
        
        self.curve = min(1.0, level_number / 50.0)
        self.size = min(max_size, int(8 + (level_number * 0.3)))
        self.width = self.size
        self.height = self.size
        
        if self.curve < 0.3:    
            self.waypoints = 1
            self.path_width = 2
            self.rooms_on_path = 1
            self.branches = 1
        elif self.curve < 0.7:  
            self.waypoints = 3
            self.path_width = 1
            self.rooms_on_path = 2
            self.branches = 2
        else:                   
            self.waypoints = 4
            self.path_width = 1
            self.rooms_on_path = 3
            self.branches = 3  
        
        self.enemy_count = int(1 + (self.curve * 6))
        self.max_terrain_cost = int(10 + (self.curve * 100))

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
        
        self.start_pos = (0, 0)
        self.end_pos = (config.height - 1, config.width - 1)

    def generate(self):
        self._generate_topography()
        self._carve_waypoint_path()
        self._thicken_path()
        self._carve_rooms()
        self._add_branches_with_terminal_rooms()
        self._place_enemies_sweeping()
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

    def _carve_waypoint_path(self):
        waypoints = []
        for _ in range(self.config.waypoints):
            wr = random.randint(2, self.config.height - 3)
            wc = random.randint(2, self.config.width - 3)
            waypoints.append((wr, wc))
            
        random.shuffle(waypoints)
        sequence = [self.start_pos] + waypoints + [self.end_pos]
        
        for i in range(len(sequence) - 1):
            self._astar_segment(sequence[i], sequence[i+1])

    def _astar_segment(self, start, end):
        nodes = [[Node(r, c, self.cost_map[r][c]) for c in range(self.config.width)] for r in range(self.config.height)]
        start_node = nodes[start[0]][start[1]]
        end_node = nodes[end[0]][end[1]]
        start_node.g = 0
        open_set = []
        heapq.heappush(open_set, start_node)
        closed_set = set()

        while open_set:
            current = heapq.heappop(open_set)
            if (current.row, current.col) == (end_node.row, end_node.col):
                segment_path = []
                while current:
                    segment_path.append((current.row, current.col))
                    self.grid[current.row][current.col] = 1
                    current = current.parent
                segment_path.reverse()
                self.path_nodes.extend(segment_path)
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

    def _carve_rooms(self):
        if len(self.path_nodes) < 10: return
        room_centers = random.sample(self.path_nodes[5:-5], min(self.config.rooms_on_path, len(self.path_nodes)-10))
        for r, c in room_centers:
            w, h = random.randint(2, 4), random.randint(2, 4)
            start_r, end_r = max(1, r - h//2), min(self.config.height - 2, r + h//2)
            start_c, end_c = max(1, c - w//2), min(self.config.width - 2, c + w//2)
            
            for rr in range(start_r, end_r + 1):
                for cc in range(start_c, end_c + 1):
                    self.grid[rr][cc] = 1

    def _add_branches_with_terminal_rooms(self):
        if self.config.branches <= 0 or len(self.path_nodes) < 10: return
        for _ in range(self.config.branches):
            start_idx = random.randint(3, len(self.path_nodes) - 3)
            r, c = self.path_nodes[start_idx]
            branch_length = random.randint(3, 5)
            dr, dc = random.choice([(-1, 0), (1, 0), (0, -1), (0, 1)])
            
            for _ in range(branch_length):
                r += dr
                c += dc
                if 1 <= r < self.config.height - 1 and 1 <= c < self.config.width - 1:
                    self.grid[r][c] = 1
                    if random.random() < 0.3: dr, dc = random.choice([(-1, 0), (1, 0), (0, -1), (0, 1)])
                else: r -= dr; c -= dc; break 
            
            w, h = random.randint(2, 3), random.randint(2, 3)
            start_r, end_r = max(1, r - h//2), min(self.config.height - 2, r + h//2)
            start_c, end_c = max(1, c - w//2), min(self.config.width - 2, c + w//2)
            
            for rr in range(start_r, end_r + 1):
                for cc in range(start_c, end_c + 1):
                    self.grid[rr][cc] = 1

    def _place_enemies_sweeping(self):
        def is_safe(pr, pc):
            return (abs(pr - self.start_pos[0]) + abs(pc - self.start_pos[1]) > 3) and \
                   (abs(pr - self.end_pos[0]) + abs(pc - self.end_pos[1]) > 3)

        candidate_spots = [(r, c) for r in range(self.config.height) for c in range(self.config.width) if self.grid[r][c] == 1 and is_safe(r, c)]
        random.shuffle(candidate_spots)
        
        used_patrol_tiles = set()
        enemies_placed = 0
        
        # INCREASED to 5: Enemies are now mathematically forced to take LONG patrols
        MIN_PATROL_LENGTH = 5 
        
        for r, c in candidate_spots:
            if enemies_placed >= self.config.enemy_count: break
            if (r, c) in used_patrol_tiles: continue

            # Raycast Horizontally
            left_c = c
            while left_c > 0 and self.grid[r][left_c - 1] == 1 and is_safe(r, left_c - 1) and (r, left_c - 1) not in used_patrol_tiles:
                left_c -= 1
            right_c = c
            while right_c < self.config.width - 1 and self.grid[r][right_c + 1] == 1 and is_safe(r, right_c + 1) and (r, right_c + 1) not in used_patrol_tiles:
                right_c += 1
            horiz_len = right_c - left_c + 1

            # Raycast Vertically
            up_r = r
            while up_r > 0 and self.grid[up_r - 1][c] == 1 and is_safe(up_r - 1, c) and (up_r - 1, c) not in used_patrol_tiles:
                up_r -= 1
            down_r = r
            while down_r < self.config.height - 1 and self.grid[down_r + 1][c] == 1 and is_safe(down_r + 1, c) and (down_r + 1, c) not in used_patrol_tiles:
                down_r += 1
            vert_len = down_r - up_r + 1

            if horiz_len >= MIN_PATROL_LENGTH and horiz_len >= vert_len:
                for i in range(left_c, right_c + 1): 
                    used_patrol_tiles.add((r, i))
                self.enemies.append({"startX": r, "startY": left_c, "endX": r, "endY": right_c})
                enemies_placed += 1
            elif vert_len >= MIN_PATROL_LENGTH and vert_len > horiz_len:
                for i in range(up_r, down_r + 1): 
                    used_patrol_tiles.add((i, c))
                self.enemies.append({"startX": up_r, "startY": c, "endX": down_r, "endY": c})
                enemies_placed += 1

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
        print(f"Success! Generated {args.end - args.start + 1} levels.")
    else:
        print(f"Generating Level {args.level}...")
        config = LevelConfig(args.level, args.seed)
        generator = LevelGenerator(config)
        json_output = generator.generate()
        filename = f"Level{args.level}.json"
        with open(filename, "w") as f:
            f.write(json_output)
        print(f"Success! Saved to {filename}")
        print(f"Grid: {config.width}x{config.height} | Enemies: {len(generator.enemies)}")

if __name__ == "__main__":
    main()