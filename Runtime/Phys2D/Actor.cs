using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Editor.Standalone;
using ASK.Runtime.Phys2D.Behaviors;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.Phys2D {
    public sealed class Actor : PhysObj
    {
        // [Foldout("Movement Events")]
        // [SerializeField] public ActorEvent OnLand;
        
        //public bool IsMovingUp => velocityY >= 0;
        
        //public int Facing => Math.Sign(velocityX);    //-1 is facing left, 1 is facing right

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
            Actor[] allActors = AllActors();
            // If the actor moves at least 1 pixel, Move one pixel at a time
            while (remainder > 0)
            {
                var collisions = CheckCollisions<PhysObj>(direction);
                bool collision = false;
                
                HashSet<Actor> ridingActors = new HashSet<Actor>(allActors.Where(c => c.IsRiding(this)));

                foreach (var collideObj in collisions)
                {
                    if (onCollide(collideObj, direction))
                    {
                        collision = true;
                        break;
                    }
                }

                if (collision) return true;
                
                transform.position += new Vector3((int)direction.x, (int)direction.y, 0);
                
                foreach (var a in ridingActors) {
                    a.Ride(direction);
                }
                
                remainder--;
            }
            
            return false;
        }

        #region Jostling
        public bool PushSquish(Vector2 direction, PhysObj pusher)
        {
            return MoveGeneral(direction, 1, (ps, ds) => {
                if (OnCollide(ps, ds)) return Squish(ps, ds);
                return false;
            });
        }
        /*public void Land()
        {
            OnLand.Invoke();
            velocityY = 0;
        }*/

        #endregion
    }
}