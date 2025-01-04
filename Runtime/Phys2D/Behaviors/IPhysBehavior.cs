using System;
using System.Collections.Generic;
using System.Diagnostics;
using ASK.Runtime.Core;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    [Serializable]
    public abstract class IPhysBehavior
    {
        public abstract PhysState ProcessSurroundings(PhysState p, Dictionary<Direction, PhysObj[]> surroundings);
    }
}