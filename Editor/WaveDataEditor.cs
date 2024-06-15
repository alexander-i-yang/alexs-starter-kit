using System.Linq;
using System.Reflection;
using UnityEditor;
using ASK.Animation;
using ASK.Core;
using UnityEngine;

namespace ASK.Editor
{
    [CustomPropertyDrawer(typeof(WaveData))]
    public class WaveDataDrawerUIE : PropertyDrawer
    {
        private bool _foldout = true;
        private bool _preview = true;
        private Vector2 _scale = new Vector2(1, 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _foldout = EditorGUILayout.Foldout(_foldout, label);
            if (!_foldout) return;

            using (new EditorGUI.IndentLevelScope())
            {
                WaveData waveData = GetRealProperty<WaveData>(property);
                DrawProperties(property, waveData);
                
                _preview = EditorGUILayout.Foldout(_preview, "Preview");
                if (_preview) DrawPreview(position, property, waveData);
            }
        }

        private void DrawProperties(SerializedProperty property, WaveData waveData)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(waveData.Frequency)));
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(waveData.Amplitude)));
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(waveData.WaveType)));
            if (waveData.WaveType == WaveType.Custom)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(waveData.Formula)));
            }
        }

        private void DrawPreview(Rect position, SerializedProperty property, WaveData waveData)
        {
            GUIStyle style = EditorStyles.helpBox;
            style.margin = new RectOffset((EditorGUI.indentLevel + 1) * 10 + 3, 0, 4, 0);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Rect relativePosition = new Rect(0, -50, position.width, 100);
                Rect layoutRectangle = GUILayoutUtility.GetRect(10, position.width, relativePosition.height, 100);
                using (new GUI.ClipScope(layoutRectangle))
                {
                    EventType eventType = Event.current.type;
                    if (eventType == EventType.Repaint)
                    {
                        PaintPreview(relativePosition, waveData);
                        RepaintInspector(property.serializedObject);
                    }

                    if (eventType == EventType.ScrollWheel)
                    {
                        HandleScroll(layoutRectangle);
                    }
                }
            }
        }

        private void HandleScroll(Rect layoutRectangle)
        {
            layoutRectangle.position = Vector2.zero;
            var mousePos = Event.current.mousePosition;
            bool inX = layoutRectangle.x < mousePos.x && mousePos.x < layoutRectangle.x + layoutRectangle.width;
            bool inY = layoutRectangle.y < mousePos.y && mousePos.y < layoutRectangle.y + layoutRectangle.height;
            bool contains = inX && inY;
            if (!contains) return; // Not sure why rect.Contains() isn't working here.
            _scale += Vector2.one * (-0.1f * Event.current.delta.y);
            _scale = Vector2.Min(Vector2.Max(_scale, Vector2.one * 0.01f), Vector2.one * 1000f);
            Event.current.Use();
        }

        void PaintPreview(Rect position, WaveData waveData)
        {
            Vector2 offset = new Vector2(0, position.height / 2);
            Handles.color = Color.red;

            float w = waveData.GetWholeCyclesUntil(position.width, _scale.x);
            Handles.DrawAAPolyLine(
                Texture2D.whiteTexture,
                1,
                waveData?.GetPoints(0, w, offset: offset, scale: _scale)
                    .Select(x => new Vector3(x.x, -x.y + 2 * offset.y, x.z))
                    .ToArray());

            Handles.color = new Color(0.7f, 0.3f, 0.3f);
            Handles.DrawAAPolyLine(
                Texture2D.whiteTexture,
                1,
                waveData?
                    .GetPoints((int)w, position.width, offset: offset, scale: _scale)
                    .Select(x => new Vector3(x.x, -x.y + 2 * offset.y, x.z))
                    .ToArray()
            );
            Preview(position, waveData, offset);
        }

        /// <summary>
        /// Preview self.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="waveData"></param>
        /// <param name="offset">Applied after scale.</param>
        private void Preview(Rect position, WaveData waveData, Vector2 offset)
        {
            float w = waveData.GetWholeCyclesUntil(position.width, _scale.x);
            float time = Application.isPlaying ? Game.TimeManager.Time : Time.realtimeSinceStartup;
            time = (time * _scale.x) % w;
            float eval = waveData.Evaluate(time, _scale, new Vector2(offset.x, 0));
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

        private const BindingFlags BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags BINDING_FLAGS_PUB = BindingFlags.Public | BindingFlags.Instance;

        public T GetRealProperty<T>(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath, BINDING_FLAGS);
            if (field == null) field = targetObjectClassType.GetField(property.propertyPath, BINDING_FLAGS_PUB);
            if (field != null)
            {
                var value = field.GetValue(targetObject);
                return (T)value;
            }

            return default;
        }
    }
}