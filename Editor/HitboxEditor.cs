using System;
using System.Collections.Generic;
using ASK.Helpers;
using ASK.Runtime.Phys2D;
using UnityEditor;
using UnityEngine;

namespace ASK.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Hitbox))]
    public class HitboxEditor : UnityEditor.Editor
    {
        private Hitbox _hitbox;
        private Vector2 _center;
        private Vector2 _snapBy = Vector2.one;

        private float _cornerHandleMargin = 0.2f;
        
        private void OnEnable()
        {
            _hitbox = (Hitbox)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            _center = _hitbox.Center;
            _center = EditorGUILayout.Vector2Field("Center", _center);
            _snapBy = EditorGUILayout.Vector2Field("Snap by", _snapBy);
            
            DrawZeroCenterButton();

            DrawSyncButton();
            
            EditorGUI.BeginChangeCheck();
            ((Hitbox)target).Center = _center;
            if (EditorGUI.EndChangeCheck()) Repaint();
        }

        private void DrawZeroCenterButton()
        {
            if (GUILayout.Button("Zero center"))
            {
                //TODO: snap size to integer values
                _center = Vector2.zero;
                Repaint();
            }
        }

        private void DrawSyncButton()
        {
            if (GUILayout.Button("Sync"))
            {
                var col = _hitbox.GetComponent<BoxCollider2D>();
                if (col) col.size = _hitbox.Bounds.size;
                
                var sr = _hitbox.GetComponentInChildren<SpriteRenderer>();
                if (sr) sr.transform.localScale = _hitbox.Bounds.size / new Vector2(8, 8);
                
                EditorUtility.SetDirty(_hitbox);
            }
        }

        protected void OnSceneGUI()
        {
            float scale = HandleUtility.GetHandleSize(_hitbox.transform.position) * _cornerHandleMargin;
            Vector2 margin = Vector2.one * scale;
            //Not using a dictionary to account for duplicate keys
            var positions = new List<Tuple<Vector3, Action<Vector2>>>
            {
                // Corner handles
                new(_hitbox.BottomLeftGlobal, newPos => _hitbox.BottomLeftGlobal = newPos),
                new(_hitbox.BottomRightGlobal, newPos => _hitbox.BottomRightGlobal = newPos),
                new(_hitbox.TopLeftGlobal, newPos => _hitbox.TopLeftGlobal = newPos),
                new( _hitbox.TopRightGlobal, newPos => _hitbox.TopRightGlobal = newPos),
                
                // Centered corner handles
                new( _hitbox.TopRightGlobal + margin, newPos => _hitbox.SetCornerGlobal(newPos)),
                new( _hitbox.TopLeftGlobal + new Vector2(-margin.x, margin.y), newPos => _hitbox.SetCornerGlobal(newPos)),
                new( _hitbox.BottomRightGlobal + new Vector2(margin.x, -margin.y), newPos => _hitbox.SetCornerGlobal(newPos)),
                new( _hitbox.BottomLeftGlobal + new Vector2(-margin.x, -margin.y), newPos => _hitbox.SetCornerGlobal(newPos)),
                new( _hitbox.GlobalCenter, newPos => _hitbox.GlobalCenter = newPos),
            };

            foreach (var p in positions) DrawCornerHandle(p.Item1, p.Item2);

            //BoxDrawer.DrawHitbox(_hitbox, Color.white);
        }

        private void DrawCornerHandle(Vector3 position, Action<Vector2> updateFunction)
        {
            EditorGUI.BeginChangeCheck();
            float size = HandleUtility.GetHandleSize(position) * 0.05f;
            
            Handles.color = Hitbox.GizmoColor;

            var newPos = Handles.FreeMoveHandle(position, size, _snapBy, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                newPos = newPos.Snap(_snapBy);
                Undo.RecordObject(this, "Change hitbox bounds");
                updateFunction(newPos);
                EditorUtility.SetDirty(_hitbox);
            }
        }
    }
}