using UnityEngine;

namespace Dialogue.Nodes.CustomAttributes
{
    // Custom property attribute for us to set serialized fields to be read only in the inspector.
    // Example:
    // [ReadOnly] public string guid;
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}