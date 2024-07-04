using System.Collections.Generic;
using UnityEditor;

namespace ASK.Editor.Utils
{
    public abstract class EditorViewData {}
    
    /// <summary>
    /// In PropertyDrawers, local fields will act like static fields if the properties are in a list.
    /// This class solves that by placing local fields within one instance of V for each class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EditorViewDataStore<T> where T : EditorViewData, new()
    {
        Dictionary<string, T> viewDatas = new ();
        
        public T GetViewData(SerializedProperty property)
        {
            T viewData;
            if (!viewDatas.TryGetValue(property.propertyPath, out viewData)) {
                viewData = new T();
                viewDatas[property.propertyPath] = viewData;
            }

            return viewData;
        }
    }
}