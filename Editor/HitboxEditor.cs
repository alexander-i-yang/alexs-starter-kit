using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Helpers;
using ASK.Runtime.Phys2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            HandleUtility.PickGameObjectCallback e = OnPickGameObjectCustomPasses;
            HandleUtility.pickGameObjectCustomPasses += e;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            _center = _hitbox.Center;
            _center = EditorGUILayout.Vector2Field("Center", _center);
            _snapBy = EditorGUILayout.Vector2Field("Snap by", _snapBy);

            DrawZeroCenterButton();
            
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
            }
        }
        
        GameObject OnPickGameObjectCustomPasses ( Camera cam , int layers , Vector2 position , GameObject[] ignore , GameObject[] filter , out int materialIndex )
        {
            materialIndex = -1;
            if (_hitbox == null) return null;
            
            GameObject gameObject = _hitbox.gameObject;
            if (layers != -1 && (gameObject.layer & layers) == 0) return null;
            if (ignore != null && ignore.Contains(gameObject)) return null;
            if (filter != null && !filter.Contains(gameObject)) return null;
            
            var ray = cam.ScreenToWorldPoint( position );
            //Bounds b = new Bounds((Vector3)_hitbox.Center + _hitbox.transform.position, new Vector3(_hitbox.Size.x, _hitbox.Size.y, 1000000));
            bool hit = _hitbox.GlobalContains(ray);
            return hit ? gameObject : null;
        }
    }
}