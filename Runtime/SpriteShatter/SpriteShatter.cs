using System.Collections.Generic;
using System.Linq;
using ASK.Core;
#if UNITY_EDITOR
using ASK.Editor.Standalone;
#endif
using ASK.Helpers;
using ASK.Runtime.Helpers;
using ASK.Runtime.SpriteShatter.Groupers;
using UnityEngine;
using MyBox;
using TriangleNet.Topology;

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

        [SerializeField] private Vector2 d_Force;
        [SerializeField] private Vector2 d_ForcePos;

        [SerializeField]
        [Tooltip("Debug only.")]
        private Triangle[] d_triangles;

        [SerializeReference]
        #if UNITY_EDITOR
        [ChilrdenClassesDropdown(typeof(ISpriteShatterVBehavior))]
        #endif
        private ISpriteShatterVBehavior spriteShatterVBehavior;
        
        [SerializeReference]
        #if UNITY_EDITOR
        [ChilrdenClassesDropdown(typeof(IGrouper))]
        #endif
        private IGrouper d_grouper;

        [SerializeField]
        [Range(-1,60)]
        private int d_selectedGroup;

        private Dictionary<Triangle, int> d_groups;
        

        public void Shatter()
        {
            Shatter(d_ForcePos, d_Force, d_grouper);
        }
        
        public void Shatter(Vector2 forceWorldOrigin, Vector2 force, IGrouper grouper)
        {
            var forceRay = NormalizeWorldForce(forceWorldOrigin, force);
            
            Triangulate(grouper, forceRay);
            var flattenedGroups = FlattenGroups(d_groups);

            

            foreach (var group in flattenedGroups)
            {
                Game.ParticlePool.ReceiveParticle<IShatterPiece>(
                    () => CreatePiece(group),
                    (p) => InitPiece(p, group)
                );
            }
            var pieces = flattenedGroups.Select(group => CreatePiece(group)).ToArray();
            
            foreach (var piece in pieces)
            {
                piece.ApplyForce(ResolveForces(forceWorldOrigin, force, piece.GetTriangles()));
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
            var clon = Instantiate(clone, transform.position, Quaternion.identity).GetComponent<IShatterPiece>();

            //TODO: look into Sprite.PhysicsShape

            InitPiece(clon, triangles);
            return clon;
        }

        public void InitPiece(IShatterPiece p, Triangle[] triangles)
        {
            p.Init(Sprite.sprite, triangles);
        }
        
        /// <summary>
        /// Triangles are normalized according to sprite coordinates.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="forcePos"></param>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public Vector2 ResolveForces(Vector2 forcePos, Vector2 force, Triangle[] triangles)
        {
            Vector2 trianglePos = triangles.Select(t => t.Center()).Average();
            var forceRay = new Ray2D(forcePos, force);
            var l1 = forceRay.GetPoint(100);
            var l2 = forceRay.GetPoint(-100);
            float distanceFromLine = Triangulator.DistFromLine(trianglePos, l1, l2);
            return force * (Mathf.Lerp(10, 0.25f, distanceFromLine)) + (trianglePos - forcePos)*force.magnitude*0.05f;
        }

        public void Triangulate(IGrouper grouper, Ray2D forceRay)
        {
            d_triangles = Triangulator.Triangulate(Mesh, MaxTriangleArea).ToArray();
            d_groups = grouper.CalculateGroupsLookup(d_triangles, forceRay);
        }

        public Vector2 e;
        
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

            var forceRay = new Ray2D(d_ForcePos - (Vector2)transform.position, d_Force);
            e = forceRay.origin;
            UnityEditor.Handles.DrawLine(d_ForcePos, d_ForcePos+d_Force);
            
            var l1 = forceRay.GetPoint(100);
            var l2 = forceRay.GetPoint(-100);
            
            float[] colors;
            //colors = d_triangles.Select(triangle => HandleUtility.DistancePointLine(triangle.Center(), l1, l2)/2).ToArray();
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
        public Ray2D NormalizeWorldForce(Vector2 bulletPos, Vector2 realBulletv)
        {
            Vector2 normalizedPos = Triangulator.Normalize(Sprite.sprite, bulletPos-(Vector2)transform.position);
            return new Ray2D(normalizedPos, realBulletv);
        }

        public void Triangulate()
        {
            Triangulate(d_grouper, new Ray2D(d_ForcePos - (Vector2)transform.position, d_Force));
        }
    }
}