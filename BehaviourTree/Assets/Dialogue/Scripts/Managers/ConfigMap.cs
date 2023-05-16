using System.Collections.Generic;

namespace Dialogue.Editor
{
    public partial class ConfigurationManager
    {
        // The config Map allows us to read from the various configure files ( 0-Vanilla.xml, 1-SCore.xml ) to load
        // the Dialogue Graph system with actions and requirements
        public class ConfigMap
        {
            public string type;
            public string className;
            public string description = "";

            public string valueToolTip;
            public string valueDescription = "";
            public string valueType = "";
            public List<string> ValueOptions = new List<string>();

            public bool supportOperator = false;

            public string idToolTip;
            public string idDescription = "";
            public string idType = "";
            public List<string> IDOptions = new List<string>();
        }
    }
}