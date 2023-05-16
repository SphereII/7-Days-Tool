
using System.Collections.Generic;
using System.Linq;
using Dialogue.Editor;
using Dialogue.Editor.EditorWindows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dialogue.Nodes.Editor
{
    public class EditorUIUtilities : MonoBehaviour
    {

        // Width of all the nodes.
    public static int GetWidth()
    {
        return 600;
    }

    // Helper control mechanism to display the contents of the Requirements / Actions.
    public static void DisplayControl(ref Vector2 position, SerializedProperty so, string description, float width,
        string search = "", string tooltip = "")
    {
        if (so == null) return;

        // Display the description
        var targetRect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(targetRect, new GUIContent(description, tooltip));

        // Display the property
        targetRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, width,
            EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(targetRect, so, GUIContent.none);

        // Add a Find if there's a valid search feature
        if (!string.IsNullOrEmpty(search))
        {
            // // If we have built in references, then show the find button and prepare the search window.
            var gameInfo = ConfigurationManager.Instance.GetGameInfo(search);
            if (gameInfo.Count > 0)
            {
                targetRect.y += EditorGUIUtility.singleLineHeight;
                if (GUI.Button(targetRect, new GUIContent("Find")))
                {
                    var searchProvider = ScriptableObject.CreateInstance<StringListSearchProvider>();
                    searchProvider.listItems = gameInfo;
                    searchProvider.so = so;
                    SearchWindow.Open(
                        new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                        searchProvider);
                }
            }
        }

        position.x += width + 10;
    }


    // Generic way to draw properties as if they were the Inspector.
    public static void DrawProperties(SerializedProperty prop, bool drawChildren)
    {
        var lastPropPath = string.Empty;
        foreach (SerializedProperty p in prop)
        {
            if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.BeginHorizontal();
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                EditorGUILayout.EndHorizontal();

                if (!p.isExpanded) continue;

                EditorGUI.indentLevel++;
                DrawProperties(p, drawChildren);
                EditorGUI.indentLevel--;
            }
            else
            {
                if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath)) continue;
                lastPropPath = p.propertyPath;

                if (p.hasChildren)
                    DrawProperties(p, drawChildren);
                else
                    EditorGUILayout.PropertyField(p, drawChildren);
            }
        }
    }

    // Generic way to find all child properties.
    protected static IEnumerable<SerializedProperty> GetDirectChildren(SerializedProperty parent)
    {
        var dots = parent.propertyPath.Count(c => c == '.');
        foreach (SerializedProperty inner in parent)
        {
            var isDirectChild = inner.propertyPath.Count(c => c == '.') == dots + 1;
            if (isDirectChild)
                yield return inner;
        }
    }

    }
}