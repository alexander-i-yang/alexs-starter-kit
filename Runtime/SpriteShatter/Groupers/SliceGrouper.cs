using System;
using System.Collections.Generic;
using ASK.Runtime.Helpers;
using MyBox;
using TriangleNet.Topology;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter.Groupers
{
    public class SliceGrouper : IGrouper
    {
        public float CleaveRange = 3f;
        
        public Dictionary<Triangle, int> CalculateGroupsLookup(Triangle[] triangles, Ray2D forceRay)
        {
            Dictionary<Triangle, int> groupsLookup = new();
            int groupNum = 2;
            
            foreach (var triangle in triangles)
            {
                if (groupsLookup.ContainsKey(triangle)) continue;

                var l1 = forceRay.GetPoint(100);
                var l2 = forceRay.GetPoint(-100);
                float distanceFromLine = Triangulator.DistFromLine(triangle.Center(), l1, l2);
                
                //float tcy = triangle.Center().y;
                if (distanceFromLine > CleaveRange)
                {
                    groupsLookup.Add(triangle, IsLeft(l1, l2, triangle.Center()) ? 0 : 1);
                    continue;
                }
                
                groupsLookup.Add(triangle, groupNum);
                groupNum++;
            }

            return groupsLookup;
        }
        
        public bool IsLeft(Vector2 lineA, Vector2 lineB, Vector2 p) {
            return (lineB.x - lineA.x)*(p.y - lineA.y) - (lineB.y - lineA.y)*(p.x - lineA.x) > 0;
        }
    }
}