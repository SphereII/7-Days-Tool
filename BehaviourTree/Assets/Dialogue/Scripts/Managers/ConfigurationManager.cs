using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Assets.Scripts.Localization;
using Dialogue.GameData;
using UnityEngine;

namespace Dialogue.Editor
{
    public partial class ConfigurationManager
    {
        public static Dictionary<string, ConfigMap> Actions = new Dictionary<string, ConfigMap>();
        public static Dictionary<string, ConfigMap> Requirements = new Dictionary<string, ConfigMap>();
        private static Dictionary<string, List<string>> _gameInfo;
        public static Dictionary<string, string> LanguageEn = new Dictionary<string, string>();
        private static ConfigurationManager _instance;
        private static List<DialogGraph> _history = new List<DialogGraph>();

        public List<DialogGraph> GetHistory()
        {
            return _history;
        }

        public void AddHistory(DialogGraph graph)
        {
            if (_history.Contains(graph)) return;
            _history.Add(graph);
        }
        public List<string> Operators = new List<string>() {"None", "lt", "lte", "eq", "neq", "gte", "gt"};
        public List<string> ActionOperator = new List<string>() {"add", "sub", "set"};

        public static string GetLocalisedValue(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            if (LanguageEn == null) return key;
            LanguageEn.TryGetValue(key, out var value);
            return value ?? key;
        }

        public static ConfigurationManager Instance
        {
            get { return _instance ??= new ConfigurationManager(); }
        }

        // Grabs loaded game information from the various dictionaries. 
        public List<string> GetGameInfo(string type)
        {
            if (_gameInfo == null)
            {
                InitGameXmLs();
            }

            if (_gameInfo == null)
            {
                Debug.Log("Game Info is still null.");
                return null;
            }

            foreach (var entry in _gameInfo)
            {
                if (entry.Key == type)
                    return entry.Value;
            }

            return new List<string>();
        }

        // Grabs the configuration map for a particular action, which allows us to hide and display controls, and update
        // text on the UI.
        public ConfigMap GetActionMapByClass(string className)
        {
            if (Actions == null)
                Init();
            if (Actions == null)
            {
                Debug.Log("No Actions loaded. Try to Reload Config.");
                return null;
            }

            var map = new ConfigMap();
            foreach (var action in Actions)
            {
                if (action.Value.className == className)
                    return action.Value;
            }

            return map;
        }

        public ConfigMap GetRequirementMapByClass(string className)
        {
            if (Requirements == null)
                Init();
            if (Requirements == null)
            {
                Debug.Log("No Requirements loaded. Try to Reload Config.");
                return null;
            }

            var map = new ConfigMap();
            foreach (var requirement in Requirements)
            {
                if (requirement.Value.className == className)
                    return requirement.Value;
            }

            return map;
        }


        private static ConfigMap GenerateConfigMap(XmlElement node)
        {
            var map = new ConfigMap
            {
                type = ParseAttribute(node, "name")
            };

            foreach (XmlElement child in node.ChildNodes)
            {
                string name;
                if (child.HasAttribute("name"))
                {
                    name = child.GetAttribute("name");
                }
                else
                {
                    continue;
                }

                switch (name)
                {
                    case "class":
                        map.className = ParseAttribute(child, "value");
                        break;
                    case "description":
                        map.description = ParseAttribute(child, "value");
                        break;
                    case "valueDescription":
                        map.valueDescription = ParseAttribute(child, "value");
                        break;
                    case "valueType":
                        map.valueType = ParseAttribute(child, "value");
                        break;
                    case "valueToolTip":
                        map.valueToolTip = ParseAttribute(child, "value");
                        break;
                    case "valueOptions":
                        map.ValueOptions = ParseAttribute(child, "value").Split(',').ToList();
                        break;
                    case "idDescription":
                        map.idDescription = ParseAttribute(child, "value");
                        break;
                    case "idType":
                        map.idType = ParseAttribute(child, "value");
                        break;
                    case "idToolTip":
                        map.idToolTip = ParseAttribute(child, "value");
                        break;
                    case "idOptions":
                        var temp = ParseAttribute(child, "value");
                        map.IDOptions = temp.Split(',').ToList();
                        break;
                    case "supportOperator":
                        map.supportOperator = false;
                        var value = ParseAttribute(child, "value");
                        if (value.ToLower() == "true")
                            map.supportOperator = true;
                        break;
                }
            }

            return map;
        }

