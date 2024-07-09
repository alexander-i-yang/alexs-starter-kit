using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter
{
    public class ImpactSpriteShatterVBehavior : ISpriteShatterVBehavior
    {
        public float CalculateVelocity(Vector2 inputForce, Triangle triangle, Vector2 forcePosition, Vector2 trianglePosition)
        {
            return (forcePosition - trianglePosition).magnitude;
        }
    }
}