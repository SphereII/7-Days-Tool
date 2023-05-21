using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dialogue;
using Dialogue.Editor;
using Dialogue.Scripts.Editor.EditorWindows;
using Dialogue.Scripts.Nodes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using File = UnityEngine.Windows.File;

public class DialogueEditor : EditorWindow
{
    private MiniMap _miniMap;

    private static DialogueGraphView _graphView;

    private InspectorView _inspectorView;
    private ToolbarPopupSearchField _searchField;
    private ToolbarMenu _toolbarMenuNodes;
   
    private static void OpenWindow()
    {
        var wnd = GetWindow<DialogueEditor>();
        wnd.titleContent = new GUIContent("DialogueEditor");
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (Selection.activeObject is not DialogGraph) return false;
        OpenWindow();

        return true;
    }

    public static DialogueGraphView GetCurrentGraphView()
    {
        return _graphView;
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Dialogue/UI/DialogueEditor.uxml");
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialogue/UI/uss/DialogueEditor.uss");
        root.styleSheets.Add(styleSheet);

        _graphView = root.Q<DialogueGraphView>();
        _graphView.OnNodeSelected = OnNodeSelectionChanged;

        // This controls a potential side inspector in the graphview, but is not currently needed.
        _inspectorView = root.Q<InspectorView>();

        // _inspectorView.Q<Button>("Refresh").clickable = new Clickable(Refresh);
        // _inspectorView.Q<Button>("Close").clickable = new Clickable(Close);
        // _inspectorView.Q<Button>("Export").clickable = new Clickable(Export);
        // _inspectorView.Q<Button>("AdjustLayout").clickable = new Clickable(UpdateLayout);
        //   _graphView.OnNodeSelected += (_ => UpdateInspector());

        new ConfigurationManager().Init();
        OnSelectionChange();
        GenerateMiniMap();
        UpdateInspector();
        ConfigureToolbar();
    }

  
    private void ConfigureToolbar()
    {
        var toolbar = rootVisualElement.Q<Toolbar>();
        if (toolbar == null) return;

        var refreshButton = toolbar.Q<ToolbarButton>("mnuRefresh");
        refreshButton.RegisterCallback<ClickEvent>(_ => Refresh());
        var autoLayoutButton = toolbar.Q<ToolbarButton>("mnuAutoLayout");
        autoLayoutButton.RegisterCallback<ClickEvent>(_ => UpdateLayout());

        var exportMenu = toolbar.Q<ToolbarMenu>("ExportOptions");
        exportMenu.menu.AppendAction("Export", _ => Export(true), _ => DropdownMenuAction.Status.Normal);
        exportMenu.menu.AppendAction("Export No Localization", _ => Export(), _ => DropdownMenuAction.Status.Normal);
        //var exportButton = toolbar.Q<ToolbarButton>("mnuExport");
        //exportButton.RegisterCallback<ClickEvent>(_ => Export());
        var closeButton = toolbar.Q<ToolbarButton>("mnuClose");
        closeButton.RegisterCallback<ClickEvent>(_ => Close());

        _searchField = toolbar.Q<ToolbarPopupSearchField>("mnuSearchField");
        _searchField.RegisterValueChangedCallback(OnSearchTextChanged);

        _toolbarMenuNodes = toolbar.Q<ToolbarMenu>("mnuNodes");
        _toolbarMenuNodes.RegisterCallback<ClickEvent>(_ => ViewNodes());

        var doodle = toolbar.Q<ToolbarButton>("mnuQuickDoodle");
        doodle.RegisterCallback<ClickEvent>(_ => OpenQuickDoodle());

        var versionLabel = toolbar.Q<Label>("lblVersion");
        var versionFile = Path.Combine("Assets", "Dialogue", "version.txt");
        if (File.Exists(versionFile))
            versionLabel.text = System.IO.File.ReadAllText(versionFile);
    }

    private void OpenQuickDoodle()
    {
        var wnd = GetWindow<QuickDoodleEditor>();
        wnd.titleContent = new GUIContent("Scratch");
        wnd.ShowPopup();
    }


    private void ViewNodes()
    {
        foreach (var node in _graphView.DialogGraph.nodes)
        {
            var menuEntry = "";
            switch (node)
            {
                case RootNode:
                    menuEntry = "Root Node";
                    break;
                case StatementNode:
                    menuEntry = $"Statements/{node.nodeView.GetText(25)}";
                    break;
                case ResponseNode responseNode:
                    menuEntry = $"Responses/{responseNode.nodeView.GetText(25)}";
                    foreach (var requirement in responseNode.requirements)
                    {
                        var subMenu = $"Requirements/{requirement}";
                        _toolbarMenuNodes.menu.AppendAction(subMenu, _ => DropDownMenuGoTo(node.nodeView),
                            _ => DropdownMenuAction.Status.Normal);
                    }

                    break;
                case ActionNode actionNode:
                    foreach (var action in actionNode.actions)
                    {
                        var subMenu = $"Actions/{action}";
                        _toolbarMenuNodes.menu.AppendAction(subMenu, _ => DropDownMenuGoTo(node.nodeView),
                            _ => DropdownMenuAction.Status.Normal);
                    }

                    break;
                default:
                    continue;
            }

            if (string.IsNullOrEmpty(menuEntry)) continue;
            _toolbarMenuNodes.menu.AppendAction(menuEntry, _ => DropDownMenuGoTo(node.nodeView),
                _ => DropdownMenuAction.Status.Normal);
        }

        _toolbarMenuNodes.ShowMenu();
    }


