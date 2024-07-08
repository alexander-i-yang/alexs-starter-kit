using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ASK.Core;
using ASK.Editor;
using ASK.Editor.Standalone;
using ASK.Runtime.Phys2D.Behaviors;
using ASK.Runtime.Phys2D.Defaults;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ASK.Runtime.Phys2D
{
    [RequireComponent(typeof(Hitbox), typeof(ISquishBehavior))]
    public abstract class PhysObj : MonoBehaviour
    {
        private Hitbox _myHitbox;
        [SerializeField] private PhysState _physState;

        [SerializeReference]
        [ChilrdenClassesDropdown(typeof(IPhysBehavior))]
        private IPhysBehavior[] _physModules =
        {
            new GravityPhysBehavior()
        };

        [SerializeReference]
        [ChilrdenClassesDropdown(typeof(ICollisionBehavior))]
        private ICollisionBehavior[] _collisionModules;

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

        [SerializeField] private Vector2 _subPixels = Vector2.zero;

        private void Awake()
        {
            _squishBehavior = GetComponent<ISquishBehavior>();
            _properties = GetComponents<PhysProperty>();
        }

        private void FixedUpdate()
        {
            // if (SelectedInEditor()) return;
            
            _physState = ResetPhysState(_physState);
            var surroundings = GetSurroundings();
            foreach (var module in _physModules)
            {
                if (module == null) continue;
                _physState = module.ProcessSurroundings(_physState, surroundings);
            }

            Move(_physState.velocity * Game.TimeManager.FixedDeltaTime);
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

        /*public float velocityY {
            get { return _physState.velocity.y; }
            protected set { _physState.velocity = new Vector2(_physState.velocity.x, value); }
        }

        public float velocityX {
            get { return _physState.velocity.x; }
            protected set { _physState.velocity = new Vector2(value, _physState.velocity.y); }
        }*/

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
            vel += _subPixels;
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
            _subPixels = vel - truncVel;
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
            foreach (var module in _collisionModules)
            {
                _physState = module.OnCollide(_physState, p, direction);
            }

            return _physState.collided;
        }

        /*public virtual bool PlayerCollide(Actor p, Vector2 direction) {
            return OnCollide(p, direction);
        }*/

        //TODO: change this so that it only looks for actors near me
        public static Actor[] AllActors()
        {
            return FindObjectsOfType<Actor>();
        }
        
        public bool IsRiding(PhysObj p) => _physState.ridingOn.Contains(p);

        public bool Squish(PhysObj p, Vector2 d) => _squishBehavior.Squish(p, d);
        
        private PhysState ResetPhysState(PhysState p)
        {
            p.collided = false;
            p.grounded = false;
            return p;
        }
        
        public void Ride(Vector2 direction) => Move(direction);

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.Label(transform.position,
                $"Velocity: <{(int)_physState.velocity.x}, {(int)_physState.velocity.y}>");
        }
        #endif

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
        public T GetProperty<T>() where T : class
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
    }
}