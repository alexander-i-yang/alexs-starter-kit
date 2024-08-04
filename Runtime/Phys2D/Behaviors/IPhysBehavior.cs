using System.Collections.Generic;
using ASK.Runtime.Core;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public interface IPhysBehavior
    {
        public PhysState ProcessSurroundings(PhysState p, Dictionary<Direction, PhysObj[]> surroundings);
    }
}