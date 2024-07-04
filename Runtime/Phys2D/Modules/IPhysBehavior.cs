using System;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    public interface IPhysBehavior
    {
        public PhysState ProcessSurroundings(PhysState p, PhysObj[] surroundings, Vector2 direction);
    }
}