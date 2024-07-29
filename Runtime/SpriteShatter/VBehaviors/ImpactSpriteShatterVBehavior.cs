using System.Linq;
using MyBox;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter.VBehaviors
{
    public class ImpactSpriteShatterVBehavior : ISpriteShatterVBehavior
    {
        [PositiveValueOnly]
        public float MassMagnitude;
        
        public Vector2 CalculateVelocity(Triangle[] triangles, Vector2 forcePosition, Vector2 inputForce)
        {
            float area = triangles.Sum(t => t.CalcArea());
            return inputForce.normalized * (MassMagnitude * area);
        }
    }
}