using System.Collections.Generic;
using System.Linq;
using ASK.Helpers;
using ASK.Runtime.Helpers;
using TriangleNet.Topology;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter
{
    [CustomEditor(typeof(SpriteShatterViz))]
    public class SpriteShatterVizEditor : UnityEditor.Editor
    {
        private int selectedGroup = -1;
        private bool normalizeVelocityViz;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var t = target as SpriteShatterViz;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                selectedGroup = EditorGUILayout.IntSlider("Selected Group", selectedGroup, -1, t.NumGroups);
                normalizeVelocityViz = EditorGUILayout.Toggle("Cap Velocity Viz", normalizeVelocityViz);
                if (check.changed) SceneView.RepaintAll();
            }

            if (GUILayout.Button("Bake Mesh"))
            {
                t.BakeMesh();
            }

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Shatter"))
                {
                    t.Shatter();
                }
            }

            t.BakeAll();
        }

        public void OnSceneGUI()
        {
            var t = target as SpriteShatterViz;
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(t.GetDrawableMesh());

            Triangle[] triangles = t.Triangles;
            var groups = t.Groups;

            Vector2 tPos = t.transform.position;
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(t.GetDrawableMesh());

            DrawTriangles(triangles, tPos);
            DrawGroups(tPos, groups);
            DrawVelocities(tPos, groups);
            
            Handles.color = Color.cyan;
            Handles.DrawLine(t.ForcePos, t.ForcePos + t.Force, 5);
        }

        private void DrawTriangles(Triangle[] triangles, Vector3 transformPos)
        {
            if (triangles == null) return;
            var colors = triangles.Select(_ => 0f).ToArray();
            Triangulator.DrawTriangles(triangles, transformPos, colors);
        }

        private void DrawGroups(Vector3 tPos, SpriteShatterGroup[] groups)
        {
            if (groups == null || groups.Length == 0) return;

            float[] colors;
            if (selectedGroup == -1)
            {
                float maxGroup = groups.Length;
                colors = groups.Select((_, i) => i / maxGroup).ToArray();
            }
            else
            {
                colors = groups.Select((_, i) => i == selectedGroup ? 1f : 0f).ToArray();
            }

            Triangulator.DrawGroups(groups, tPos, colors);
        }

        public void DrawVelocities(Vector3 tPos, SpriteShatterGroup[] groups)
        {
            void DrawGroup(SpriteShatterGroup group)
            {
                Vector3 p0 = (Vector3)group.Center + tPos;
                Vector3 velocity = normalizeVelocityViz ? group.velocity.normalized : (Vector3)group.velocity;
                Vector3 p1 = (Vector3)group.Center + tPos + velocity;

                Handles.color = Color.yellow;
                Handles.DrawLine(p0, p1, 2);
            }
            
            if (selectedGroup < 0)
            {
                foreach (var group in groups)
                    DrawGroup(group);
            }
            else
            {
                var group = groups[selectedGroup];
                DrawGroup(group);
            }
        }
    }
}