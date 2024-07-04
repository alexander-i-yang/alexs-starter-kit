using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyBox.EditorTools;
using UnityEditor;

namespace ASK.Editor.Standalone
{
    public static class EditorReflection
    {
        /// <summary>
        /// Returns non-abstract classes that derive from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> ImplementableTypes(Type T)
        {
            var derived = FindDerivedTypes(T);
            return derived.Where(t => t.IsClass && !t.IsAbstract);
        }
        
        /// <summary>
        /// Returns all derived interfaces, abstract classes, etc. that derive from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> FindDerivedTypes(Type T)
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> ret = new();
            foreach (var assembly in allAssemblies)
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (T.IsAssignableFrom(t)) ret.Add(t);
                }
            }
            return ret;
        }
        
        public const BindingFlags BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags BINDING_FLAGS_PUB = BindingFlags.Public | BindingFlags.Instance;
        
        public static T GetRealProperty<T>(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath, BINDING_FLAGS);
            if (field == null) field = targetObjectClassType.GetField(property.propertyPath, BINDING_FLAGS_PUB);
            if (field != null)
            {
                var value = field.GetValue(targetObject);
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// Traverses the property for the property with the name given by [name]. Will create a copy first.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="name">Name of the property to find.</param>
        /// <param name="searchBound">Stops searching after this many iterations.</param>
        /// <returns></returns>
        public static SerializedProperty GetChildProperty(SerializedProperty property, string name, int searchBound=100)
        {
            var copy = property.Copy();
            for (int i = 0; i < searchBound; ++i)
            {
                if (copy.name == name) return copy;
                if (!copy.Next(true)) break;
            }
            return null;
        }

        public static SerializedProperty GetArrayElement(SerializedProperty property)
        {
            string path = property.GetFixedPropertyPath();
            
            if (!path.Contains("["))
                throw new ArgumentException(
                    $"Property {property.name} must be an array. Got path {property.GetFixedPropertyPath()}"
                    );
            
            var pathSplit = path.Split("[");
            path = pathSplit[pathSplit.Length - 2];
            int index = Int32.Parse(pathSplit[pathSplit.Length - 1].TrimEnd(']'));
            var x = property.serializedObject.FindProperty(path);
            return x.GetArrayElementAtIndex(index);
        }
        
    }
}