using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ASK.Runtime.Phys2D {
    public sealed class Solid : PhysObj
    {
        public override bool MoveGeneral(Vector2 direction, int magnitude, Func<PhysObj, Vector2, bool> onCollide) {
            if (magnitude < 0) throw new ArgumentException("Magnitude must be >0");

            int remainder = magnitude;

            Actor[] allActors = AllActors();
            
            // If the actor moves at least 1 pixel, Move one pixel at a time
            while (remainder > 0) {
                HashSet<Actor> ridingActors = new HashSet<Actor>(allActors.Where(c => c.IsRiding(this)));
                Actor[] collidingActors = CheckCollisions<Actor>(direction);
                foreach (var collidingActor in collidingActors)
                {
                    if (ridingActors.Contains(collidingActor)) {
                        ridingActors.Remove(collidingActor);
                    }
                    collidingActor.PushSquish(direction, this);
                }

                transform.position += new Vector3((int)direction.x, (int)direction.y, 0);
                
                //Ride actors
                foreach (var a in ridingActors) {
                    a.Ride(direction);
                }
                
                remainder -= 1;
            }
            
            return false;
        }
    }
}