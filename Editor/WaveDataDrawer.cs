using System.Linq;
using UnityEditor;
using ASK.Animation;
using ASK.Core;
using ASK.Editor.Utils;
using UnityEngine;

namespace ASK.Editor
{
    [CustomPropertyDrawer(typeof(WaveData))]
    public class WaveDataDrawer : PropertyDrawer
    {
        private static EditorViewDataStore<WaveViewData> _viewDataStore = new();
        
        private class WaveViewData : EditorViewData {
            public bool Foldout = true;
            public bool Preview = true;
            public Vector2 Scale = new Vector2(1, 1);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            WaveViewData waveViewData = _viewDataStore.GetViewData(property);
            
            position = AutoPosition.IncrLine(position, 0);
            waveViewData.Foldout = EditorGUI.Foldout(position, waveViewData.Foldout, label);
            if (!waveViewData.Foldout) return;
            WaveData waveData = (WaveData)property.boxedValue;

            using (new EditorGUI.IndentLevelScope())
            {
                position = DrawProperties(position, property, waveData);
                position = AutoPosition.IncrLine(position, 1);
                waveViewData.Preview = EditorGUI.Foldout(position, waveViewData.Preview, "Preview");
                if (waveViewData.Preview)
                {
                    position = AutoPosition.IncrLine(position, 1);
                    DrawPreview(position, property, waveData, waveViewData);
                }
            }
            EditorGUI.EndProperty();
        }

        private Rect DrawProperties(Rect position, SerializedProperty property, WaveData waveData)
        {
            position = AutoPosition.IncrLine(position, 1);
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaveData.Frequency)));
            position = AutoPosition.IncrLine(position, 1);
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaveData.Amplitude)));
            position = AutoPosition.IncrLine(position, 1);
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaveData.WaveType)));
            if (waveData.WaveType == WaveType.Custom)
            {
                position = AutoPosition.IncrLine(position, 1);
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(waveData.Formula)));
            }

            return position;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            WaveViewData waveViewData = _viewDataStore.GetViewData(property);
            int totalLines = 1;
            
            if (waveViewData.Foldout)
            {
                totalLines += 4;
                WaveData waveData = (WaveData)property.boxedValue;
                if (waveData.WaveType == WaveType.Custom) totalLines++;
                if (waveViewData.Preview) totalLines += 5;
            }

            return AutoPosition.GetHeight(totalLines);
        }


        private void DrawPreview(Rect position, SerializedProperty property, WaveData waveData, WaveViewData waveViewData)
        {
            int indent = (EditorGUI.indentLevel + 1) * 10;
            int height = (int)(EditorGUIUtility.singleLineHeight * 5);
            position = new Rect(position.x + indent, position.y, position.width - indent, height);
            using (new GUI.GroupScope(position, EditorStyles.helpBox))
            {
                EventType eventType = Event.current.type;
                position = new Rect(0, 0, position.width, height);
                if (eventType == EventType.Repaint)
                {
                    PaintPreview(position, waveData, waveViewData.Scale);
                    RepaintInspector(property.serializedObject);
                }

                if (eventType == EventType.ScrollWheel)
                {
                    HandleScroll(position, waveViewData);
                }
            }
        }

        private void HandleScroll(Rect layoutRectangle, WaveViewData waveViewData)
        {
            layoutRectangle.position = Vector2.zero;
            var mousePos = Event.current.mousePosition;
            bool inX = layoutRectangle.x < mousePos.x && mousePos.x < layoutRectangle.x + layoutRectangle.width;
            bool inY = layoutRectangle.y < mousePos.y && mousePos.y < layoutRectangle.y + layoutRectangle.height;
            bool contains = inX && inY;
            if (!contains) return; // Not sure why rect.Contains() isn't working here.
            waveViewData.Scale += Vector2.one * (-0.1f * Event.current.delta.y);
            waveViewData.Scale = Vector2.Min(Vector2.Max(waveViewData.Scale, Vector2.one * 0.01f), Vector2.one * 1000f);
            Event.current.Use();
        }

        void PaintPreview(Rect position, WaveData waveData, Vector2 scale)
        {
            Vector2 offset = new Vector2(position.x, position.height / 2);
            Handles.color = Color.red;

            float w = waveData.GetWholeCyclesUntil(position.width, scale.x);
            Handles.DrawAAPolyLine(
                Texture2D.whiteTexture,
                1,
                waveData?.GetPoints(0, w, offset: offset, scale: scale)
                    .Select(x => new Vector3(x.x, -x.y + 2 * offset.y, x.z))
                    .ToArray());

            Handles.color = new Color(0.7f, 0.3f, 0.3f);
            Handles.DrawAAPolyLine(
                Texture2D.whiteTexture,
                1,
                waveData?
                    .GetPoints((int)w, position.width, offset: offset, scale: scale)
                    .Select(x => new Vector3(x.x, -x.y + 2 * offset.y, x.z))
                    .ToArray()
            );
            Preview(position, waveData, offset, scale);
        }

        /// <summary>
        /// Preview self.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="waveData"></param>
        /// <param name="offset">Applied after scale.</param>
        private void Preview(Rect position, WaveData waveData, Vector2 offset, Vector2 scale)
        {
            float w = waveData.GetWholeCyclesUntil(position.width, scale.x);
            float time = Application.isPlaying ? Game.TimeManager.Time : Time.realtimeSinceStartup;
            time = (time * scale.x) % w;
            float eval = waveData.Evaluate(time, scale, new Vector2(offset.x, 0));
            eval = -eval; //flip due to inverted coordinate system
            eval += offset.y;
            DrawTimeBar(time, position.height);
            FloatObject(eval, position.width);
            DrawCircle(time, eval, 3);
        }

        private void DrawCircle(float x, float y, float r)
        {
            Handles.DrawSolidDisc(new Vector2(x, y), Vector3.back, r);
        }

        private void FloatObject(float y, float width)
        {
            Handles.DrawLine(new Vector3(0, y), new Vector3(width, y));
        }

        public void DrawTimeBar(float time, float height)
        {
            Handles.DrawLine(new Vector2(time, 0), new Vector2(time, height));
        }

        public static void RepaintInspector(SerializedObject BaseObject)
        {
            foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                if (item.serializedObject == BaseObject)
                {
                    item.Repaint();
                    return;
                }
        }
    }
}