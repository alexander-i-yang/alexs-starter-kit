using System;
using System.Linq;
using ASK.Runtime.Phys2D.Modules;
using UnityEngine;

namespace ASK.Animation
{
    [Serializable]
    public class ModuleContainer
    {
        [SerializeReference]
        public IPhysBehavior PhysBehavior = new GravityPhysBehavior();
        public int Throwawy; //You need a primitive field for the editor to behave correctly. Why? Idk.
    }
}