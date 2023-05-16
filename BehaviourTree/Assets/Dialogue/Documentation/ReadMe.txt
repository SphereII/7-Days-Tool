The 7 Days To Dialogue app was created by sphereii and xyth to allow modders to easily create new dialogs for NPCs.

This is a work in progress. Suggestions, feedback, and bug reports are welcome.

Overview.txt :
    Document that covers the flow on how an NPC is connected to a dialog, and breaks down all the parts of a dialog.
    
    It's encouraged to review this document to better understand the flow expected.


Using Unity GraphView (experimental) component, we have created a utility that will help you visually build up a conversation
to have with an NPC. 

Features:
    - Visually create a conversation with an NPC
    - Easily add in requirements and actions when needed.
    - Import existing dialogs.xml
    - Load reference files items.xml, buffs.xml, quests.xml, and Localization.txt.
    - Export the dialogs to xml files to be used in the game.
    - Test Scene allows you to play the sequence in Unity
    - "Scratch" feature to allow you to quickly flesh out a conversation.
    - Includes SCore (sphereii's Core) hooks for additional requirements / actions.
    - Allows additional requirements / actions to be loaded via the Assets/Dialogue/Configuration folder.
    - Auto Saving of graph after each change
    - Undo / Redo features using Ctrl-Z, and through the Edit menu

Terminology:
    DialogGraph     : A ScriptableObject that contains the entire dialog, including statements and responses.
    Root Node       : The starter node that defines the dialog id.
    Statement Node  : A node that defines what the NPC will say.
    Response Node   : A node that defines what a player can say in response.
    Action Node     : A node that defines one or more actions.
    Edge            : Links the Statements to Responses, and Responses to Actions.
    

Quick Start:
    Create an empty Unity project, using Unity 2021.3+
    Import the 7 Days To Dialogue Unity Package.
    Copy the items.xml, buffs.xml, quests.xml, and Localization.txt into the GameXmls folder.
        - This will help populate the right localization references, allow you to quick pick buffs from a list, etc.
    Create an empty folder off of Assets called "Dialogs"
    Right click inside "Dialogs" folder, go to Create, then Dialog Graph.
    - Enter in the name of the dialog graph. This defines the dialog id.
    Double click on the new dialog graph.
        * Note: This may take a minute or two for the first time, as it reads the optional GameXmls.
    Press the Auto Layout button.
        
Tip: If you are adding dialogs to a modded game, use the game files from the ConfigDumps output of your save game.
    
Constraints:
    - Statement Node's output ("To Response...") can only connect to another Statement Node, or a Response Node.
    - An Action Node may appear connected, but this connection will be removed.
    - A Response Node's output ("To statement and/or actions") can only connect to a Statement Node or Action Node. 

Testing a Dialog Graph:
    Load the Dialogue Test scene under Assets/Dialogue/Scenes
    Click on the TestingScreen in the Hierarchy
    In the Inspect, drag your dialog graph into the Dialogue Runner (Script)'s Dialog Graph Entry
    Press Play.
    
    Responses with requirements with have toggles to turn on and off, simulating passing the requirement check.


Exporting a Dialog Graph:
    Open the Dialog Graph you want to export
    Press the Export button
    The exported dialogs.xml will be exported into Assets/Exports/Dialogue/
    (Optional) A Localization.txt will be generated.