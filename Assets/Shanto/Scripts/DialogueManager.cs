using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;

    public DialogueData dialogueData;
    private Action onDialogueFinished;


    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void StartDialogue(string startNodeID, Action onFinished)
    {
        dialoguePanel.SetActive(true);
        DisplayNode(dialogueData.GetNode(startNodeID));

        onDialogueFinished = onFinished;
    }

    void DisplayNode(DialogueNode node)
    {
        if (node == null) return;

        speakerNameText.text = node.speakerName;
        dialogueText.text = node.dialogueText;

        foreach (Transform child in optionsContainer) Destroy(child.gameObject);

        foreach (var option in node.options)
        {
            GameObject btn = Instantiate(optionButtonPrefab, optionsContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = option.optionText;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                option.onOptionSelected?.Invoke();

                if (option.isExit)
                    EndDialogue();
                else
                    DisplayNode(dialogueData.GetNode(option.nextNodeID));
            });
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // invoke callback AFTER hiding the panel
        onDialogueFinished?.Invoke();
        onDialogueFinished = null;     // reset for safety
    }
}
