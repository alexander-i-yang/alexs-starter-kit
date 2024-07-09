using System;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter
{
    public interface ISpriteShatterVBehavior
    {
        public float CalculateVelocity(Vector2 inputForce, Triangle triangle, Vector2 forcePosition, Vector2 trianglePosition);
    }
}