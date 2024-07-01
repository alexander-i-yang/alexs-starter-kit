using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ASK.Animation;
using ASK.Runtime.Phys2D;
using MyBox.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ASK.Editor
{
    public static class EditorReflectionUtils
    {
        /// <summary>
        /// Returns non-abstract classes that derive from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> ImplementableTypes<T>()
        {
            var derived = FindDerivedTypes<T>();
            return derived.Where(t => t.IsClass && !t.IsAbstract);
        }
        
        /// <summary>
        /// Returns all derived interfaces, abstract classes, etc. that derive from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> FindDerivedTypes<T>()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> ret = new();
            foreach (var assembly in allAssemblies)
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (typeof(T).IsAssignableFrom(t)) ret.Add(t);
                }
            }
            return ret;
        }
    }
}