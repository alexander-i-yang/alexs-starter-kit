using UnityEditor;
using UnityEngine;

namespace ASK.Editor.Standalone
{
    public static class AutoPosition
    {
        public static Rect IncrLine(Rect position, int numLines)
        {
            position.position += new Vector2(0, EditorGUIUtility.singleLineHeight * numLines + EditorGUIUtility.standardVerticalSpacing);
            position.height = EditorGUIUtility.singleLineHeight;
            return position;
        }

        public static float GetHeight(int numLines) =>
            (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * numLines
            + EditorGUIUtility.standardVerticalSpacing;
    }
}