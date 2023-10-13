using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Serialization;
using Dialogue.GameData.Dialogs;
using Dialogue.Scripts.GameClasses.ExportsOnly;
using Dialogue.Scripts.GameClasses.Sounds;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;

namespace Dialogue.Editor
{
    public class ExportManager
    {
        private bool _localizeText = true;
        private string _localizationPrefix = "";
        private readonly Dictionary<string, string> _localization = new Dictionary<string, string>();
        private List<DialogGraph> _importedDialogs = new List<DialogGraph>();
        private string _exportTarget = "";
        private DialogGraph _dialogGraph;
        private bool _createAudioActions = false;

        public void Init(DialogGraph dialogGraph)
        {
            _dialogGraph = dialogGraph;
        }

        public void ExportDialog(bool createAudioActions = false)
        {
            var dialog = new Dialog();
            _createAudioActions = createAudioActions;

            _importedDialogs.Clear();
            var rootNode = _dialogGraph.rootNode as RootNode;
            if (rootNode == null)
            {
                EditorUtility.DisplayDialog("Missing Root Node",
                    "You do not have a root node defined. Cancelling export.", "ok");
                Debug.Log("No root node?! Export cancelled.");
                return;
            }

            var startStatement = rootNode.child as StatementNode;
            if (startStatement == null)
            {
                EditorUtility.DisplayDialog("Missing Starting Statement Node",
                    "You do not have a statement node off your root node.. Cancelling export.", "ok");
                Debug.Log("No statement off the root node. Export Cancelled.");
                return;
            }

            dialog.Id = rootNode.dialogId;
            dialog.Startstatementid = startStatement.id;

            _localizationPrefix = rootNode.localizationPrefix;
            if (string.IsNullOrEmpty(_localizationPrefix))
                _localizationPrefix = rootNode.dialogId;
            _localization.Clear();

            Debug.Log($"Generating {dialog.Id}...");
            ParseDialog(_dialogGraph, dialog);
            ParseImportedDialog(dialog);

            var dialogs = new DialogsExport();
            dialogs.append.Dialog.Add(dialog);
            
            _exportTarget = Path.Combine("Assets", "Exports", "Dialogue", dialog.Id);

            var path = EditorUtility.SaveFilePanel(
                "File To...",
                $"",
                "dialogs.xml",
                "xml");

            if (path.Length == 0)
            {
                return;
            }

            _exportTarget = Path.GetDirectoryName(path);

            var outputFile = path;
            
            // if (File.Exists(outputFile))
            // {
            //     if (!EditorUtility.DisplayDialog("Overwrite existing file?", $"Overwrite {outputFile}?", "Yes", "No"))
            //     {
            //         Debug.Log("Cancelling export.");
            //         return;
            //     }
            // }
            
            using var writer = new StreamWriter(outputFile);
            Debug.Log($"Writing {outputFile}...");
            var serializer = new XmlSerializer(typeof(DialogsExport));

            serializer.Serialize(writer, dialogs);
            Debug.Log($"Done with export.");
            AssetDatabase.ImportAsset(outputFile);
        }

        public void ExportLocalization()
        {
            var localizationFile = Path.Combine(_exportTarget, "Localization.txt");
            if (File.Exists(localizationFile))
                File.Delete(localizationFile);

            File.AppendAllText(localizationFile, $"Key,english{Environment.NewLine}");
            foreach (var kvp in _localization)
            {
                File.AppendAllText(Path.Combine(_exportTarget, "Localization.txt"),
                    $"{kvp.Key}, {Quotify(kvp.Value)} {Environment.NewLine}");
            }

            AssetDatabase.ImportAsset(localizationFile);
        }

     
        private void ParseImportedDialog(Dialog dialog)
        {
            for (var x = 0; x < _importedDialogs.Count; x++)
            {
                var importedDialog = _importedDialogs[x];
                Debug.Log($"Importing DialogGraph: {importedDialog.name}");
                ParseDialog(importedDialog, dialog);
            }
        }

        private void ParseDialog(DialogGraph import, Dialog dialog)
        {
            foreach (var node in import.nodes)
            {
                switch (node)
                {
                    case StatementNode statementNode:
                        dialog.Statement.Add(ExportStatements(statementNode));
                        break;
                    case ResponseNode responseNode:
                        dialog.Response.Add(ExportResponse(responseNode));
                        break;
                }
            }
        }

        private void ResetStatement(ref DialogGraph dialogGraph, string oldStatementID, string newStatementID)
        {
            BaseNode statementToBeRemoved = null;
            foreach (var node in dialogGraph.nodes)
            {
                switch (node)
                {
                    case StatementNode statementNode:
                    {
                        // If this is the actual statement, we want to remove it compeletely.
                        if (statementNode.id == oldStatementID)
                        {
                            Debug.Log($"Removing old {statementNode.id}");
                            statementToBeRemoved = node;
                        }

                        if (statementNode.nextstatementId == oldStatementID)
                        {
                            Debug.Log($"Converting statement to new statement: {statementNode.id}");
                            statementNode.nextstatementId = newStatementID;
                        }

                        break;
                    }
                    case ResponseNode responseNode:
                    {
                        if (responseNode.nextstatementId == oldStatementID)
                        {
                            Debug.Log($"Converting Responses next statement: {responseNode.id}");
                            responseNode.nextstatementId = newStatementID;
                        }

                        break;
                    }
                }
            }

            if (statementToBeRemoved == null) return;
            dialogGraph.nodes.Remove(statementToBeRemoved);
        }

