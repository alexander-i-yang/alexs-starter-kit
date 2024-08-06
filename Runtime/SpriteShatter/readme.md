# Sprite Shatter
## At a glance
Put the SpriteShatter Monobehavior on a component with a sprite
on it. Click "Bake Mesh", then call `Shatter` in the code.

## How it works
1. **Mesh generation** - I use the marching squares algorithm to automatically generate 
a polygon that encompasses the target sprite. Since some  sprites are static and have
meshes that will not change during runtime, use the "Bake Mesh" button in the inspector
to bake in the editor. Note: cannot automatically mesh porous/discontiguous sprites.
2. **Triangulation** - at runtime, I triangulate the mesh using a library called `Triangle.net`.
This splits the mesh into a list of triangles. Control triangulation using the `MaxTriangleSize`
in the SpriteShatter component.
3. **Grouping** - the list of triangles are passed into a grouper, which partitions the list
of triangles into a set of groups, each with a subset of the original input triangles. This is
where `forcePos` and `force` come into play - the two inputs control how triangles are grouped.
4. **Velocity** - the input `SpriteShatterVBehavior` calculates velocities to apply to each group.
This works similarly to the grouping step.
5. **Prefab creation** - a sprite shatter piece is created for each group. It is passed into my
Particle Pool.

## Shatter Inputs
* `Vector2 forceWorldOrigin` - world position of the impact force
* `Vector2 force` - impact force
* `IGrouper grouper` - grouper strategy to group contiguous triangles
* `ISpriteShatterVBehavior spriteShatterVBehavior` - velocity strategy to apply velocities
to each group

## Built-in groupers
Two groupers are already defined - the Chunk grouper and the Slice grouper.
1. The Chunk grouper groups triangles into evenly sized contiguous chunks.
2. The slice grouper interprets `forcePos` and `force` as a sword swipe that cuts the sprite
cleanly in half.

## Built-in Prefab
The built-in prefab that the Sprite Shatter component creates is actually a parent to smaller
Sprite Shatter pieces, one for each triangle in the group. Each piece contains a collider and
a sprite, both of which are limited to the piece's triangle. (The sprite uses a shader that
relies on Barycentric coordinates.)