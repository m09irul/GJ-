using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueNode
{
    public string nodeID;
    public string speakerName;

    public string dialogueText;

    public DialogueOption[] options;
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public string nextNodeID;
    public bool isExit;

    public UnityEvent onOptionSelected;
}
