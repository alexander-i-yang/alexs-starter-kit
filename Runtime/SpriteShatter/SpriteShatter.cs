using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Editor.Standalone;
using ASK.Helpers;
using ASK.Runtime.Helpers;
using UnityEngine;
using MyBox;
using TriangleNet.Topology;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatter : MonoBehaviour
    {
        [SerializeField] private GameObject clone;

        private SpriteRenderer _sprite;
        
        public SpriteRenderer Sprite
        {
            get
            {
                if (_sprite != null) return _sprite;
                _sprite = GetComponent<SpriteRenderer>();
                return _sprite;
            }
        }

        public Vector2[] Mesh;

        [SerializeField]
        [PositiveValueOnly]
        private float MaxTriangleArea = 2;

        [SerializeField] private float d_ForceMagnitude;
        [SerializeField] private Vector2 testForcePos;

        [SerializeField]
        [Tooltip("Debug only.")]
        private Triangle[] d_triangles;

        [SerializeReference]
        [ChilrdenClassesDropdown(typeof(ISpriteShatterVBehavior))]
        private ISpriteShatterVBehavior spriteShatterVBehavior;

        [SerializeField]
        [Range(-1,60)]
        private int d_selectedGroup;

        private Dictionary<Triangle, int> d_groups;

        public void Shatter()
        {
            Sprite.enabled = false;
            Triangulate();

            var flattenedGroups = FlattenGroups(d_groups);

            var pieces = flattenedGroups.Select(group => CreatePiece(group)).ToArray();
            foreach (var piece in pieces)
            {
                piece.ApplyForce(
                    ResolveForces(Vector2.zero, (Vector2)transform.position + testForcePos, piece.GetTriangles()));
            }
        }

        [SerializeField]
        [PositiveValueOnly]
        private int maxGroupSize;
        /*public Dictionary<Triangle, int> CalculateGroupsLookup(Triangle[] triangles)
        {
            Dictionary<Triangle, int> groupsLookup = new();

            int groupNum = 0;
            foreach (var triangle in triangles)
            {
                int groupSize = 0;
                Queue<Triangle> q = new Queue<Triangle>();
                q.Enqueue(triangle);
                
                while (q.Count > 0 && groupSize <= maxGroupSize)
                {
                    var groupTriangle = q.Dequeue();
                    if (groupTriangle == null || groupTriangle.ID < 0 || groupsLookup.ContainsKey(groupTriangle)) continue;
                    
                    groupTriangle.neighbors.ForEach(o => q.Enqueue(o.Triangle));
                    
                    groupsLookup.Add(groupTriangle, groupNum);
                    groupSize++;
                }

                groupNum++;
            }

            return groupsLookup;
        }*/
        
        public Dictionary<Triangle, int> CalculateGroupsLookup(Triangle[] triangles)
        {
            Dictionary<Triangle, int> groupsLookup = new();

            int groupNum = 2;
            
            foreach (var triangle in triangles)
            {
                if (groupsLookup.ContainsKey(triangle)) continue;
                float tcy = triangle.Center().y;
                float tprob = Math.Abs(tcy)/2f;
                if (tprob > 0.5f)
                {
                    groupsLookup.Add(triangle, tcy > 0 ? 0 : 1);
                    continue;
                }
                
                
                groupsLookup.Add(triangle, groupNum);
                
                Queue<Triangle> q = new Queue<Triangle>();
                triangle.neighbors.ForEach(o => q.Enqueue(o.Triangle));
                
                while (q.Count > 0)
                {
                    var groupTriangle = q.Dequeue();
                    if (groupTriangle == null || groupTriangle.ID < 0 || groupsLookup.ContainsKey(groupTriangle)) continue;
                    
                    float cy = groupTriangle.Center().y;
                    float prob = Math.Abs(cy)/6f;
                    if (Random.value > prob) continue;
                    
                    groupTriangle.neighbors.ForEach(o => q.Enqueue(o.Triangle));
                    
                    groupsLookup.Add(groupTriangle, groupNum);
                }

                groupNum++;
            }

            return groupsLookup;
        }

        public Triangle[][] FlattenGroups(Dictionary<Triangle, int> groupsLookup)
        {
            Dictionary<int, List<Triangle>> ret = new();
            foreach (var kv in groupsLookup)
            {
                var tri = kv.Key;
                var group = kv.Value;

                if (!ret.ContainsKey(group)) ret[group] = new();
                ret[group].Add(tri);
            }

            return ret.Values.Select(l => l.ToArray()).ToArray();
        }

        public IShatterPiece CreatePiece(Triangle[] triangles)
        {
            var clon = Instantiate(clone, transform.position, Quaternion.identity);

            //TODO: look into Sprite.PhysicsShape

            var piece = clon.GetComponent<IShatterPiece>();
            piece.Init(Sprite.sprite, triangles);

            return piece;
        }
        
        /// <summary>
        /// Triangles are normalized according to sprite coordinates.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="forcePos"></param>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public Vector2 ResolveForces(Vector2 force, Vector2 forcePos, Triangle[] triangles)
        {
            Vector2 normalizedPos = Triangulator.Normalize(Sprite.sprite, (Vector2)transform.position - forcePos);
            Vector2 trianglePos = triangles.Select(t => t.Center()).Average();
            return d_ForceMagnitude*(normalizedPos - trianglePos);
        }

        public void Triangulate()
        {
            d_triangles = Triangulator.TriangulateAndNormalize(Mesh, MaxTriangleArea).ToArray();
            d_groups = CalculateGroupsLookup(d_triangles);
        }
        
        #if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            if (d_triangles == null || d_groups == null) return;
            
            Vector2 tPos = transform.position;
            // float[] colors = d_triangles
            //     .Select(t => ResolveForces(Vector2.zero, tPos + testForcePos, t).magnitude)
            //     .ToArray();
            //colors = colors.Select(c => c/colors.Max()).ToArray();
            
            int i = 0;

            float[] colors;
             if (d_selectedGroup == -1)
             {
                 float maxGroup = d_groups.Values.Max();
                 colors = d_triangles.Select(t => d_groups[t] / maxGroup).ToArray();
             }
             else
             {
                 colors = d_triangles.Select(t => d_groups[t] == d_selectedGroup ? 1f : 0f).ToArray();
             }
            Triangulator.DrawTriangles(d_triangles, tPos, colors);
        }
        #endif
    }
}