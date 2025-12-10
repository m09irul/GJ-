using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData conversation;
    public string startNodeID = "START";

    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        DialogueManager.instance.StartDialogue(conversation, startNodeID);
    }
}
