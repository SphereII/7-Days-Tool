using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Assets.Scripts.Localization;
using Dialogue;
using Dialogue.Editor;
using Dialogue.GameData.Dialogs;
using UnityEditor;
using UnityEngine;

public class ImportManager
{
    public void Init()
    {
        var targetPath = Path.Combine("Assets", "Imports", "Dialogue");
        if (!Directory.Exists(targetPath))
        {
            Debug.Log($"Missing {targetPath}");
            return;
        }

        foreach (var file in Directory.GetFiles(targetPath, "*.xml", SearchOption.AllDirectories))
        {
            Debug.Log($"Reading {file}...");
            var fileContents = File.ReadAllText(file);
            // Check for missing root node
            if (fileContents.StartsWith("<dialog "))
            {
                fileContents = $"<dialogs>${fileContents}</dialogs>";
            }

            var doc = new XmlDocument()
            {
                PreserveWhitespace = true,
                XmlResolver = null
            };
            try
            {
                doc.LoadXml(fileContents);
            }
            catch (XmlException e)
            {
                Debug.Log($"Error parsing: {file}. Not a valid XML file. {e}");
                continue;
            }

            var nodes = doc.SelectNodes("//dialog");
            if (nodes == null)
            {
                Debug.Log($"No dialog found in {file}");
                continue;
            }


            // Check if there's a localization file for this dialog.xml
            var parentFolder = Path.GetDirectoryName(file);
            var csvLoader = new CsvLoader();
            if (parentFolder != null)
            {
                var localizationFile = Path.Combine(parentFolder, "Localization.txt");
                if (File.Exists(localizationFile))
                {
                    Debug.Log($"Loading Localization for dialog {file}...");
                    csvLoader.Load(localizationFile);
                }
            }


            var serializer = new XmlSerializer(typeof(Dialog));
            foreach (XmlNode node in nodes)
            {
                using var reader = new XmlNodeReader(node);
                var dialog = (Dialog) serializer.Deserialize(reader);
                GenerateDialog(dialog, csvLoader);
            }
        }
    }

    private void GenerateDialog(Dialog dialog, CsvLoader csvLoader)
    {
        var graph = ScriptableObject.CreateInstance<DialogGraph>();
        var cfg = new ConfigurationManager();
        cfg.Init();

        var output = Path.Combine("Assets", "Imported", "Dialogue");
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        var outputFile = Path.Combine(output, $"{dialog.Id}.asset");
        if (File.Exists(outputFile))
        {
            Debug.Log($"Skipping Asset: {outputFile} : Asset Exists.");
            return;
        }

        AssetDatabase.CreateAsset(graph, outputFile);

        foreach (var statement in dialog.Statement)
        {
            GenerateStatementNode(graph, statement, dialog, csvLoader);
        }

        foreach (var response in dialog.Response)
        {
            GenerateResponse(graph, response, csvLoader);
        }

        foreach (var node in graph.nodes)
        {
            switch (node)
            {
                case StatementNode statementNode:
                {
                    var nextStatement = FindStatementNode(graph, statementNode.nextstatementId);
                    if (nextStatement != null)
                        graph.AddChild(statementNode, nextStatement);
                    foreach (var responseEntry in statementNode.ResponseEntries)
                    {
                        var foundNode = FindResponseEntry(graph, responseEntry.Id);
                        if (foundNode == null) continue;
                        graph.AddChild(statementNode, foundNode);
                    }

                    break;
                }
                case ResponseNode responseNode:
                {
                    var nextStatement = FindStatementNode(graph, responseNode.nextstatementId);
                    if (nextStatement == null) continue;
                    graph.AddChild(responseNode, nextStatement);
                    break;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(outputFile);

        Debug.Log($"Done Saving: {outputFile}");
    }

    private ResponseNode FindResponseEntry(DialogGraph graph, string responseEntryId)
    {
        foreach (var node in graph.nodes)
        {
            if (node is ResponseNode responseNode)
            {
                if (responseNode.id == responseEntryId)
                    return responseNode;
            }
        }

        return null;
    }

    private StatementNode FindStatementNode(DialogGraph graph, string statementID)
    {
        foreach (var node in graph.nodes)
        {
            if (node is StatementNode statementNode)
            {
                if (statementNode.id == statementID)
                    return statementNode;
            }
        }

        return null;
    }

    private void GenerateStatementNode(DialogGraph graph, DialogStatement statement, Dialog dialog, CsvLoader csvLoader)
    {
        var statementNode = graph.CreateNode(typeof(StatementNode)) as StatementNode;
        if (statementNode == null) return;
        statementNode.id = statement.id;

        // Use the ref text first, if not available, check the localization, and finally fall back to just a plain text
        statementNode.statementText = statement.RefText;
        if (string.IsNullOrEmpty(statement.RefText))
        {
            statementNode.statementText = csvLoader.Get(statement.text);
        }

        if (string.IsNullOrEmpty(statementNode.statementText))
            statementNode.statementText = statement.text;

        statementNode.name = statement.id;
        statementNode.nextstatementId = statement.Nextstatementid;
        statementNode.ResponseEntries = statement.responseEntry;

        // Check if this is the start statement, as it'll be our root node.
        if (statement.id != dialog.Startstatementid) return;
        Debug.Log($"Adding Root Node: {statementNode.statementText}");
        var rootNode = graph.CreateNode(typeof(RootNode)) as RootNode;
        if (rootNode == null) return;

        rootNode.dialogId = dialog.Id;
        graph.rootNode = rootNode;
        graph.AddChild(rootNode, statementNode);
    }

    private void GenerateActionNode(DialogGraph graph, List<DialogAction> actions, ResponseNode response)
    {
        if (actions.Count == 0) return;

        var actionNode = graph.CreateNode(typeof(ActionNode)) as ActionNode;
        if (actionNode == null) return;
        foreach (var action in actions)
        {
            // Do we need to do any clean up before importing? No?
        }

        actionNode.actions.AddRange(actions);
        graph.AddChild(response, actionNode);
    }

    private void GenerateResponse(DialogGraph graph, DialogResponse dialogResponse, CsvLoader csvLoader)
    {
        var responseNode = graph.CreateNode(typeof(ResponseNode)) as ResponseNode;
        if (responseNode == null) return;
        responseNode.id = dialogResponse.Id;

        // Use the ref text first, if not available, check the localization, and finally fall back to just a plain text
        responseNode.responseText = dialogResponse.RefText;
        if (string.IsNullOrEmpty(dialogResponse.RefText))
        {
            responseNode.responseText = csvLoader.Get(dialogResponse.Text);
        }

        if (string.IsNullOrEmpty(responseNode.responseText))
            responseNode.responseText = dialogResponse.Text;

        responseNode.requirements = new List<RequirementBase>();

        responseNode.requirements.AddRange(dialogResponse.Requirement);
        responseNode.nextstatementId = dialogResponse.Nextstatementid;

        GenerateActionNode(graph, dialogResponse.Actions, responseNode);
    }
}