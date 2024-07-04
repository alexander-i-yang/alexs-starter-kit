using System;
using System.Collections.Generic;
using System.Linq;
using MyBox.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ASK.Editor.Standalone
{
    [CustomPropertyDrawer(typeof(ChilrdenClassesDropdown), true)]
    public class ChilrdenClassesDropdownDrawer : PropertyDrawer
    {
        private class PViewData : EditorViewData
        {
            public int Index = -1;
        }

        private static EditorViewDataStore<PViewData> _viewDataStore = new();
        private const int POPUP_PADDING = 8;
        private const int PADDING = 4;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                DrawDefault(position, property);
                return;
            }

            position.y += PADDING;
            
            var viewData = _viewDataStore.GetViewData(property);
            if (viewData.Index == -1) viewData.Index = GetIndexOfType(property);

            EditorGUI.BeginProperty(position, label, property);
            DrawProperty(position, property, viewData);
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            EditorGUI.EndProperty();
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
                var a = EditorGUI.GetPropertyHeight(property);
                viewData.Index = DrawPopup(position, viewData.Index);
                if (check.changed) SetBehaviorType(property, viewData);
            }

            if (viewData.Index == 0) return;

            position = AutoPosition.IncrLine(position, 0);
            position.y += POPUP_PADDING;
            using (new EditorGUI.IndentLevelScope(1))
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }

        private int DrawPopup(Rect position, int index)
        {
            var style = EditorStyles.popup;
            int padding = 8;
            //style.padding = new RectOffset(padding, 0, 0, 0);
            style.fixedHeight = 18 + POPUP_PADDING;
            position.height += POPUP_PADDING;
            int indent = index == 0 ? 0 : 2;
            using (new EditorGUI.IndentLevelScope(indent))
                return EditorGUI.Popup(position, index, GetDropdownContent());
        }

        private int GetIndexOfType(SerializedProperty property)
        {
            var boxedValue = property.boxedValue;
            if (boxedValue == null) return 0;
            return Types.IndexOf(boxedValue.GetType()) + 1; //Account for initial "Select" option
        }

        private void SetBehaviorType(SerializedProperty property, PViewData viewData)
        {
            property.boxedValue = CreateInstance(viewData.Index);
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.Repaint();
        }

        public GUIContent[] GetDropdownContent()
        {
            var options = Types.Select(t => $"{t.Name} ({t})").ToList();
            options.Insert(0, "Select");
            EditorGUI.BeginChangeCheck();
            return options.Select(x => new GUIContent(x)).ToArray();
        }

        private object CreateInstance(int index)
        {
            if (index == 0) return null;
            Type t = Types[index - 1];
            return Activator.CreateInstance(t);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return AutoPosition.GetHeight(0)
                   + EditorGUI.GetPropertyHeight(property, GUIContent.none, true)
                   + EditorGUIUtility.standardVerticalSpacing
                   //+ (property.isExpanded ? 8 : 0)
                   + POPUP_PADDING + 2*PADDING;
        }

        private List<Type> Types => ((ChilrdenClassesDropdown)this.attribute).Types;
    }
}