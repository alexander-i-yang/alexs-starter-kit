using System.Collections.Generic;
using System.Linq;
using ASK.Editor.Standalone;
using ASK.Helpers;
using ASK.Runtime.Helpers;
using ASK.Runtime.SpriteShatter.Groupers;
using ASK.Runtime.SpriteShatter.VBehaviors;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatterViz : SpriteShatter
    {
        [SerializeField] public Vector2 Force;
        [SerializeField] public Vector2 ForcePos;

        [SerializeField] public Triangle[] Triangles;
        [SerializeField] public SpriteShatterGroup[] Groups;

        [SerializeReference]
        #if UNITY_EDITOR
        [ChilrdenClassesDropdown(typeof(ISpriteShatterVBehavior))]
        #endif
        private ISpriteShatterVBehavior spriteShatterVBehavior;

        [FormerlySerializedAs("d_grouper")]
        [SerializeReference]
        #if UNITY_EDITOR
        [ChilrdenClassesDropdown(typeof(IGrouper))]
        #endif
        private IGrouper grouper;

        public int NumGroups => Groups == null ? 0 : Groups.Length;

        public void Shatter()
        {
            Shatter(ForcePos, Force, grouper, spriteShatterVBehavior);
        }
        
        public void BakeAll()
        {
            Triangles = Triangulate();
            Groups = CreateGroups(Triangles, grouper, spriteShatterVBehavior, ForcePos, Force);
        }
    }
}