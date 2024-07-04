using System;
using System.Collections.Generic;
using ASK.Editor.Standalone;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public interface IPhysBehavior
    {
        public PhysState ProcessSurroundings(PhysState p, Dictionary<Vector2, PhysObj[]> surroundings);
    }
}