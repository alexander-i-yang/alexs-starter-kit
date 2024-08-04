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
        private int selectedGroup;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var t = target as SpriteShatterViz;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                selectedGroup = EditorGUILayout.IntSlider("Selected Group", selectedGroup, -1, t.NumGroups);
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
            if (selectedGroup == -1)
            {
                
            }
            else
            {
                /*if (flattened.Length <= selectedGroup) return;
                var triangles = flattened[selectedGroup];
                var center = triangles.Select(t => t.Center()).Average();
                Vector2 appliedForce = spriteShatterVBehavior.CalculateVelocity(triangles, ForcePos, Force);
                Helper.DrawArrow(
                    (Vector3)center + tPos,
                    appliedForce,
                    Color.yellow,
                    0);*/
            }
        }
    }
}