    private void OnSearchTextChanged(ChangeEvent<string> evt)
    {
        if (string.IsNullOrEmpty(evt.newValue)) return;
        if (evt.newValue.Length < 3) return;

        foreach (var node in _graphView.DialogGraph.nodes)
        {
            var addItem = false;
            var nodeView = node.nodeView;
            var nodeText = nodeView.GetText();
            if (nodeText.Contains(evt.newValue))
                addItem = true;

            if (addItem)
            {
                _searchField.menu.AppendAction(nodeText, _ => DropDownMenuGoTo(nodeView),
                    _ => DropdownMenuAction.Status.Normal);
            }
        }

        _searchField.ShowMenu();
    }

    private static void DropDownMenuGoTo(NodeView nodeView)
    {
        _graphView.ClearSelection();
        _graphView.AddToSelection(nodeView);
        _graphView.FrameSelection();
    }

    // Re-distribute the nodes on the graph
    private static void UpdateLayout()
    {
        _graphView.UpdateLayout();
    }

    // Reload the graph from the serialized objects.
    private static void Refresh()
    {
        _graphView.PopulateView(_graphView.DialogGraph);
    }


    private static void Export(bool forceLocalization = false)
    {
        var exportManager = new ExportManager();
        exportManager.Init(_graphView.DialogGraph, forceLocalization);
    }

    private void UpdateInspector()
    {
      //  _inspectorView.UpdateInspector(cvars);
        // var list = _inspectorView.Q<ListView>("CVarList");
        // list.itemsSource = cvars;
        

    }
    // private void UpdateInspector()
    // {
    //     var statementFoldout = _inspectorView.Q<Foldout>("StatementFoldout");
    //     var responseFoldout = _inspectorView.Q<Foldout>("ResponseFoldout");
    //     if (statementFoldout == null) return;
    //     if (_graphView == null || _graphView.DialogGraph == null) return;
    //      
    //     
    //     statementFoldout.Clear();
    //     responseFoldout.Clear();
    //     if (_graphView.DialogGraph.rootNode != null)
    //     {
    //         var button = new Button
    //         {
    //             text = "Root Node",
    //             tooltip = "Sets focus on root node",
    //             clickable = new Clickable(() =>
    //                 {
    //                     _graphView.ClearSelection();
    //                     _graphView.AddToSelection(_graphView.DialogGraph.rootNode.nodeView);
    //                     _graphView.FrameSelection();
    //                 }
    //             )
    //         };
    //         statementFoldout.Add(button);
    //         
    //     }
    //
    //     foreach (var node in _graphView.DialogGraph.nodes)
    //     {
    //         var button = new Button
    //         {
    //             clickable = new Clickable(() =>
    //                 {
    //                     _graphView.ClearSelection();
    //                     _graphView.AddToSelection(node.nodeView);
    //                     _graphView.FrameSelection();
    //                 }
    //             )
    //         };
    //         switch (node)
    //         {
    //             case StatementNode statementNode:
    //                 button.text = statementNode.statementText;
    //                 button.tooltip = statementNode.statementText;
    //                 statementFoldout.Add(button);
    //                 break;
    //             case ResponseNode responseNode:
    //                 button.text = responseNode.responseText;
    //                 button.tooltip = responseNode.responseText;
    //
    //                 responseFoldout.Add(button);
    //                 break;
    //         }
    //         button.AddToClassList("button-left-align");
    //
    //     }
    //
    //    
    // }

    private void OnNodeSelectionChanged(NodeView nodeView)
    {
        // _inspectorView.UpdateSelection(nodeView);
    }

    private void OnGUI()
    {
        // Sometimes The mini map will move around, depending on the size of the window when its creating.
        if (Event.current.rawType != EventType.Layout) return;
        _miniMap?.SetPosition(new Rect(20, position.height - 200, 200, 140));
    }

    private void GenerateMiniMap()
    {
        _miniMap = new MiniMap
        {
            anchored = true
        };
        _miniMap?.SetPosition(new Rect(20, position.height - 200, 200, 140));
        _graphView.Add(_miniMap);
    }

    private void OnSelectionChange()
    {
        var tree = Selection.activeObject as DialogGraph;
        if (tree == null) return;
        if (!AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID())) return;
        _graphView.PopulateView(tree);
    }
}