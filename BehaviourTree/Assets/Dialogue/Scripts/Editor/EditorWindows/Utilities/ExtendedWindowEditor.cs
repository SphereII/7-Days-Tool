using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dialogue.Editor.EditorWindows
{
    public class ExtendedWindowEditor : UnityEditor.Editor
    {
        protected SerializedProperty serializedProperty;
        protected SerializedProperty currentProperty;
        public static Texture2D corner
        {
            get { return _corner != null ? _corner : _corner = Resources.Load<Texture2D>("xnode_corner"); }
        }

        private static Texture2D _corner;


        protected bool showList = true;
        private bool showNote = false;

        private Vector2 size;
        private bool isDragging;

        // In order to display the List of objects, we need to convert it to a reorderable list.
        protected ReorderableList list;

        protected void ShowNote()
        {
            var notes = serializedObject.FindProperty("notes");
            if (notes == null) return;

            showNote = EditorGUILayout.Foldout(showNote, $"Notes");
            if (!showNote) return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(notes, true);
            EditorGUILayout.EndVertical();
        }
    }
}