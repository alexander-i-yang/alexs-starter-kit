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
using UnityEngine.Serialization;

namespace ASK.Runtime.SpriteShatter
{
    [Serializable]
    public struct SpriteShatterGroup
    {
        [FormerlySerializedAs("triangles")] public List<Triangle> Triangles;
        public Vector2 velocity;
        public Vector2 Center => Triangles.Select(t => t.Center()).Average();
    }
    
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

            var triangles = Triangulate();
            var flattenedGroups = CreateGroups(triangles, grouper, spriteShatterVBehavior, forceWorldOrigin, force);
            
            var pieces = new List<IShatterPiece>();

            foreach (var group in flattenedGroups)
            {
                var p = Game.ParticlePool.ReceiveParticle(
                    () => CreatePiece(sprite, group),
                    (p) => InitPiece(sprite, p, group)
                );
                pieces.Add(p);
            }
        }

        public SpriteShatterGroup[] CreateGroups(
            Triangle[] triangles,
            IGrouper grouper,
            ISpriteShatterVBehavior spriteShatterVBehavior,
            Vector2 forceWorldOrigin,
            Vector2 force
            )
        {
            var sprite = MySR.sprite;

            var forceRay = NormalizeWorldForce(sprite, forceWorldOrigin, force);
            var groupsLookup = grouper.CalculateGroupsLookup(triangles, forceRay);
            var flattenedGroups = FlattenGroups(groupsLookup);
            for (var i = 0; i < flattenedGroups.Length; i++)
            {
                var groupTriangles = flattenedGroups[i].Triangles;
                Vector2 v = spriteShatterVBehavior.CalculateVelocity(groupTriangles, forceWorldOrigin, force);
                flattenedGroups[i].velocity = v;
            }

            return flattenedGroups;
        }

        public static SpriteShatterGroup[] FlattenGroups(Dictionary<Triangle, int> groupsLookup)
        {
            SpriteShatterGroup[] ret = new SpriteShatterGroup[groupsLookup.Values.Max()+1];
            foreach (var kv in groupsLookup)
            {
                var tri = kv.Key;
                var groupInd = kv.Value;
                var triangles = ret[groupInd].Triangles;

                if (triangles == null) triangles = new ();
                triangles.Add(tri);
                ret[groupInd].Triangles = triangles;
            }

            return ret.Where(t => t.Triangles != null).ToArray();
        }

        public IShatterPiece CreatePiece(Sprite sprite, SpriteShatterGroup group)
        {
            var clon = Instantiate(clone).GetComponent<IShatterPiece>();

            InitPiece(sprite, clon, group);
            return clon;
        }

        public void InitPiece(Sprite sprite, IShatterPiece p, SpriteShatterGroup group)
        {
            p.Init(sprite, group);
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

        public Ray2D NormalizeWorldForce(Sprite sprite, Vector2 forcePos, Vector2 velocity)
        {
            forcePos -= (Vector2)transform.position;
            //forcePos = Triangulator.Normalize(sprite, forcePos);
            return new Ray2D(forcePos, velocity);
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