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
                /*var stuck = CheckCollisions(Vector2.zero);
                foreach (var st in stuck)
                {
                    bool ret = st != this;
                    if (ret) {
                        Debug.LogError("Stuck against" + st);
                    }

                    return ret;
                }*/

                HashSet<Actor> ridingActors = new HashSet<Actor>(allActors.Where(c => c.IsRiding(this)));
                Actor[] collidingActors = CheckCollisions<Actor>(direction);
                foreach (var collidingActor in collidingActors)
                {
                    if (ridingActors.Contains(collidingActor)) {
                        ridingActors.Remove(collidingActor);
                    }
                    collidingActor.Push(direction, this);
                }

                //Ride actors
                foreach (var a in ridingActors) {
                    a.Ride(direction);
                }
                
                transform.position += new Vector3((int)direction.x, (int)direction.y, 0);
                remainder -= 1;
            }
            
            return false;
        }
    }
}