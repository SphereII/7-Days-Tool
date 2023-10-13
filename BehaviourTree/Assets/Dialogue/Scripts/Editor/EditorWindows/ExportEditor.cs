using System.Collections.Generic;
using Dialogue.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Scripts.Editor.EditorWindows
{
    public class ExportEditor : EditorWindow
    {
        private const string Uxml = "Assets/Dialogue/UI/ExportManagerView.uxml";
        private VisualElement _ve;

        private TextField _dialogId;
        
        
        private TextField _localizationPrefix;
        private Toggle _createLocalization;

        private Toggle _createSounds;
        private Toggle _createActions;
        private Toggle _createVoices;

        private Toggle _xvaSynth;
        private TextField _gameID;
        private TextField _voiceID;
        
        private Button _export;
        private Button _close;

        private DialogueGraphView _graphView;
        private RootNode _rootNode;
        public void CreateGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            _graphView = DialogueEditor.GetCurrentGraphView();
            if (_graphView == null)
            {
                Debug.Log("no valid graph loaded.");
                Close();
                return;
            }
            

            _rootNode = _graphView.DialogGraph.rootNode as RootNode;
            if (_rootNode == null)
            {
                Debug.Log("no RootNode!");
                Close();
                return;
            }
            
            _dialogId = _ve.Q<TextField>("DialogID");
            _dialogId.value = _rootNode.dialogId;

            // Localization
            _localizationPrefix = _ve.Q<TextField>("LocalizationPrefix");
            if (string.IsNullOrEmpty(_rootNode.localizationPrefix))
                _rootNode.localizationPrefix = _rootNode.dialogId;
            _localizationPrefix.value = _rootNode.localizationPrefix;
            
            _createLocalization = _ve.Q<Toggle>("LocalizationToggle");

            // Sounds / Audio Settings
            _createSounds = _ve.Q<Toggle>("SoundsXMLToggle");
            _createSounds.value = _rootNode.createSounds;

            _createActions = _ve.Q<Toggle>("GenerateActions");
            _createActions.value = _rootNode.createSoundActions;

            _createVoices = _ve.Q<Toggle>("CreateVoices");
            _createActions.value = _rootNode.createVoices;
            
            // xvaSynth
            _xvaSynth = _ve.Q<Toggle>("TogglexVASynth");
            _gameID = _ve.Q<TextField>("TextGameID");
            _voiceID = _ve.Q<TextField>("TextVoiceID");
            _export = _ve.Q<Button>("ExportButton");
            _close = _ve.Q<Button>("Close");

            SetupControls();
            rootVisualElement.Add(_ve);
        }

        private void UpdateLocalizationPrefix(ChangeEvent<string> evt)
        {
            _rootNode.localizationPrefix = evt.newValue;
        }

        private void UpdateDialogid(ChangeEvent<string> evt)
        {
            _rootNode.dialogId = evt.newValue;
        }

        private void SetupControls()
        {
            _dialogId.RegisterValueChangedCallback(UpdateDialogid);
            _localizationPrefix.RegisterValueChangedCallback(UpdateLocalizationPrefix);

            _export.RegisterCallback<ClickEvent>(_ => Export());

            _close.RegisterCallback<ClickEvent>(_ => Close());
        }

        private void Export()
        {
            var exportManager = new ExportManager();
            _rootNode.createLocalization = _createLocalization.value;
            _rootNode.createSounds = _createSounds.value;
            _rootNode.createVoices = _createVoices.value;
            _rootNode.createSoundActions = _createActions.value;
            
            exportManager.Init(_graphView.DialogGraph);
            exportManager.ExportDialog(_rootNode.createSoundActions);
            if (_rootNode.createLocalization)
            {
                exportManager.ExportLocalization();
            }

            if (_rootNode.createSounds)
            {
                exportManager.ExportSounds();
            }

            if (_rootNode.createVoices)
            {
                var gameID = "";
                var voiceID = "";
                if (_xvaSynth.value)
                {
                    gameID = _gameID.text;
                    voiceID = _voiceID.text;
                }
                exportManager.ExportVoices(gameID, voiceID);
            }
            
            Close();
        }
    }
}