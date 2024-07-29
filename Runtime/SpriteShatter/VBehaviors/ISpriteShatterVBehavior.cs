using System;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter.VBehaviors
{
    public interface ISpriteShatterVBehavior
    {
        public Vector2 CalculateVelocity(Triangle[] triangles, Vector2 forcePosition, Vector2 inputForce);
    }
}