using System;
using ASK.Core;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.Phys2D {
    public sealed class Actor : PhysObj
    {
        [Foldout("Movement Events")]
        [SerializeField] public ActorEvent OnLand;

        [SerializeField, Foldout("Gravity")] protected int BonkHeadV;

        public bool IsMovingUp => velocityY >= 0;
        
        public int Facing => Math.Sign(velocity.x);    //-1 is facing left, 1 is facing right

        // private void Awake()
        // {
        //     JostleBehavior = GetComponent<JostleBehavior>();
        // }

        /// <summary>
        /// Moves this actor a specified number of pixels.
        /// </summary>
        /// <param name="direction"><b>MUST</b> be a cardinal direction with a <b>magnitude of one.</b></param>
        /// <param name="magnitude">Must be <b>non-negative</b> amount of pixels to move.</param>
        /// <param name="onCollide">Collision function that determines how to behave when colliding with an object</param>
        /// <returns>True if it needs to stop on a collision, false otherwise</returns>
        public override bool MoveGeneral(Vector2 direction, int magnitude, Func<PhysObj, Vector2, bool> onCollide) {
            if (magnitude < 0) throw new ArgumentException("Magnitude must be >0");

            int remainder = magnitude;
            // If the actor moves at least 1 pixel, Move one pixel at a time
            while (remainder > 0) {
                var collides = CheckCollisions(direction);
                bool collision = false;
                foreach (PhysObj p in collides)
                {
                    if (onCollide(p, direction))
                    {
                        collision = true;
                        break;
                    }
                }
                if (collision) {
                    return true;
                }
                transform.position += new Vector3((int)direction.x, (int)direction.y, 0);
                remainder--;
            }
            
            return false;
        }

        public void ApplyVelocity(Vector2 v)
        {
            velocity += v;
        }

        #region Jostling
        protected PhysObj ridingOn { get; private set; }
        //Prev velocity of RidingOn
        protected Vector2 prevRidingV { get; private set; }
        protected PhysObj prevRidingOn { get; private set; }
        public bool IsRiding(Solid solid) => ridingOn == solid;
        public void Ride(Vector2 direction) => Move(direction);
        
        /**
         * When there was a floor but now there's not
         */
        protected bool JumpedOff() => prevRidingOn != null && ridingOn == null;
        
        /**
         * When the floor was moving but now it's not
         */
        protected bool FloorStopped() => prevRidingV != Vector2.zero && ridingOn != null && ridingOn.velocity == Vector2.zero;
        
        protected bool ShouldApplyV() => JumpedOff() || FloorStopped();
        
        /**
         * Set _ridingOn to whatever CalcRiding returns.
         * Should get called every frame.
         */
        /*public virtual Vector2 ResolveJostle()
        {
            Vector2 ret = Vector2.zero;
            ridingOn = RidingOn();
            if (ShouldApplyV())
            {
                ret = ResolveApplyV(ret);
            }
            prevRidingOn = ridingOn;
            prevRidingV = ridingOn == null ? Vector2.zero : ridingOn.velocity;
            return ret;
        }*/
        
        /**
         * Input previousApplyVelocity, output new apply velocity.
         * Only called when shouldApplyV.
         */
        protected Vector2 ResolveApplyV(Vector2 v) => prevRidingV;
        
        public bool Push(Vector2 direction, Solid pusher)
        {
            return MoveGeneral(direction, 1, (ps, ds) => {
                if (ps != pusher && OnCollide(ps, ds)) return Squish(ps, ds);
                return false;
            });
        }
        
        public void BonkHead() {
            velocityY = Math.Min(BonkHeadV, velocityY);
        }
        
        public void Land()
        {
            OnLand.Invoke(velocity);
            velocityY = 0;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Handles.Label(transform.position, $"Velocity: <{(int)velocityX}, {(int)velocityY}>");
        }
        #endif
        
        #endregion

        public void SetVelocity(Vector2 newV) => velocity = newV;
    }
}