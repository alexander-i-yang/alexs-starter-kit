using System;
using System.Linq;
using ASK.Helpers;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.Phys2D
{
    public sealed class Hitbox : MonoBehaviour
    {
        public static Color GizmoColor = new(1, 0.5f, 0.4f);
        
        [field: SerializeField] public Rect Bounds { get; set; }

        public Vector2 Center
        {
            get => Bounds.center;
            set
            {
                var bounds = Bounds;
                bounds.center = value;
                Bounds = bounds;
            }
        }

        public Vector2 GlobalCenter
        {
            get => Bounds.center + (Vector2)transform.position;
            set { Center = value - (Vector2)transform.position; }
        }

        /// <summary>
        /// Returns true if the two hitboxes will touch/overlap if this hitbox moves by position.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool WillCollide(Hitbox other, Vector2 direction)
        {
            if (other == this) return false;
            var otherRect = other.Bounds;
            otherRect.position += (Vector2)other.transform.position;

            var bounds = Bounds;
            bounds.position += (Vector2)transform.position + direction;

            var ret = bounds.Overlaps(otherRect);

            return ret;
        }

        private Vector3 _e;

        public Vector2 BottomLeftGlobal
        {
            get => (Vector2)transform.position + Bounds.position;
            set => Bounds = Rect.MinMaxRect(
                value.x - transform.position.x,
                value.y - transform.position.y,
                Bounds.xMax,
                Bounds.yMax
            );
        }

        public Vector2 BottomRightGlobal
        {
            get => BottomLeftGlobal + Vector2.right * Bounds.width;
            set => Bounds = Rect.MinMaxRect(
                Bounds.xMin,
                value.y - transform.position.y,
                value.x - transform.position.x,
                Bounds.yMax
            );
        }

        public Vector2 TopLeftGlobal
        {
            get => BottomLeftGlobal + Vector2.up * Bounds.height;
            set => Bounds = Rect.MinMaxRect(
                value.x - transform.position.x,
                Bounds.yMin,
                Bounds.xMax,
                value.y - transform.position.y
            );
        }

        public Vector2 TopRightGlobal
        {
            get => BottomLeftGlobal + Bounds.size;
            set => Bounds = Rect.MinMaxRect(
                Bounds.xMin,
                Bounds.yMin,
                value.x - transform.position.x,
                value.y - transform.position.y
            );
        }

        public Vector2 Size
        {
            get => Bounds.size;
        }

        /// <summary>
        /// Sets size while keeping the center the same.
        /// </summary>
        /// <param name="newSize"></param>
        public void SetSizeCentered(Vector2 newSize)
        {
            var oldCenter = Center;
            var bounds = Bounds;
            bounds.size = newSize;
            Bounds = bounds;
            Center = oldCenter;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //BoxDrawer.DrawHitboxHandles(this, new Color(1,0.5f,0.4f));
            BoxDrawer.FillHitbox(this, GizmoColor.WithAlphaSetTo(0.05f), GizmoColor);

            AddPickHandlers();
        }

        private bool _didAddPickHandlers = false;
        
        private void AddPickHandlers()
        {
            HandleUtility.PickGameObjectCallback e = OnPickGameObjectCustomPasses;
            HandleUtility.pickGameObjectCustomPasses += e;
        }
        
        GameObject OnPickGameObjectCustomPasses ( Camera cam , int layers , Vector2 position , GameObject[] ignore , GameObject[] filter , out int materialIndex )
        {
            _didAddPickHandlers = true;
            materialIndex = -1;
            if (this == null) return null;
            
            if (layers != -1 && (gameObject.layer & layers) == 0) return null;
            if (ignore != null && ignore.Contains(gameObject)) return null;
            if (filter != null && !filter.Contains(gameObject)) return null;
            
            var ray = cam.ScreenToWorldPoint( position );
            //Bounds b = new Bounds((Vector3)_hitbox.Center + _hitbox.transform.position, new Vector3(_hitbox.Size.x, _hitbox.Size.y, 1000000));
            bool hit = GlobalContains(ray);
            return hit ? gameObject : null;
        }
        
        #endif

        /// <summary>
        /// Sets the corner of the hitbox to newPos. Preserves the original center.
        /// NewPos is a global position.
        /// </summary>
        /// <param name="newPos">Global position for new corner.</param>
        public void SetCornerGlobal(Vector2 newPos)
        {
            Vector2 newExtents = (Center + (Vector2)transform.position - newPos).Abs();
            var bounds = Bounds;
            bounds.size = newExtents * 2;
            bounds.center = Center;
            Bounds = bounds;
        }

        public bool GlobalContains(Vector3 ray) => Bounds.Contains(ray - transform.position);
    }
}