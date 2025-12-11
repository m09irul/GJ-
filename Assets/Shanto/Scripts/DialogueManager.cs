using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;
    public Image image;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;   // speed of each letter
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    public DialogueData dialogueData;
    private Action onDialogueFinished;
    private DialogueNode currentNode;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // ---------------------------------------------------
    // START DIALOGUE
    // ---------------------------------------------------

    public void StartDialogue(string startNodeID, Action onFinished)
    {
        dialoguePanel.SetActive(true);
        onDialogueFinished = onFinished;

        ShowNode(dialogueData.GetNode(startNodeID));
    }

    // ---------------------------------------------------
    // SHOW NODE
    // ---------------------------------------------------

    void ShowNode(DialogueNode node)
    {
        if (node == null) return;
        currentNode = node;

        // Setup image
        if (node.image != null)
        {
            image.sprite = node.image;
            image.SetNativeSize();
            image.gameObject.SetActive(true);
        }
        else
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
        }

        // Hide buttons until typing finished
        ClearOptions();
        optionsContainer.gameObject.SetActive(false);

        // Start typing text
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(node.dialogueText));
    }

    // ---------------------------------------------------
    // TYPEWRITER EFFECT
    // ---------------------------------------------------

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // After typing finishes → show buttons
        ShowOptions();
    }

    // ---------------------------------------------------
    // OPTION BUTTONS
    // ---------------------------------------------------

    void ShowOptions()
    {
        ClearOptions();

        foreach (var option in currentNode.options)
        {
            GameObject btn = Instantiate(optionButtonPrefab, optionsContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = option.optionText;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (option.isExit)
                    EndDialogue();
                else
                    ShowNode(dialogueData.GetNode(option.nextNodeID));
            });
        }

        optionsContainer.gameObject.SetActive(true);
    }

    void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);
    }

    // ---------------------------------------------------
    // END DIALOGUE
    // ---------------------------------------------------

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        onDialogueFinished?.Invoke();
        onDialogueFinished = null;
    }
}
