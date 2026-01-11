using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public float typingSpeed = 0.03f;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    public DialogueData dialogueData;
    private Action onDialogueFinished;
    private DialogueNode currentNode;

    void Awake()
    {
        instance = this;
    }

    // ---------------------------------------------------
    // START DIALOGUE
    // ---------------------------------------------------

    public void StartDialogue(string startNodeID, Action onFinished = null)
    {
        AudioManager.instance.play("DialougePanelOpen");

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

        // Start typing
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(node.dialogueText));
    }

    // ---------------------------------------------------
    // TYPEWRITER (NO UI GLITCH, NO <br> FLASHING)
    // ---------------------------------------------------

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        // Convert <br> to \n before typing
        fullText = fullText.Replace("<br>", "\n");

        // IMPORTANT:
        // TMP parses the entire rich-text at once but reveals characters gradually
        dialogueText.text = fullText;

        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int total = dialogueText.textInfo.characterCount;

        for (int i = 0; i < total; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

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
