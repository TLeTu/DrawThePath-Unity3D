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