using System.Collections.Generic;
using System.Linq;
using ASK.Core;
#if UNITY_EDITOR
using ASK.Editor.Standalone;
using UnityEditor;
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

        public SpriteRenderer d_sprite;
        public void Shatter(Vector2 forceWorldOrigin, Vector2 force, IGrouper grouper)
        {
            var forceRay = NormalizeWorldForce(forceWorldOrigin, force);
            
            Triangulate(grouper, forceRay);
            var flattenedGroups = FlattenGroups(d_groups);

            var pieces = new List<IShatterPiece>();

            foreach (var group in flattenedGroups)
            {
                if (group.Length < 2) continue;
                
                var p = Game.ParticlePool.ReceiveParticle(
                    () => CreatePiece(group),
                    (p) => InitPiece(p, group)
                );
                pieces.Add(p);
            }
            
            foreach (var piece in pieces)
            {
                piece.ApplyForce(spriteShatterVBehavior.CalculateVelocity(piece.GetTriangles(), forceWorldOrigin, force));
            }
        }

        [SerializeField]
        [PositiveValueOnly]
        private int maxGroupSize;
        
        

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
            var clon = Instantiate(clone).GetComponent<IShatterPiece>();

            //TODO: look into Sprite.PhysicsShape

            InitPiece(clon, triangles);
            return clon;
        }

        public void InitPiece(IShatterPiece p, Triangle[] triangles)
        {
            p.Init(Sprite.sprite, triangles);
            p.transform.position = transform.position;
        }

        public uint d_gl = 3;
        public float d_pr = 0.99f;
        public Vector2[] d_pct;
        public void Triangulate(IGrouper grouper, Ray2D forceRay)
        {
            d_triangles = Triangulator.Triangulate(Mesh, MaxTriangleArea).ToArray();
            d_groups = grouper.CalculateGroupsLookup(d_triangles, forceRay);
            
            var ct = new ContourTracer();
            ct.Trace(d_sprite.sprite.texture, Vector2.zero, 1, d_gl, d_pr, 0.1f);
            d_pct = ct.GetPath(0);
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
            Handles.DrawPolyLine(d_pct.ToVector3().ToArray());
            var flattened = FlattenGroups(d_groups);
            
            if (d_selectedGroup == -1)
            {
                foreach (var triangles in flattened)
                {
                    var center = triangles.Select(t => t.Center()).Average();
                    Vector2 appliedForce = spriteShatterVBehavior.CalculateVelocity(triangles, d_ForcePos, d_Force);
                    Helper.DrawArrow(
                        (Vector3)center + transform.position,
                        appliedForce,
                        Color.yellow,
                        0);
                }
            }
            else
            {
                if (flattened.Length <= d_selectedGroup) return;
                var triangles = flattened[d_selectedGroup];
                var center = triangles.Select(t => t.Center()).Average();
                Vector2 appliedForce = spriteShatterVBehavior.CalculateVelocity(triangles, d_ForcePos, d_Force);
                Helper.DrawArrow(
                    (Vector3)center + transform.position,
                    appliedForce,
                    Color.yellow,
                    0);
            }
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