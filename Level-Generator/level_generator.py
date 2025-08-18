import json
import random
import sys
from collections import deque

# --- Utility Functions ---
def in_bounds(x, y, width, height):
    return 0 <= x < width and 0 <= y < height

def neighbors(x, y, width, height):
    for dx, dy in [(-1,0),(1,0),(0,-1),(0,1)]:
        nx, ny = x+dx, y+dy
        if in_bounds(nx, ny, width, height):
            yield nx, ny

def manhattan(a, b):
    return abs(a[0]-b[0]) + abs(a[1]-b[1])

# --- Main Path Generation ---
def generate_main_path(width, height, path_width, num_turns):
    # Pick random start/end on opposite edges
    if random.choice([True, False]):
        start = (0, random.randint(0, height-1))
        end = (width-1, random.randint(0, height-1))
        horizontal = True
    else:
        start = (random.randint(0, width-1), 0)
        end = (random.randint(0, width-1), height-1)
        horizontal = False
    
    # Generate waypoints for turns
    waypoints = [start]
    for i in range(num_turns):
        if horizontal:
            x = random.randint(1, width-2)
            y = random.randint(0, height-1)
        else:
            x = random.randint(0, width-1)
            y = random.randint(1, height-2)
        waypoints.append((x, y))
    waypoints.append(end)
    # Sort waypoints to avoid backtracking
    if horizontal:
        waypoints = sorted(waypoints, key=lambda p: p[0])
    else:
        waypoints = sorted(waypoints, key=lambda p: p[1])
    
    # Carve path between waypoints
    grid = [[0 for _ in range(width)] for _ in range(height)]
    main_path_set = set()
    for i in range(len(waypoints)-1):
        x0, y0 = waypoints[i]
        x1, y1 = waypoints[i+1]
        # L-shape: horizontal then vertical or vice versa
        if random.choice([True, False]):
            for x in range(min(x0,x1), max(x0,x1)+1):
                for dx in range(-(path_width//2), path_width-(path_width//2)):
                    nx = x+dx
                    if in_bounds(nx, y0, width, height):
                        grid[y0][nx] = 1
                        main_path_set.add((nx, y0))
            for y in range(min(y0,y1), max(y0,y1)+1):
                for dy in range(-(path_width//2), path_width-(path_width//2)):
                    ny = y+dy
                    if in_bounds(x1, ny, width, height):
                        grid[ny][x1] = 1
                        main_path_set.add((x1, ny))
        else:
            for y in range(min(y0,y1), max(y0,y1)+1):
                for dy in range(-(path_width//2), path_width-(path_width//2)):
                    ny = y+dy
                    if in_bounds(x0, ny, width, height):
                        grid[ny][x0] = 1
                        main_path_set.add((x0, ny))
            for x in range(min(x0,x1), max(x0,x1)+1):
                for dx in range(-(path_width//2), path_width-(path_width//2)):
                    nx = x+dx
                    if in_bounds(nx, y1, width, height):
                        grid[y1][nx] = 1
                        main_path_set.add((nx, y1))

    # Add random open areas/branches off the main path
    main_path_tiles = list(main_path_set)
    num_areas = random.randint(1, 2)
    for _ in range(num_areas):
        if not main_path_tiles:
            break
        bx, by = random.choice(main_path_tiles)
        # Try to add a 2x2 or 3x3 open area adjacent to the path
        area_size = random.choice([2, 3])
        for dx in range(-(area_size//2), area_size-(area_size//2)):
            for dy in range(-(area_size//2), area_size-(area_size//2)):
                nx, ny = bx+dx, by+dy
                if in_bounds(nx, ny, width, height):
                    grid[ny][nx] = 1
    return grid, start, end, main_path_set

# --- Enemy Placement ---
def find_non_path_tiles(grid, path_set):
    non_path = []
    for y, row in enumerate(grid):
        for x, v in enumerate(row):
            if v == 1 and (x, y) not in path_set:
                non_path.append((x, y))
    return non_path

def bfs_path(grid, start, end):
    width, height = len(grid[0]), len(grid)
    queue = deque([(start, [start])])
    visited = set([start])
    while queue:
        (x, y), path = queue.popleft()
        if (x, y) == end:
            return path
        for nx, ny in neighbors(x, y, width, height):
            if grid[ny][nx] == 1 and (nx, ny) not in visited:
                visited.add((nx, ny))
                queue.append(((nx, ny), path+[(nx, ny)]))
    return None

def place_enemies(grid, path_set, num_enemies, main_path):
    width, height = len(grid[0]), len(grid)
    enemies = []
    non_path_tiles = find_non_path_tiles(grid, path_set)
    if not non_path_tiles:
        print("Warning: No non-path walkable tiles available for enemy placement. No enemies will be placed.")
        return enemies
    for _ in range(num_enemies):
        # Pick patrol start/end not on main path
        tries = 0
        while tries < 100:
            start = random.choice(non_path_tiles)
            # Find a patrol end that crosses main path
            possible_ends = [t for t in non_path_tiles if t != start and manhattan(start, t) > 2]
            random.shuffle(possible_ends)
            for end in possible_ends:
                patrol = bfs_path(grid, start, end)
                if patrol and any(p in main_path for p in patrol):
                    enemies.append({
                        "startX": start[0], "startY": start[1],
                        "endX": end[0], "endY": end[1]
                    })
                    break
            if len(enemies) > 0 and len(enemies) == _+1:
                break
            tries += 1
    return enemies

# --- Main Script ---
def main():
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--map_width', type=int, required=True)
    parser.add_argument('--map_height', type=int, required=True)
    parser.add_argument('--path_width', type=int, required=True)
    parser.add_argument('--num_enemies', type=int, required=True)
    parser.add_argument('--num_path_turns', type=int, required=True)
    args = parser.parse_args()

    grid, start, end, main_path_set = generate_main_path(args.map_width, args.map_height, args.path_width, args.num_path_turns)
    # Place enemies
    enemies = place_enemies(grid, main_path_set, args.num_enemies, main_path_set)
    # Output JSON
    out = {
        "width": args.map_width,
        "height": args.map_height,
        "tiles": [{"row": row} for row in grid],
        "startTileX": start[0],
        "startTileY": start[1],
        "endTileX": end[0],
        "endTileY": end[1],
        "enemies": enemies
    }
    with open("generated_level.json", "w") as f:
        json.dump(out, f, indent=4)

if __name__ == "__main__":
    main()
