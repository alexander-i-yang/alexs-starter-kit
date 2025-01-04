using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Runtime.Core;
#if UNITY_EDITOR
using ASK.Editor.Standalone;
#endif
using ASK.Runtime.Phys2D.Behaviors;
using ASK.Runtime.Phys2D.Defaults;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Runtime.Phys2D
{
    [RequireComponent(typeof(Hitbox), typeof(ISquishBehavior))]
    public abstract class PhysObj : MonoBehaviour
    {
        private Hitbox _myHitbox;
        
        [SerializeField]
        private PhysObjStrategy _physObjStrategy = new();

        private PhysProperty[] _properties;

        private ISquishBehavior _squishBehavior;

        protected Hitbox myHitbox
        {
            get
            {
                if (_myHitbox == null) _myHitbox = GetComponent<Hitbox>();
                return _myHitbox;
            }
        }
        
        public Vector2 SubPixels { get; private set; } = Vector2.zero;

        private void Awake()
        {
            _squishBehavior = GetComponent<ISquishBehavior>();
            _properties = GetComponents<PhysProperty>();
        }
        
        private void FixedUpdate()
        {
            // if (SelectedInEditor()) return;

            var surroundings = GetSurroundings();
            Vector2 velocity = _physObjStrategy.Process(surroundings);

            Move(velocity * Game.TimeManager.FixedDeltaTime);
        }

        private Dictionary<Direction, PhysObj[]> GetSurroundings()
        {
            return new Dictionary<Direction, PhysObj[]>
            {
                { Direction.Left, CheckCollisions(Vector2.left) },
                { Direction.Right, CheckCollisions(Vector2.right) },
                { Direction.Up, CheckCollisions(Vector2.up) },
                { Direction.Down, CheckCollisions(Vector2.down) },
            };
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
                if (WillCollide(p, direction))
                    ret.Add(p);
            }

            return ret.ToArray();
        }

        private bool WillCollide<T>(T physObj, Vector2 direction) where T : PhysObj
        {
            return this != physObj && myHitbox.WillCollide(physObj.myHitbox, direction);
        }

        protected void Move(Vector2 vel)
        {
            vel += SubPixels;
            int moveX = (int)Math.Abs(vel.x);
            if (moveX != 0)
            {
                Vector2 xDir = new Vector2(vel.x / moveX, 0).normalized;
                MoveGeneral(xDir, moveX, OnCollide);
            }

            int moveY = (int)Math.Abs(vel.y);
            if (moveY != 0)
            {
                Vector2 yDir = new Vector2(0, vel.y / moveY).normalized;
                MoveGeneral(yDir, moveY, OnCollide);
            }

            Vector2 truncVel = new Vector2((int)vel.x, (int)vel.y);
            SubPixels = vel - truncVel;
        }

        public abstract bool MoveGeneral(Vector2 direction, int magnitude, Func<PhysObj, Vector2, bool> onCollide);

        /// <summary>
        /// Called when p bumps into this PhysObj.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="direction">The direction p was moving.</param>
        public virtual void OnCollideWith(PhysObj p, Vector2 direction)
        {
        }

        // public abstract bool Collidable(PhysObj collideWith);
        public bool OnCollide(PhysObj p, Vector2 direction)
        {
            if (p == this) return false;
            p.OnCollideWith(this, direction);
            return _physObjStrategy.OnCollide(p, direction);
        }

        /*public virtual bool PlayerCollide(Actor p, Vector2 direction) {
            return OnCollide(p, direction);
        }*/

        //TODO: change this so that it only looks for actors near me
        public static Actor[] AllActors()
        {
            return FindObjectsOfType<Actor>();
        }

        public bool IsRiding(PhysObj p) => _physObjStrategy.IsRiding(p);

        public bool Squish(PhysObj p, Vector2 d) => _squishBehavior.Squish(p, d);
        
        public void Ride(Vector2 direction) => Move(direction);

        /*private bool SelectedInEditor()
        {
            #if UNITY_EDITOR
            return Selection.activeGameObject == gameObject && Tools.current == Tool.Move;
            #endif
            return false;
        }*/

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
        public T GetProperty<T>() where T : PhysProperty
        {
            foreach (var p in _properties)
            {
                if (p is T tp)
                {
                    return tp;
                }
            }

            return default;
        }

        /*public Vector2 GetVelocity() => _physState.velocity;

        public PhysState PhysState => _physState;*/
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.Label(transform.position,
                $"Velocity: <{(int)_physObjStrategy.Velocity().x}, {(int)_physObjStrategy.Velocity().y}>");
        }
        #endif
    }
}