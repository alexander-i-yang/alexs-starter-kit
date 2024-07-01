using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using ASK.Runtime.Phys2D;
using ASK.Runtime.Phys2D.Modules;
using MyBox;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ASK.Editor
{
    [CustomPropertyDrawer(typeof(ModuleProp))]
    public class PhysObjEditor : PropertyDrawer
    {
        private int _index;

        private IEnumerable<Type> types = EditorReflectionUtils.ImplementableTypes<IPhysBehavior>();
        
        /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var types = SelectableTypes<IPhysBehavior>();
            var options = types.Select(t => $"{t.Name} ({t})").ToList();
            options.Insert(0, "Select");
            
            EditorGUI.BeginChangeCheck();
            _index = EditorGUILayout.Popup(_index, options.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log(_index);
            }
        }*/
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ModuleProp waveData = (ModuleProp)property.boxedValue;
            // EditorGUI.DropdownButton(position, new GUIContent("Hello"), FocusType.Keyboard);
            
            var options = types.Select(t => $"{t.Name} ({t})").ToList();
            options.Insert(0, "Select");
            EditorGUI.BeginChangeCheck();
            _index = EditorGUI.Popup(position, _index, options.Select(x => new GUIContent(x)).ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log(_index);
            }
        }
    }
}