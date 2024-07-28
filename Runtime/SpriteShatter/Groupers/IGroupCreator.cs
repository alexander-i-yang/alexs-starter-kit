using System;
using System.Collections.Generic;
using ASK.Runtime.Helpers;
using MyBox;
using TriangleNet.Topology;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ASK.Runtime.SpriteShatter.Groupers
{
    public interface IGrouper
    {
        public Dictionary<Triangle, int> CalculateGroupsLookup(Triangle[] triangles, Ray2D forceRay);
    }
}