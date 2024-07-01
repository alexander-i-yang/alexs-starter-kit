using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Helpers;
using ASK.Runtime.Phys2D.Modules;
using UnityEngine;

namespace ASK.Runtime.Phys2D {
    [RequireComponent(typeof(Hitbox), typeof(ISquishBehavior))]
    public abstract class PhysObj : MonoBehaviour
    {
        private Hitbox _myHitbox;
        [SerializeField] private PhysState _physState = new PhysState();
        
        [SerializeField] private IPhysModule[] _physModules;
        [SerializeField] private ICollisionBehavior[] _collisionModules;
        private ISquishBehavior _squishBehavior;
        
        public IPhysModule[] PhysModules => _physModules;

        protected Hitbox myHitbox
        {
            get
            {
                if (_myHitbox == null) _myHitbox = GetComponent<Hitbox>();
                return _myHitbox;
            }
        }

        public Vector2 velocity { get; protected set; }  = Vector2.zero;
        
        [SerializeField] private Vector2 _subPixels = Vector2.zero;

        private void Awake()
        {
            _physModules = GetComponents<IPhysModule>();
            _collisionModules = GetComponents<ICollisionBehavior>();
            _squishBehavior = GetComponent<ISquishBehavior>();
        }
        
        private void FixedUpdate()
        {
            var surroundings = CheckCollisions(Vector2.down);
            foreach (var module in _physModules)
            {
                _physState = module.ProcessSurroundings(_physState, surroundings, Vector2.down);
            }
            
            Move(_physState.velocity * Game.TimeManager.FixedDeltaTime);
        }

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
        public PhysObj[] CheckCollisions(Vector2 direction) =>
            CheckCollisions<PhysObj>(direction);

        /// <summary>
        /// Checks the interactable layer for any collisions. Will call onCollide if it hits anything.
        /// </summary>
        /// <param name="direction"><b>MUST</b> be a cardinal direction with a <b>magnitude of one.</b></param>
        /// <param name="onCollide">(<b>physObj</b> collided with, <b>Vector2</b> direction),
        /// returns physObj when collide, otherwise null.</param>
        /// <typeparam name="T">Type of physObj to check against. Must inherit from PhysObj.</typeparam>
        /// <returns></returns>
        public T[] CheckCollisions<T>(Vector2 direction) where T : PhysObj
        {
            var physObjs = FindObjectsOfType<T>();
            List<T> ret = new();
            foreach (var p in physObjs)
            {
                bool willCollide = myHitbox.WillCollide(p.myHitbox, direction);
                if (willCollide) ret.Add(p);
            }

            return ret.ToArray();
        }

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

        /// <summary>
        /// Called when p bumps into this PhysObj.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="direction">The direction p was moving.</param>
        public virtual void OnCollideWith(PhysObj p, Vector2 direction) {}
        // public abstract bool Collidable(PhysObj collideWith);
        public bool OnCollide(PhysObj p, Vector2 direction)
        {
            p.OnCollideWith(this, direction);
            foreach (var module in _collisionModules)
            {
                _physState = module.OnCollide(_physState, p, direction);
            }
            return _physState.Collided;
        }

        /*public virtual bool PlayerCollide(Actor p, Vector2 direction) {
            return OnCollide(p, direction);
        }*/

        //TODO: change this so that it only looks for actors near me
        public static Actor[] AllActors() {
            return FindObjectsOfType<Actor>();
        }

        public bool Squish(PhysObj p, Vector2 d) => _squishBehavior.Squish(p, d);

        /**
         * Gets the physObj underneath this PhysObj's feet.
         */
        // public PhysObj GetBelowPhysObj() => CheckCollisions(Vector2.down, (p, d) => this.Collidable(p));

        /**
         * Calculates the physics object this PhysObj is riding on.
         */
        // public virtual PhysObj RidingOn() => GetBelowPhysObj();

        //public int ColliderBottomY() => Convert.ToInt16(transform.position.y + myHitbox.offset.y - myHitbox.bounds.extents.y);

        //public int ColliderTopY() => Convert.ToInt16(transform.position.y + myHitbox.offset.y + myHitbox.bounds.extents.y);
    }
}