        private DialogStatement ExportStatements(StatementNode statementNode)
        {
            var statement = new DialogStatement
            {
                id = statementNode.id,
                RefText = statementNode.statementText
            };
            if (_localizeText)
            {
                var key = AddToLocalization(statement.id, statementNode.statementText);
                statement.text = key;
            }

            if (string.IsNullOrEmpty(statement.text))
                statement.text = statement.RefText;

            statement.Nextstatementid = statementNode.nextstatementId;
            if (statementNode.questEntry?.Count > 0)
            {
                statement.questEntry = new List<DialogQuestEntry>();
                var entries = CleanUpEmptyValues(statementNode.questEntry);
                statement.questEntry.AddRange(entries);
            }

            foreach (var responseNode in statementNode.Children)
            {
                switch (responseNode)
                {
                    case ActionNode actionNode:
                        if (actionNode.actions == null) break;
                        var actions = CleanUpEmptyValues(actionNode.actions);
                        statement.Actions.AddRange(actions);
                        break;
                    case ResponseNode node:
                    {
                        var responseEntry = new DialogResponseEntry
                        {
                            Id = node.id,
                            Ref_text = node.responseText
                        };
                        
                        // This is a bit of an ugly cludge for the responses entry for the quests.
                        if (responseEntry.Id.StartsWith("jobsprev"))
                            responseEntry.uniqueid = "prev";
                        else if (responseEntry.Id.StartsWith("jobsnext"))
                            responseEntry.uniqueid = "next";
                        
                       // if (statement.id == "currentjobsspecial")

                        statement.responseEntry.Add(responseEntry);
                        break;
                    }
                    case ImportNode importNode:
                    {
                        var graph = importNode.importedDialog;
                        if (graph == null) continue;

                        // No children, no merge.
                        var children = importNode.GetChildren();
                        if (children.Count == 0) continue;

                        // Re-assign the old statement IDs to the new one, since the old start statement will be removed.
                        var newStatementID = statement.id;
                        var oldStatementID = importNode.GetOriginalStatementID();
                        if (string.IsNullOrEmpty(oldStatementID)) continue;

                        var cloneDialog = importNode.CloneDialogGraph();
                        ResetStatement(ref cloneDialog, oldStatementID, newStatementID);

                        // Add to the imported graph later.
                        _importedDialogs.Add(cloneDialog);

                        // But we want to add the responses to this statement.
                        foreach (var newChild in children)
                        {
                            var newresponse = newChild as ResponseNode;
                            if (newresponse == null) continue;

                            // If the next statement is still referring to the old dialog, change it here.
                            if (newresponse.nextstatementId == oldStatementID)
                                newresponse.nextstatementId = newStatementID;
                            var responseEntry = new DialogResponseEntry
                            {
                                Id = newresponse.id,
                                Ref_text = newresponse.responseText
                            };
                            statement.responseEntry.Add(responseEntry);
                        }

                        break;
                    }
                }
            }

            return statement;
        }

        private static string Quotify(string s)
        {
            return $"\"{s.Trim()}\"";
        }

        private string AddToLocalization(string id, string text)
        {
            var key = $"{_localizationPrefix}_{id}";
            _localization.TryAdd(key, text);
            return key;
        }

        private DialogResponse ExportResponse(ResponseNode responseNode)
        {
            var response = new DialogResponse
            {
                Id = responseNode.id,
                RefText = responseNode.responseText
            };

            Debug.Log($"Adding Response: {responseNode.responseText} Next Statement: {responseNode.nextstatementId}");
            if (_localizeText)
            {
                var key = AddToLocalization(response.Id, responseNode.responseText);
                response.Text = key;
            }
            
            if (string.IsNullOrEmpty(response.Text))
                response.Text = response.RefText;

            response.Nextstatementid = responseNode.nextstatementId;

            foreach (var subnode in responseNode.Children)
            {
                switch (subnode)
                {
                    case StatementNode statementNode:
                        response.Nextstatementid = statementNode.id;
                        if (_createAudioActions)
                        {
                            var rootNode = _dialogGraph.rootNode as RootNode;
                            if (rootNode == null) break;
                            var action = new DialogAction();
                            action.type = "PlaySoundSDX, SCore";
                            action.Id = $"{rootNode.dialogId}_{statementNode.id}";
                            response.Actions.Add(action);
                        }

                        break;
                    case ActionNode actionNode:
                        if (actionNode.actions == null) break;
                        var actions = CleanUpEmptyValues(actionNode.actions);
                        response.Actions.AddRange(actions);
                        break;
                    case GotoNode gotoNode:
                        response.Nextstatementid = gotoNode.statementId;
                        break;
                }
            }

          

            var cleanRequirements = CleanUpEmptyValues(responseNode.requirements);
            response.Requirement.AddRange(cleanRequirements);
            return response;
        }

