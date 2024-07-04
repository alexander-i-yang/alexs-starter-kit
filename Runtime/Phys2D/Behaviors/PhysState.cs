using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Runtime.Phys2D.Behaviors
{
    [Serializable]
    public struct PhysState
    {
        public bool collided;
        public Vector2 velocity;
        public bool grounded;

        public PhysObj[] ridingOn;
    }
}