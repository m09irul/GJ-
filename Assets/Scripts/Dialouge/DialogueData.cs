using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Nyaan/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes;

    public DialogueNode GetNode(string id)
    {
        return nodes.Find(n => n.nodeID == id);
    }
}
