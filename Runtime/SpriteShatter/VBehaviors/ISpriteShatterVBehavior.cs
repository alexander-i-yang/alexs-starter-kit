using System;
using System.Collections.Generic;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter.VBehaviors
{
    public interface ISpriteShatterVBehavior
    {
        public Vector2 CalculateVelocity(IList<Triangle> triangles, Vector2 forcePosition, Vector2 inputForce);
    }
}