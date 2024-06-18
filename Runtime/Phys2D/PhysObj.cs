using System;
using System.Collections.Generic;
using ASK.Core;
using ASK.Helpers;
using UnityEngine;

namespace ASK.Runtime.Phys2D {
    [RequireComponent(typeof(Hitbox))]
    public abstract class PhysObj : MonoBehaviour
    {
        private Hitbox _myHitbox;
        protected Hitbox myHitbox
        {
            get
            {
                if (_myHitbox == null) _myHitbox = GetComponent<Hitbox>();
                return _myHitbox;
            }
        }

        public Vector2 velocity { get; protected set; }  = Vector2.zero;
        
        [NonSerialized] private Vector2 _subPixels = Vector2.zero;

        public float velocityY {
            get { return velocity.y; }
            protected set { velocity = new Vector2(velocity.x, value); }
        }

        public float velocityX {
            get { return velocity.x; }
            protected set { velocity = new Vector2(value, velocity.y); }
        }

        /// <summary>
        /// See CheckCollisions<T>
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="onCollide"></param>
        /// <returns></returns>
        public PhysObj CheckCollisions(Vector2 direction, Func<PhysObj, Vector2, bool> onCollide) =>
            CheckCollisions<PhysObj>(direction, onCollide);

        /// <summary>
        /// Checks the interactable layer for any collisions. Will call onCollide if it hits anything.
        /// </summary>
        /// <param name="direction"><b>MUST</b> be a cardinal direction with a <b>magnitude of one.</b></param>
        /// <param name="onCollide">(<b>physObj</b> collided with, <b>Vector2</b> direction),
        /// returns physObj when collide, otherwise null.</param>
        /// <typeparam name="T">Type of physObj to check against. Must inherit from PhysObj.</typeparam>
        /// <returns></returns>
        public T CheckCollisions<T>(Vector2 direction, Func<T, Vector2, bool> onCollide) where T : PhysObj
        {
            var physObjs = FindObjectsOfType<T>();
            foreach (var p in physObjs)
            {
                bool willCollide = myHitbox.WillCollide(p.myHitbox, direction);
                if (willCollide)
                    if (onCollide(p, direction))
                        return p;
            }

            return null;
        }
        
        /*
        public T CheckCollisions<T>(Vector2 direction, Func<T, Vector2, bool> onCollide) where T : PhysObj{
            Vector2 colliderSize = myHitbox.size;
            Vector2 sizeMult = colliderSize - Vector2.one;
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Interactable", "Ground", "Player");
            filter.useLayerMask = true;
            Physics2D.BoxCast(transform.position, sizeMult, 0, direction, filter, hits, 8f);

            List<T> collideTs = new();
            
            foreach (var hit in hits) {
                if (hit.transform == transform)
                {
                    continue;
                }
                var t = hit.transform.GetComponent<T>();
                if (t != null)
                {
                    collideTs.Add(t);
                }
            }
            
            foreach (var s in collideTs)
            {
                bool proactiveCollision = ProactiveBoxCast(
                    s.transform, 
                    s.NextFrameOffset,
                    sizeMult,
                    1,
                    direction, 
                    filter
                );
                if (proactiveCollision)
                {
                    bool col = onCollide(s, direction);
                    if (col)
                    {
                        return s;
                    }
                }
            }
            
            return null;
        }*/
        
        /*private void OnDrawGizmosSelected() {
            Vector2 direction = velocity == Vector2.zero ? Vector2.up: velocity.normalized;
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;
            Vector2 colliderSize = col.size;
            Vector2 sizeMult = colliderSize - Vector2.one;
            // Vector2 sizeMult = colliderSize;
            BoxDrawer.DrawBoxCast2D(
                origin: (Vector2) transform.position,
                size: sizeMult,
                direction: direction,
                distance: 1,
                angle: 0,
                color: Color.blue
            );
        }*/

        protected void Move(Vector2 vel) {
            vel += _subPixels;
            int moveX = (int) Math.Abs(vel.x);
            if (moveX != 0) {
                Vector2 xDir = new Vector2(vel.x / moveX, 0).normalized;
                MoveGeneral(xDir, moveX, OnCollide);
            }

            int moveY = (int) Math.Abs(vel.y);
            if (moveY != 0) {
                Vector2 yDir = new Vector2(0, vel.y / moveY).normalized;
                MoveGeneral(yDir, moveY, OnCollide);
            }

            Vector2 truncVel = new Vector2((int) vel.x, (int) vel.y);
            _subPixels = vel - truncVel;
        }
        public abstract bool MoveGeneral(Vector2 direction, int magnitude, Func<PhysObj, Vector2, bool> onCollide);
        
        public abstract bool Collidable(PhysObj collideWith);
        public virtual bool OnCollide(PhysObj p, Vector2 direction) {
            return p.Collidable(this);
        }

        /*public virtual bool PlayerCollide(Actor p, Vector2 direction) {
            return OnCollide(p, direction);
        }*/

        public virtual bool IsGround(PhysObj whosAsking) {
            return Collidable(whosAsking);
        }

        //TODO: change this so that it only looks for actors near me
        public static Actor[] AllActors() {
            return FindObjectsOfType<Actor>();
        }
        
        public abstract bool Squish(PhysObj p, Vector2 d);
        
        /**
         * Gets the physObj underneath this PhysObj's feet.
         */
        public PhysObj GetBelowPhysObj() => CheckCollisions(Vector2.down, (p, d) => this.Collidable(p));

        /**
         * Calculates the physics object this PhysObj is riding on.
         */
        public virtual PhysObj RidingOn() => GetBelowPhysObj();

        //public int ColliderBottomY() => Convert.ToInt16(transform.position.y + myHitbox.offset.y - myHitbox.bounds.extents.y);
        
        //public int ColliderTopY() => Convert.ToInt16(transform.position.y + myHitbox.offset.y + myHitbox.bounds.extents.y);
    }
}