/*using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Animation;
using ASK.Editor.Utils;
using ASK.Runtime.Phys2D.Modules;
using UnityEditor;
using UnityEngine;

namespace ASK.Editor
{
    [CustomPropertyDrawer(typeof(ModuleContainer))]
    public class ModuleDrawer : PropertyDrawer
    {
        private class ModuleViewData : EditorViewData
        {
            public int Index = -1;
        }
        private static EditorViewDataStore<ModuleViewData> _viewDataStore = new();
        
        private List<Type> types = EditorReflection.ImplementableTypes<IPhysBehavior>().ToList();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ModuleViewData viewData = _viewDataStore.GetViewData(property);
            
            if (viewData.Index == -1) viewData.Index = GetIndexOfType(property);
            
            position = AutoPosition.IncrLine(position, 0);
            
            EditorGUI.BeginChangeCheck();
            viewData.Index = EditorGUI.Popup(position, viewData.Index, GetDropdownContent());
            if (EditorGUI.EndChangeCheck())
            {
                SetBehaviorType(property, viewData);
            }
            
            if (viewData.Index == 0) return;
            position = AutoPosition.IncrLine(position, 1);
            
            if (property != null) EditorGUI.PropertyField(position, GetReal(property), true);
        }

        private SerializedProperty GetReal(SerializedProperty property) =>
            EditorReflection.GetChildProperty(property, nameof(ModuleContainer.PhysBehavior));

        private void SetBehaviorType(SerializedProperty property, ModuleViewData viewData)
        {
            var p = GetReal(property);
            p.boxedValue = CreateInstance(viewData.Index);
            property.serializedObject.ApplyModifiedProperties();
        }

        private IPhysBehavior CreateInstance(int index)
        {
            if (index == 0) return null;
            Type t = types[index - 1];
            return (IPhysBehavior)Activator.CreateInstance(t);
        }

        private int GetIndexOfType(SerializedProperty property)
        {
            var real = GetReal(property);
            if (real == null || real.boxedValue == null) return 0;
            return types.IndexOf(real.boxedValue.GetType());
        }

        public GUIContent[] GetDropdownContent()
        {
            var options = types.Select(t => $"{t.Name} ({t})").ToList();
            options.Insert(0, "Select");
            EditorGUI.BeginChangeCheck();
            return options.Select(x => new GUIContent(x)).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var viewData = _viewDataStore.GetViewData(property);
            float ret = AutoPosition.GetHeight(1);
            if (viewData.Index != 0)
            {
                property = GetReal(property);
                if (property == null) return ret;
                ret += EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            }
            return ret;
        }
    }
}*/