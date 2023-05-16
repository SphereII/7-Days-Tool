using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dialogue.Editor.EditorWindows
{
    public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public List<string> listItems;
        public Action<string> onSetIndexCallback;
        public SerializedProperty so;

        public StringListSearchProvider(List<string> items, Action<string> callback)
        {
            listItems = items;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchList = new List<SearchTreeEntry>();
            searchList.Add(new SearchTreeGroupEntry(new GUIContent("List"), 0));
            foreach (string item in listItems)
            {
                var entry = new SearchTreeEntry(new GUIContent(item));
                entry.level = 1;
                entry.userData = item.Trim();
                searchList.Add(entry);
            }

            return searchList;
        }


        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (so != null)
            {
                so.stringValue = SearchTreeEntry.userData.ToString();
                so.serializedObject.ApplyModifiedProperties();
            }

            onSetIndexCallback?.Invoke((string) SearchTreeEntry.userData);
            return true;
        }
    }
}