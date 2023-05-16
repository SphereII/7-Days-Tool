using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Dialogue.GameData.Dialogs
{
    public enum StanceTypes
    {
        None,
        Like,
        Neutral,
        Dislike
    }
    
    [Serializable]
    [XmlRoot(ElementName = "npc_info")]
    public class NPCInfo
    {
        [Tooltip("Unique identifier used to link trader information to the NPC.")]
        [XmlAttribute(AttributeName = "id")] public string Id;
        
        [Tooltip("The localized name of the NPC")]
        [XmlAttribute(AttributeName = "name")] public string Name;

        [XmlAttribute(AttributeName = "name_key")]
        [HideInInspector]
        public string Name_key;

        [XmlAttribute(AttributeName = "faction")]
        [Tooltip("Which faction this npc belongs too.")]
        public string Faction;

        [XmlAttribute(AttributeName = "portrait")]
        [HideInInspector]
        public string Portrait = "npc_joel";

        
        [XmlAttribute(AttributeName = "greeting_type")]
        [HideInInspector]
        public string Greeting_type = "nearby";

        [XmlAttribute(AttributeName = "stance")]
        [Tooltip("Controls the greeting type towards the player.")]
        public StanceTypes Stance;

        [XmlAttribute(AttributeName = "voice_set")]
        [HideInInspector]
        public string Voice_set ="trader";

        [XmlAttribute(AttributeName = "trader_id")]
        [Tooltip("Connects the NPC to a trading inventory.")]
        public string Trader_id;

        [XmlAttribute(AttributeName = "dialog_id")]
        [HideInInspector]
        public string Dialog_id;

        [XmlAttribute(AttributeName = "quest_faction")]
        public string Quest_faction;
        
        [XmlAttribute(AttributeName = "quest_list")]
        public string Quest_list;
    }
}