using System;
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
using ASK.Runtime.SpriteShatter.VBehaviors;
using UnityEngine;
using MyBox;
using TriangleNet.Topology;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatter : MonoBehaviour
    {
        [SerializeField] private GameObject clone;

        private SpriteRenderer _sprite;

        public Triangle[] d_triangles { get; private set; }
        public Dictionary<Triangle, int> d_groups { get; private set; }
        
        public SpriteRenderer MySpriteRenderer
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

        public void Shatter(
            Vector2 forceWorldOrigin,
            Vector2 force,
            IGrouper grouper,
            ISpriteShatterVBehavior spriteShatterVBehavior)
        {
            var forceRay = NormalizeWorldForce(forceWorldOrigin, force);
            
            Triangulate(grouper, forceRay);
            var flattenedGroups = FlattenGroups(d_groups);

            var pieces = new List<IShatterPiece>();

            foreach (var group in flattenedGroups)
            {
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
            p.Init(MySpriteRenderer.sprite, triangles);
            p.transform.position = transform.position;
        }

        public void BakeMesh()
        {
            Mesh = TexToPoly.GetPolygon(MySpriteRenderer.sprite.texture);
        }

        public void Triangulate(IGrouper grouper, Ray2D forceRay)
        {
            d_triangles = Triangulator.Triangulate(Mesh, MaxTriangleArea).ToArray();
            d_groups = grouper.CalculateGroupsLookup(d_triangles, forceRay);
        }

        
        public Ray2D NormalizeWorldForce(Vector2 bulletPos, Vector2 realBulletv)
        {
            Vector2 normalizedPos = Triangulator.Normalize(MySpriteRenderer.sprite, bulletPos-(Vector2)transform.position);
            return new Ray2D(normalizedPos, realBulletv);
        }

        public Vector3[] GetDrawableMesh()
        {
            if (Mesh.Length > 0)
            {
                return Mesh.Append(Mesh[0]).Select(v => (Vector3)v + transform.position).ToArray();
            }

            return Array.Empty<Vector3>();
        }
    }
}