        private static void InitGameXmLs()
        {
            _gameInfo = new Dictionary<string, List<string>>();

            var targetPath = Path.Combine("Assets", "GameXMLs");
            if (!Directory.Exists(targetPath))
            {
                Debug.Log($"Missing {targetPath}");
                return;
            }

            foreach (var file in Directory.GetFiles(targetPath, "*.xml", SearchOption.AllDirectories))
            {
                Debug.Log($"Reading {file}...");
                var fileContents = File.ReadAllText(file);
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(fileContents);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error reading {file}: {ex}");
                    continue;
                }

                if (file.EndsWith("items.xml"))
                {
                    var nodes = doc.SelectNodes("//item");
                    ParseGameInfo("item", nodes);
                }
                else if (file.EndsWith("buffs.xml"))
                {
                    var nodes = doc.SelectNodes("//buff");
                    ParseGameInfo("buff", nodes);
                }
                else if (file.EndsWith("quests.xml"))
                {
                    var nodes = doc.SelectNodes("//quest");
                    ParseGameInfo("quest", nodes);
                }
            }


            var list = ((DefaultDialogEnums.FactionStance[]) Enum.GetValues(typeof(DefaultDialogEnums.FactionStance)))
                .Select(c => c.ToString()).ToList();
            _gameInfo.TryAdd("FactionStance", list);

            list = ((DefaultDialogEnums.DialogOperator[]) Enum.GetValues(typeof(DefaultDialogEnums.DialogOperator)))
                .Select(c => c.ToString()).ToList();
            _gameInfo.TryAdd("DialogOperator", list);

            list = ((DefaultDialogEnums.ModifiyCVar[]) Enum.GetValues(typeof(DefaultDialogEnums.ModifiyCVar)))
                .Select(c => c.ToString()).ToList();
            _gameInfo.TryAdd("ModifiyCVar", list);


            list = ((DefaultDialogEnums.QuestStatus[]) Enum.GetValues(typeof(DefaultDialogEnums.QuestStatus)))
                .Select(c => c.ToString()).ToList();
            _gameInfo.TryAdd("queststatus", list);

            // Localization
            var localization = Path.Combine(targetPath, "Localization.txt");
            if (File.Exists(localization))
            {
                Debug.Log($"Loading Localization: {localization}");
                var csvLoader = new CsvLoader();
                csvLoader.Load(localization);
                LanguageEn = csvLoader.GetDictionaryValues(DefaultDialogEnums.Language.English.ToString());
            }

            foreach (var info in _gameInfo)
            {
                Debug.Log($"Loaded {info.Key} : {info.Value.Count}");
            }
        }

        public void Init(bool force = true)
        {
            if (force == false)
            {
                if (Actions?.Count > 0 && Requirements?.Count > 0)
                {
                    Debug.Log("Already Initialized Configuration. Skipping...");
                    return;
                }
            }

            Actions = new Dictionary<string, ConfigMap>();
            Requirements = new Dictionary<string, ConfigMap>();

            var targetPath = Path.Combine("Assets", "Dialogue", "Configuration");
            if (!Directory.Exists(targetPath))
            {
                Debug.Log($"Missing {targetPath}");
                return;
            }

            foreach (var file in Directory.GetFiles(targetPath, "*.xml", SearchOption.AllDirectories))
            {
                Debug.Log($"Reading {targetPath}...");
                var fileContents = File.ReadAllText(file);
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(fileContents);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error reading {file}: {ex}");
                    continue;
                }

                var nodes = doc.SelectNodes("//action");
                ParseActions(nodes);
                nodes = doc.SelectNodes("//requirement");
                ParseRequirements(nodes);
            }

            Debug.Log($"Loaded {Actions.Count} Actions.");
            Debug.Log($"Loaded {Requirements.Count} Requirements.");
            InitGameXmLs();
        }


        private static string ParseAttribute(XmlElement element, string attribute)
        {
            return !element.HasAttribute(attribute) ? "None" : element.GetAttribute(attribute);
        }

        private static void ParseGameInfo(string key, XmlNodeList nodes)
        {
            if (nodes == null) return;
            var list = new List<string>();
            foreach (XmlElement node in nodes)
            {
                // If there's no name, use the id.
                var name = ParseAttribute(node, "name");
                if (name == "None") // Looking at you, quests.
                    name = ParseAttribute(node, "id");
                list.Add(name);
            }

            _gameInfo.TryAdd(key, list);
        }

        private void ParseActions(XmlNodeList nodes)
        {
            if (nodes == null) return;
            foreach (XmlElement node in nodes)
            {
                var map = GenerateConfigMap(node);
                Actions.TryAdd(map.type, map);
            }
        }

        private void ParseRequirements(XmlNodeList nodes)
        {
            if (nodes == null) return;
            foreach (XmlElement node in nodes)
            {
                var map = GenerateConfigMap(node);
                Requirements.TryAdd(map.type, map);
            }
        }
    }
}