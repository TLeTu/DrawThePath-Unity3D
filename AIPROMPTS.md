#### Combine the cubes mesh for optimization
Implement a Unity script that optimizes a large grid of cube GameObjects by combining their meshes at runtime to reduce draw calls. The script should:

Work with any number of cube tiles that are children of a parent GameObject.

Group tiles by shared material, so different tile types (grass, water, etc.) are preserved.

Use Mesh.CombineMeshes() for each material group.

Disable the original cube GameObjects after combining.

Keep the combined meshes as separate GameObjects named after the material they represent.

Be able to run either automatically on Start() or via a public method callable in the editor.

Include comments explaining each step.

Assume tiles will not move at runtime (static ground).

Ensure it works even if cubes have different positions, rotations, or scales.
#### Get tile in the combined mesh
Implement a Unity script that allows selecting individual tiles on a combined mesh grid using raycasts and math, without keeping separate GameObjects for each tile. The script should:

Work with a grid defined by gridWidth, gridHeight, and tileSize.

Assume the grid is centered at (0, 0, 0) in world space.

When the player clicks (mouse) or taps (touch), cast a ray from the camera.

If the ray hits the mesh’s collider, calculate the (row, col) coordinates of the tile hit using the hit point, grid origin, and tile size.

Return the center world position of the selected tile for movement or other logic.

Include methods:

Vector2Int WorldToGrid(Vector3 worldPos) — converts world position to grid coordinates.

Vector3 GridToWorld(Vector2Int gridPos) — converts grid coordinates to the center world position.

Support both mouse and touch input in Update().

Include comments explaining the math for coordinate conversion.