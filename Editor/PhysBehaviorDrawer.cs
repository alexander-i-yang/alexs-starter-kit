using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Editor.Utils;
using ASK.Runtime.Phys2D.Modules;
using MyBox.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ASK.Editor
{
    [CustomPropertyDrawer(typeof(IPhysBehavior), true)]
    public class PhysBehaviorDrawer : PropertyDrawer
    {
        private class PViewData : EditorViewData
        {
            public int Index = -1;
        }

        private static EditorViewDataStore<PViewData> _viewDataStore = new ();
        private static readonly List<Type> _types = EditorReflection.ImplementableTypes<IPhysBehavior>().ToList();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                DrawDefault(position, property);
                return;
            }
            
            var viewData = _viewDataStore.GetViewData(property);
            if (viewData.Index == -1) viewData.Index = GetIndexOfType(property);
            
            EditorGUI.BeginProperty(position, label, property);
            DrawProperty(position, property, viewData);
            EditorGUI.EndProperty();
            
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        private void DrawDefault(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(position, property, true);
        }

        private void DrawProperty(Rect position, SerializedProperty property, PViewData viewData)
        {
            position = AutoPosition.IncrLine(position, 0);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var dropdownPos = position;
                int indent = viewData.Index == 0 ? 0 : 2;
                using (new EditorGUI.IndentLevelScope(indent))
                    viewData.Index = EditorGUI.Popup(dropdownPos, viewData.Index, GetDropdownContent());
                if (check.changed) SetBehaviorType(property, viewData);
            }

            if (viewData.Index == 0) return;

            position = AutoPosition.IncrLine(position, 0);
            using (new EditorGUI.IndentLevelScope(1))
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }
        
        private int GetIndexOfType(SerializedProperty property)
        {
            var real = EditorReflection.GetArrayElement(property);
            if (real == null || real.boxedValue == null) return 0;
            return _types.IndexOf(real.boxedValue.GetType());
        }

        private void SetBehaviorType(SerializedProperty property, PViewData viewData)
        {
            var p = EditorReflection.GetArrayElement(property);
            p.boxedValue = CreateInstance(viewData.Index);
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.Repaint();
        }
        
        public GUIContent[] GetDropdownContent()
        {
            var options = _types.Select(t => $"{t.Name} ({t})").ToList();
            options.Insert(0, "Select");
            EditorGUI.BeginChangeCheck();
            return options.Select(x => new GUIContent(x)).ToArray();
        }
        
        private IPhysBehavior CreateInstance(int index)
        {
            if (index == 0) return null;
            Type t = _types[index - 1];
            return (IPhysBehavior)Activator.CreateInstance(t);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return AutoPosition.GetHeight(0)
                   + EditorGUI.GetPropertyHeight(property, GUIContent.none, true)
                   + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}