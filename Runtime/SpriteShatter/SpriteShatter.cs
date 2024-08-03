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
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteShatter : MonoBehaviour
    {
        [SerializeField] private GameObject clone;

        private SpriteRenderer _sr;
        public SpriteRenderer MySR
        {
            get
            {
                if (_sr != null) return _sr;
                _sr = GetComponent<SpriteRenderer>();
                return _sr;
            }
        }

        public Vector2[] Mesh = new Vector2[0];

        [SerializeField] [PositiveValueOnly] private float MaxTriangleArea = 2;

        public void Shatter(
            Vector2 forceWorldOrigin,
            Vector2 force,
            IGrouper grouper,
            ISpriteShatterVBehavior spriteShatterVBehavior)
        {
            var sprite = MySR.sprite;
            var forceRay = NormalizeWorldForce(sprite, forceWorldOrigin, force);

            var triangles = Triangulate();
            var flattenedGroups = CreateFlattenedGroups(triangles, grouper, forceRay);

            var pieces = new List<IShatterPiece>();

            foreach (var group in flattenedGroups)
            {
                var p = Game.ParticlePool.ReceiveParticle(
                    () => CreatePiece(sprite, group),
                    (p) => InitPiece(sprite, p, group)
                );
                pieces.Add(p);
            }

            foreach (var piece in pieces)
            {
                piece.ApplyForce(
                    spriteShatterVBehavior.CalculateVelocity(piece.GetTriangles(), forceWorldOrigin, force));
            }
        }

        public static Triangle[][] FlattenGroups(Dictionary<Triangle, int> groupsLookup)
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

        public IShatterPiece CreatePiece(Sprite sprite, Triangle[] triangles)
        {
            var clon = Instantiate(clone).GetComponent<IShatterPiece>();

            //TODO: look into Sprite.PhysicsShape

            InitPiece(sprite, clon, triangles);
            return clon;
        }

        public void InitPiece(Sprite sprite, IShatterPiece p, Triangle[] triangles)
        {
            p.Init(sprite, triangles);
            p.transform.position = transform.position;
        }

        public void BakeMesh()
        {
            Mesh = TexToPoly.GetPolygon(MySR.sprite.texture);
        }

        public Triangle[] Triangulate()
        {
            return Triangulator.Triangulate(Mesh, MaxTriangleArea).ToArray();
        }

        public Triangle[][] CreateFlattenedGroups(Triangle[] triangles, IGrouper grouper, Ray2D forceRay)
        {
            var groups = grouper.CalculateGroupsLookup(triangles, forceRay);
            return FlattenGroups(groups);
        }

        public Ray2D NormalizeWorldForce(Sprite sprite, Vector2 bulletPos, Vector2 realBulletv)
        {
            Vector2 normalizedPos = Triangulator.Normalize(sprite, bulletPos - (Vector2)transform.position);
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