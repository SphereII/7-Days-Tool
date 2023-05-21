using System.Collections.Generic;
using System.Linq;
using Dialogue.Editor;
using Dialogue.Editor.EditorWindows;
using Dialogue.GameData.Dialogs;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Scripts.Editor.PropertyDrawers
{
    public class BaseDrawer : PropertyDrawer
    {
        protected const string Uxml = "Assets/Dialogue/UI/ActionRequirementDrawer.uxml";
        protected VisualElement _ve;
        protected ConfigurationManager Cfg = new ConfigurationManager();

        protected ConfigurationManager.ConfigMap ConfigMap;

        protected string Guid;
        // Action Description
        protected Label Description;

        // Types
        protected Label AvailableTypesLabel;
        protected DropdownField AvailableTypes;

        // Value
        private VisualElement _valueGroup;
        private Label _valueDescription;
        private TextField _valueField;
        private Button _valueSearch;

        // Operator
        protected VisualElement OperatorGroup;
        protected DropdownField Operator;

        // ID
        private VisualElement _idGroup;
        private Label _idDescription;
        private TextField _idType;
        private Button _idSearch;
        protected SerializedProperty CurrentProperty;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();

            CurrentProperty = property;
            Guid = CurrentProperty.FindPropertyRelative("guid").stringValue;
            if (Guid.StartsWith("000000"))
            {
                Guid = GUID.Generate().ToString();
                CurrentProperty.FindPropertyRelative("guid").stringValue = Guid;
            }

            SetupElements();
            SetupCallbacks();
            return _ve;
        }

        private void SetupElements()
        {
            Description = GetVisualElement("Description") as Label;
            AvailableTypes = GetVisualElement("DropDown") as DropdownField;
            AvailableTypesLabel = GetVisualElement("TypeLabel") as Label;
            // Value
            _valueGroup = GetVisualElement("ValueColumn");
            _valueDescription = GetVisualElement("ValueDescription") as Label;
            _valueSearch = GetVisualElement("ValueSearch") as Button;
            _valueField = GetVisualElement("ValueField") as TextField;

            // Operator
            OperatorGroup = GetVisualElement("OperatorColumn");
            Operator = GetVisualElement("OperatorDropDown") as DropdownField;

            // ID
            _idGroup = GetVisualElement("IdColumn");
            _idDescription = GetVisualElement("idDescription") as Label;
            _idType = GetVisualElement("IDField") as TextField;
            _idSearch = GetVisualElement("idSearch") as Button;
        }

        protected virtual void SetupCallbacks()
        {
        }

        protected void ConfigureID()
        {
            ShowVisualElement(_idGroup, false);

            if (string.IsNullOrEmpty(ConfigMap.idType)) return;
            ShowVisualElement(_idGroup, true);
            _idDescription.text = ConfigMap.idDescription;
            _idType.tooltip = ConfigMap.idToolTip;

            if (!ConfigMap.supportOperator) return;
            ShowVisualElement(OperatorGroup, true);

            // Hide the search bar until we have something to search for.
            ShowVisualElement(_idSearch, false);
            var searchProvider = ConfigureSearch(ConfigMap.idType, ConfigMap.IDOptions);
            if (searchProvider == null) return;

            if (string.IsNullOrEmpty(_idType.value))
            {
                if (searchProvider.listItems.Count > 0)
                {
                    var firstEntry = searchProvider.listItems[0];
                    _idType.value = firstEntry;
                }
            }

            ShowVisualElement(_idSearch, true);
            searchProvider.so = CurrentProperty.FindPropertyRelative("Id");
            _idSearch.clickable = new Clickable(_ => { OpenSearchProvider(searchProvider); });
            
            
        }

        private void OpenSearchProvider(StringListSearchProvider searchProvider)
        {
            SearchWindow.Open(
                new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                searchProvider);
        }

        protected void ConfigureValue()
        {
            ShowVisualElement(_valueGroup, false);
            if (string.IsNullOrEmpty(ConfigMap.valueType)) return;

            // Display and update the value information.
            ShowVisualElement(_valueGroup, true);
            _valueDescription.text = ConfigMap.valueDescription;
            _valueField.tooltip = ConfigMap.valueToolTip;

            // Hide the search bar until we know we have something to search for.
            ShowVisualElement(_valueSearch, false);

            if (!ConfigMap.supportOperator) return;
            ShowVisualElement(OperatorGroup, true);
            
            var searchProvider = ConfigureSearch(ConfigMap.valueType, ConfigMap.ValueOptions);
            if (searchProvider == null) return;

            // If there's no value, set it as the first entry in the list
            if (string.IsNullOrEmpty(_valueField.value))
            {
                if (searchProvider.listItems.Count > 0)
                {
                    var firstEntry = searchProvider.listItems[0];
                    _valueField.value = firstEntry;
                }
            }

            ShowVisualElement(_valueSearch, true);

            // The "Value" in this is the DialogAction / RequirementBase.Value field. This will update it..
            searchProvider.so = CurrentProperty.FindPropertyRelative("Value");
            _valueSearch.clickable = new Clickable(_ => { OpenSearchProvider(searchProvider); });

  
        }

        private StringListSearchProvider ConfigureSearch(string searchType, List<string> altList = null)
        {
            if (string.IsNullOrEmpty(searchType)) return null;

         
            // // If we have built in references, then show the find button and prepare the search window.
            var searchlist = ConfigurationManager.Instance.GetGameInfo(searchType);
            if (searchlist?.Count <= 0)
                searchlist = altList;

            if (searchlist?.Count <= 0)
            {
                if (searchType != "cvar") return null;
                
                var currentGraph = DialogueEditor.GetCurrentGraphView();
                if (currentGraph == null) return null;
                var rootNode = currentGraph.DialogGraph.rootNode as RootNode;
                if (rootNode == null) return null;
                var list = new List<string>();
                foreach (var cvar in rootNode.dialogCVarsList) 
                     list.Add(cvar.cVarName);
                searchlist = list;
            }

            var searchProvider = ScriptableObject.CreateInstance<StringListSearchProvider>();
            searchProvider.listItems = searchlist;
            searchProvider.so = CurrentProperty;
            return searchProvider;
        }

        protected void ShowVisualElement(VisualElement visualElement, bool state)
        {
            if (visualElement == null) return;
            visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private VisualElement GetVisualElement(string controlName)
        {
            return _ve.Q<VisualElement>(controlName);
        }
    }
}