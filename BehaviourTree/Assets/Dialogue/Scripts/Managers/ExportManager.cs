using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Dialogue.GameData.Dialogs;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEngine;

namespace Dialogue.Editor
{
    public class ExportManager
    {
        private bool _localizeText = true;
        private string _localizationPrefix = "";
        private readonly Dictionary<string, string> _localization = new Dictionary<string, string>();

        public void Init(DialogGraph dialogGraph)
        {
            var dialog = new Dialog();
            var rootNode = dialogGraph.rootNode as RootNode;
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

            _localizeText = rootNode.createLocalization;
            _localization.Clear();

            Debug.Log($"Generating {dialog.Id}...");
            foreach (var node in dialogGraph.nodes)
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

            var serializer = new XmlSerializer(typeof(Dialogs));

            var dialogs = new Dialogs();
            dialogs.Dialog.Add(dialog);

            var exportTarget = Path.Combine("Assets", "Exports", "Dialogue",dialog.Id);
            if (!Directory.Exists(exportTarget))
                Directory.CreateDirectory(exportTarget);

            var outputFile = Path.Combine(exportTarget, dialog.Id + ".xml");
            if (File.Exists(outputFile))
            {
                if (!EditorUtility.DisplayDialog("Overwrite existing file?", $"Overwrite {outputFile}?", "Yes", "No"))
                {
                    Debug.Log("Cancelling export.");
                    return;
                }
            }

            if (_localizeText)
            {
                var localizationFile = Path.Combine(exportTarget, "Localization.txt");
                if ( File.Exists(localizationFile))
                    File.Delete(localizationFile);
                File.AppendAllText(localizationFile, $"Key,english,{Environment.NewLine}");
                foreach (var kvp in _localization)
                {
                    File.AppendAllText(Path.Combine(exportTarget, "Localization.txt"),
                        $"{kvp.Key}, {Quotify(kvp.Value)} {Environment.NewLine}");
                }

                AssetDatabase.ImportAsset(localizationFile);
            }

            using var writer = new StreamWriter(outputFile);
            Debug.Log($"Writing {outputFile}...");
            serializer.Serialize(writer, dialogs);
            Debug.Log($"Done with export.");
            AssetDatabase.ImportAsset(outputFile);
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

            foreach (var responseNode in statementNode.Children)
            {
                if (responseNode is not ResponseNode node) continue;
                var responseEntry = new DialogResponseEntry
                {
                    Id = node.id,
                    Ref_text = node.responseText
                };
                statement.responseEntry.Add(responseEntry);
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
            if (_localizeText)
            {
                var key = AddToLocalization(response.Id, responseNode.responseText);
                response.Text = key;
            }

            if (string.IsNullOrEmpty(response.Text))
                response.Text = response.RefText;

            foreach (var subnode in responseNode.Children)
            {
                switch (subnode)
                {
                    case StatementNode statementNode:
                        response.Nextstatementid = statementNode.id;
                        break;
                    case ActionNode actionNode:
                        var actions = CleanUpEmptyValues(actionNode.actions);
                        response.Actions.AddRange(actions);
                        break;
                    case GotoNode gotoNode:
                        response.Nextstatementid = gotoNode.statementId;
                        break;
                }
            }
            response.Requirement.AddRange(responseNode.requirements);
            return response;
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
        
    }
}