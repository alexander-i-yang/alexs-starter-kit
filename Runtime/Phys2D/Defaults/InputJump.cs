using System;
using System.Collections.Generic;
using ASK.Runtime.Phys2D;
using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace Test.Player
{
    public class InputJump : MonoBehaviour
    {
        [SerializeField] private float JumpV;
        [SerializeField] private float DoubleJumpV;
        
        private bool canDoubleJump;

        [SerializeField]
        private float maxJJP = 0.125f;
        
        [SerializeField]
        private float jjp;

        private Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        void Update()
        {
            bool jumpPressed = Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) ||
                                     Input.GetKeyDown(KeyCode.W);

            if (jumpPressed) Jump();
            
            PhysState p = _actor.PhysState;

            if (p.grounded && jjp > 0)
            {
                _actor.SetVelocity(new Vector2(p.velocity.x, JumpV));
                
                if (Tutorial.JumpText.activeSelf)
                {
                    Tutorial.JumpText.SetActive(false);
                    FindObjectOfType<Tutorial>().StartAim();
                }
                
            }
            
            if (jjp > 0)
            {
                jjp = Math.Max(0, jjp - ASK.Core.Game.TimeManager.DeltaTime);
            }
        }

        void Jump()
        {
            PhysState p = _actor.PhysState;
            
            if (p.stun > 0) return;
            
            if (p.grounded) canDoubleJump = true;

            jjp = maxJJP;
        }
    }
}