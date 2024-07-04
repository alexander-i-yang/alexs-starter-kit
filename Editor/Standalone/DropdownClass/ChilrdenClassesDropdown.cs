using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ASK.Editor.Standalone
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ChilrdenClassesDropdown : PropertyAttribute
    {
        public List<Type> Types { get; private set; }

        public ChilrdenClassesDropdown(Type t)
        {
            Types = EditorReflection.ImplementableTypes(t).ToList();
        }
    }
}