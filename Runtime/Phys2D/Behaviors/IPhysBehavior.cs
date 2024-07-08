using System.Collections.Generic;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public interface IPhysBehavior
    {
        public PhysState ProcessSurroundings(PhysState p, Dictionary<Direction, PhysObj[]> surroundings);
    }
}