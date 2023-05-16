using UnityEngine;

namespace Dialogue.GameData
{
    public class DefaultDialogEnums
    {
        public void Init()
        {
            
        }

        public enum ModifiyCVar
        {
            add,
            sub
            
        }
        public enum Operator
        {
            None,
            lt,
            lte,
            eq,
            neq,
            gte,
            gt
        }

        public enum FactionStance
        {
            Hate,
            Dislike = 200,
            Neutral = 400,
            Like = 600,
            Love = 800,
            Leader = 1001
        }
      

        public enum DialogOperator
        {
            None,
            Add,
            Sub
        }

        public enum QuestStatus
        {
            InProgress,
            NotStarted
        }
    
        public enum Language
        {
            English,
            German,
            Latem,
            French,
            Italian,
            Japanese,
            Koreana,
            Polish,
            Brazilian,
            Russian,
            Turkish,
            SChinese,
            TChinese,
            Spanish
        }
    }
}