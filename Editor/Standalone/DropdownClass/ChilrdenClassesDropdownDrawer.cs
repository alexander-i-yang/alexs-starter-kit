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
        private const int POPUP_INDENT = 2 * 12;
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
            //EditorUtility.SetDirty(property.serializedObject.targetObject);
            EditorGUI.EndProperty();
        }

        private void DrawDefault(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(position, property, true);
        }

        private void DrawProperty(Rect position, SerializedProperty property, PViewData viewData)
        {
            position = AutoPosition.IncrLine(position, 0);

            DrawPopup(position, property, viewData);

            if (viewData.Index == 0) return;

            position = AutoPosition.IncrLine(position, 0);
            position.y += POPUP_PADDING;
            using (new EditorGUI.IndentLevelScope(1))
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }

        private void DrawPopup(Rect position, SerializedProperty property, PViewData viewData)
        {
            var style = EditorStyles.popup;

            style.fixedHeight = 18 + POPUP_PADDING;
            style.richText = true;
            position.height += POPUP_PADDING;

            int index = viewData.Index;
            if (index != 0 && property.hasChildren)
            {
                
                position.x += POPUP_INDENT;
                position.width -= POPUP_INDENT;
            }

            GenericMenu.MenuFunction2 onClick = i =>
            {
                viewData.Index = (int)i;
                SetBehaviorType(property, viewData.Index);
            };

            if (EditorGUI.DropdownButton(position, GetDropdownLabel(property), FocusType.Keyboard, style))
                ShowMenu(position, onClick, index);
            //return EditorGUI.Popup(position, index, GetDropdownContent());
        }

        private GUIContent GetDropdownLabel(SerializedProperty p)
        {
            string ret = "";
            var box = p.boxedValue;
            
            if (box == null) ret = "<b>Select</b>";
            else ret = $"<b>{box.GetType().Name}</b> <color=#ffffff80>({box.GetType()})</color>";
            
            return new(ret);
        }


        private int GetIndexOfType(SerializedProperty property)
        {
            var boxedValue = property.boxedValue;
            if (boxedValue == null) return 0;
            return Types.IndexOf(boxedValue.GetType()) + 1; //Account for initial "Select" option
        }

        private void SetBehaviorType(SerializedProperty property, int i)
        {
            property.boxedValue = CreateInstance(i);
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.Repaint();
        }

        private GUIContent[] GetDropdownContent()
        {
            var options = Types.Select(t => $"{t.Name} ({t})").ToList();
            options.Insert(0, "Select");
            return options.Select(x => new GUIContent(x)).ToArray();
        }

        private void ShowMenu(Rect position, GenericMenu.MenuFunction2 onClick, int index)
        {
            GenericMenu menu = new GenericMenu();

            var contents = GetDropdownContent();

            for (var i = 0; i < contents.Length; i++)
            {
                var content = contents[i];
                menu.AddItem(content, i == index, onClick, i);
            }

            menu.DropDown(position);
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
                   + POPUP_PADDING + 2 * PADDING;
        }

        private List<Type> Types => ((ChilrdenClassesDropdown)this.attribute).Types;
    }
}