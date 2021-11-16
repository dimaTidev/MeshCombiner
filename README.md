Bake system for fast mesh bake in runtime or editor
- Combine
- Combine with sort material (each bake by unique material)
- Separate submeshes
- Combine with atlasing
- Combine with atlasing color (vertex color to texture block)
- [Editor] Save meshes to asset files
- [Editor] Save texture atlas to asset
- [Editor] Extract simple colliders


# How to use
- Setup hierarchy => all meshes parented to one object (parent must be clear from meshFilter or meshRenderer)
- Drop to parent `MeshCombiner.cs`
- Select bake queue in script
- Use editor buttons if need bake in editor
