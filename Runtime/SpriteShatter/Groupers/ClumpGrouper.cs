using System.Collections.Generic;
using MyBox;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter.Groupers
{
    public class ClumpGrouper : IGrouper
    {
        public int MaxGroupSize;
        
        public Dictionary<Triangle, int> CalculateGroupsLookup(Triangle[] triangles, Ray2D forceRay)
        {
            Dictionary<Triangle, int> groupsLookup = new();
            int groupNum = 0;
            
            foreach (var triangle in triangles)
            {
                Queue<Triangle> q = new Queue<Triangle>();
                q.Enqueue(triangle);
                int curGroupSize = 0;
                while (q.Count > 0)
                {
                    var groupTriangle = q.Dequeue();
                    if (groupTriangle == null || groupTriangle.ID < 0 || groupsLookup.ContainsKey(groupTriangle)) continue;

                    groupTriangle.neighbors.ForEach(o => q.Enqueue(o.Triangle));

                    groupsLookup.Add(groupTriangle, groupNum);
                    curGroupSize++;

                    if (curGroupSize >= MaxGroupSize) break;
                }

                if (curGroupSize > 0) groupNum++;
            }

            return groupsLookup;
        }
    }
}