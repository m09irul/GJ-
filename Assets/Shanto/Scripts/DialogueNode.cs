using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueNode
{
    public string nodeID;

    public string dialogueText;
    public Sprite image;

    public DialogueOption[] options;
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public string nextNodeID;
    public bool isExit;
}
