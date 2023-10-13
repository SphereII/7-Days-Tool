using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dialogue.Editor
{
    public static class MenuManager
    {
        [MenuItem("7 Days To Die/Dialogue/Reload Config")]
        public static void ReloadConfig()
        {
            var configurationManager = new ConfigurationManager();
            configurationManager.Init();
        }
   
        [MenuItem("7 Days To Die/Dialogue/Import")]
        public static void Import()
        {
            var importManager = new ImportManager();
            importManager.Init();
        }
    }
}