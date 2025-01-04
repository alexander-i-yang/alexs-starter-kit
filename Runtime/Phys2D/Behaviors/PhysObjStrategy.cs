using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Editor.Standalone;
using ASK.Runtime.Core;
using ASK.Runtime.Phys2D.Defaults;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Runtime.Phys2D.Behaviors
{

    /*public abstract class PhysObjStrategy
    {
        public abstract Vector2 Process(Dictionary<Direction, PhysObj[]> surroundings);

        public abstract bool IsRiding(PhysObj physObj);

        public abstract bool OnCollide(PhysObj p, Vector2 direction);

        public abstract Vector2 Velocity();
    }*/

    [Serializable]
    public class PhysObjStrategy
    {
        [SerializeField]
        private PhysState _physState = new();
        
        [SerializeReference]
        #if UNITY_EDITOR
        [ChilrdenClassesDropdown(typeof(IPhysBehavior))]
        #endif
        private IPhysBehavior[] _physModules =
        {
            new GravityPhysBehavior(),
        };
        
        [SerializeReference]
        #if UNITY_EDITOR
        [ChilrdenClassesDropdown(typeof(ICollisionBehavior))]
        #endif
        private ICollisionBehavior[] _collisionModules;

        public Vector2 Process(Dictionary<Direction, PhysObj[]> surroundings)
        {
            _physState = ResetPhysState(_physState);
            foreach (var module in _physModules)
            {
                _physState = module.ProcessSurroundings(_physState, surroundings);
            }

            return _physState.velocity;
        }
        
        private PhysState ResetPhysState(PhysState p)
        {
            p.collided = false;
            p.grounded = false;
            return p;
        }
        
        public bool IsRiding(PhysObj physObj) => _physState.ridingOn.Contains(physObj);

        public bool OnCollide(PhysObj p, Vector2 direction)
        {
            foreach (var module in _collisionModules)
            {
                _physState = module.OnCollide(_physState, p, direction);
            }

            return _physState.collided;
        }

        public Vector2 Velocity() => _physState.velocity;
    }
    
    [Serializable]
    public class PhysState
    {
        public bool collided;
        public Vector2 velocity;
        public bool grounded;

        public PhysObj[] ridingOn;

        [SerializeField]
        public InputState InputState;
    }
    
    [Serializable]
    public struct InputState
    {
        public bool jumpPressed;
        public float x;
    }
}