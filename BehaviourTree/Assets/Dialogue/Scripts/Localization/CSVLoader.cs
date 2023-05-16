using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Dialogue.Editor;
using UnityEngine;

namespace Assets.Scripts.Localization
{
    public class CsvLoader
    {
        private string _fileContents;
        private const char LineSeperator = '\n';
        private const char Surround = '"';
        private readonly string[] _fieldSeperator = { "\", \"", "," };
        private Dictionary<string, string> _currentDictionary = new Dictionary<string, string>();
        public void Load(string path)
        {
            if (!File.Exists(path)) return;
            _fileContents = File.ReadAllText(path);
            _currentDictionary = GetDictionaryValues("english");

        }

        public Dictionary<string,string> GetDictionaryValues(string attributeId)
        {
            var dictionary = new Dictionary<string,string>();
            if (string.IsNullOrEmpty(_fileContents))
                return dictionary;
            var lines = _fileContents.Split(LineSeperator);

            var attributeIndex = -1;
            var headers = lines[0].Split(_fieldSeperator, StringSplitOptions.None);
            for(var i = 0; i < headers.Length; i++)
            {
                var header = headers[i].TrimEnd();
                if (!header.Equals(attributeId, StringComparison.OrdinalIgnoreCase)) continue;
                attributeIndex = i;
                break;
            }

            var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            for(var i = 1; i<lines.Length; i++)
            {
                var line = lines[i];
                var fields = csvParser.Split(line);
                for(var f =0; f< fields.Length; f++)
                {
                    fields[f] = fields[f].TrimStart(' ', Surround);
                    fields[f] = fields[f].TrimEnd(Surround);
                }

                if (fields.Length <= attributeIndex) continue;
                var key = fields[0];
                if (dictionary.ContainsKey(key)) continue;
                try
                {
                    var value = fields[attributeIndex];
                    dictionary.Add(key, value);

                }
                catch (Exception e)
                {
                    Debug.Log($"Error Parsing {key} {attributeIndex}: {e}");
                }
            }

            _currentDictionary = dictionary;
            return dictionary;
        }

        public string Get(string key)
        {
            _currentDictionary.TryGetValue(key, out var value);
            if (!string.IsNullOrEmpty(value)) return string.IsNullOrEmpty(value) ? key : value;
            
            // If its not in the current dictionary, try searching in the global one.
            if (ConfigurationManager.LanguageEn.Count == 0)
                return key;
            ConfigurationManager.LanguageEn.TryGetValue(key, out value);
            if (string.IsNullOrEmpty(value))
                return key;
            return string.IsNullOrEmpty(value) ? key : value;
        }
    }
}