        private List<RequirementBase> CleanUpEmptyValues(List<RequirementBase> requirements)
        {
            foreach (var requirement in requirements)
            {
                if (string.IsNullOrEmpty(requirement.Id))
                    requirement.Id = null;
                if (string.IsNullOrEmpty(requirement.Value))
                    requirement.Value = null;
                if (string.IsNullOrEmpty(requirement.op))
                    requirement.op = null;
                requirement.Requirementtype = "Hide";
            }

            return requirements;
        }

        private List<DialogQuestEntry> CleanUpEmptyValues(List<DialogQuestEntry> questEntries)
        {
            foreach (var entry in questEntries)
            {
                if (string.IsNullOrEmpty(entry.tier))
                    entry.tier = null;
                if (string.IsNullOrEmpty(entry.type))
                    entry.type = null;
            }
            return questEntries;
        }
        private List<DialogAction> CleanUpEmptyValues(List<DialogAction> actions)
        {
            foreach (var action in actions)
            {
                if (string.IsNullOrEmpty(action.Id))
                    action.Id = null;
                if (string.IsNullOrEmpty(action.Value))
                    action.Value = null;
                if (string.IsNullOrEmpty(action.op))
                    action.op = null;
            }

            return actions;
        }

      

        public void ExportSounds()
        {
            Debug.Log("Generating sounds.xml...");

            var dialogId = (_dialogGraph.rootNode as RootNode)?.dialogId;

            var sounds = new Sounds();
            sounds.soundAppend = new AppendSound();

            foreach (var node in _dialogGraph.nodes)
            {
                if (node is not StatementNode statementNode) continue;

                Debug.Log("Generating Sound Data Node XML");
                var soundDataNode = new SoundDataNode();
                soundDataNode.Name = $"{dialogId}_{statementNode.id}";
                soundDataNode.AudioClip.ClipName = $"#@modfolder:Resources/{dialogId}.unity3d?{statementNode.id}";
                soundDataNode.AudioClip.Loop = false;
                soundDataNode.AudioSource.Name = "Sounds/AudioSource_Notifications";
                soundDataNode.CrouchNoiseScale.Value = 0.5;
                soundDataNode.NoiseScale.Value = 1;
                soundDataNode.MaxVoices.Value = 10;
                sounds.soundAppend.SoundDataNodes.Add(soundDataNode);

                /*
                var sourceFolder = Path.Combine(_exportTarget, "Unprocessed");
                if (!Directory.Exists(sourceFolder)) continue;

                var targetFolder = Path.Combine(_exportTarget, "Processed");
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                var targetFile = Path.Combine(targetFolder, statementNode.id + ".wav");
                if (File.Exists(targetFile)) continue;

                var targetSearch = statementNode.nodeView.GetCleanText(50);
                foreach (var file in Directory.GetFiles(sourceFolder))
                {
                    if (!file.Contains(targetSearch)) continue;
                    Debug.Log($"Copying {file} to {targetFile}...");
                    File.Copy(file, targetFile);
                    break;
                }
                */
            }

            var soundfile = Path.Combine(_exportTarget, "sounds.xml");
            var overrides = new XmlAttributeOverrides();
            var attribs = new XmlAttributes();
            attribs.XmlIgnore = true;
            attribs.XmlElements.Add(new XmlElementAttribute("SoundDataNodes"));
            overrides.Add(typeof(Sounds), "SoundDataNodes", attribs);
            
            using var writer = new StreamWriter(soundfile);
            Debug.Log($"Writing {soundfile}...");
            var serializer = new XmlSerializer(typeof(Sounds));

            serializer.Serialize(writer, sounds);
   //         AssetDatabase.ImportAsset(soundfile);
        }

        public void ExportVoices(string gameId= "", string voiceId ="")
        {
            Debug.Log("Generating voices.csv...");
            var voiceOutput = Path.Combine(_exportTarget, "voice.csv");
            if (File.Exists(voiceOutput))
                File.Delete(voiceOutput);

            var useGameId = false;
            if (!string.IsNullOrEmpty(gameId) && !string.IsNullOrEmpty(voiceId))
            {
                useGameId = true;
                File.AppendAllText(voiceOutput, $"game_id,voice_id,text {Environment.NewLine}");
            }

        
            foreach (var node in _dialogGraph.nodes)
            {
                if (node is not StatementNode statementNode) continue;
                if (useGameId)
                {
                    File.AppendAllText(voiceOutput,
                        $"{Quotify(gameId)},{Quotify(voiceId)},{Quotify(statementNode.statementText)} {Environment.NewLine}");
                }
                else
                {
                    File.AppendAllText(voiceOutput, $"{Quotify(statementNode.statementText)} {Environment.NewLine}");
                }
            }

//            AssetDatabase.ImportAsset(voiceOutput);
        }
    }
}