using System.Collections;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class GetGunEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private PlayerMovementScript playerMovement;
    [SerializeField] private PlayerInteractScript playerInteract;

    [Header("Dialogue Fragments")]
    [SerializeField] private ArticyRef dialogueFragmentStageA;
    [SerializeField] private ArticyRef dialogueFragmentStageB;
    [SerializeField] private ArticyRef dialogueFragmentStageC;

    [Header("Timings")]
    [SerializeField] private float controlBlockDuration = 3f;

    private bool isSequenceRunning;

    private void Reset()
    {
        dialogueUI = FindObjectOfType<DialogueUI>(true);
        playerMovement = FindObjectOfType<PlayerMovementScript>(true);
        playerInteract = FindObjectOfType<PlayerInteractScript>(true);
    }

    private void Awake()
    {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true);

        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovementScript>(true);

        if (playerInteract == null)
            playerInteract = FindObjectOfType<PlayerInteractScript>(true);
    }

    private void Update()
    {
        if (isSequenceRunning)
            return;

        if (!IsGunStealingTriggered())
            return;

        StartCoroutine(RunSequence());
    }

    private bool IsGunStealingTriggered()
    {
        var variables = ArticyGlobalVariables.Default;
        if (variables == null)
            return false;

        return variables.EVT.event_gunStealing == 1;
    }

    private IEnumerator RunSequence()
    {
        isSequenceRunning = true;

        try
        {
            yield return WaitForAllDialoguesToClose();

            yield return BlockPlayerControl(controlBlockDuration);

            yield return StartStageDialogue();

            yield return BlockPlayerControl(controlBlockDuration);

            yield return StartDialogue(dialogueFragmentStageC);

            ResetGunStealingFlag();
        }
        finally
        {
            SetPlayerControlEnabled(true);
            isSequenceRunning = false;
        }
    }

    private IEnumerator WaitForAllDialoguesToClose()
    {
        while (AreAnyDialoguesOpen())
            yield return null;
    }

    private bool AreAnyDialoguesOpen()
    {
        DialogueUI[] dialogueUIs = FindObjectsOfType<DialogueUI>(true);
        if (dialogueUIs == null || dialogueUIs.Length == 0)
            return false;

        for (int i = 0; i < dialogueUIs.Length; i++)
        {
            DialogueUI ui = dialogueUIs[i];
            if (ui != null && ui.IsDialogueOpen)
                return true;
        }

        return false;
    }

    private IEnumerator BlockPlayerControl(float duration)
    {
        SetPlayerControlEnabled(false);

        float waitDuration = Mathf.Max(duration, 0f);
        if (waitDuration > 0f)
            yield return new WaitForSeconds(waitDuration);

        SetPlayerControlEnabled(true);
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovementScript>(true);

        if (playerInteract == null)
            playerInteract = FindObjectOfType<PlayerInteractScript>(true);

        if (playerMovement != null)
            playerMovement.enabled = enabled;

        if (playerInteract != null)
            playerInteract.enabled = enabled;
    }

    private IEnumerator StartStageDialogue()
    {
        ArticyRef dialogueRef = GetStageDialogueRef();
        if (dialogueRef == null)
            yield break;

        yield return StartDialogue(dialogueRef);
    }

    private ArticyRef GetStageDialogueRef()
    {
        var variables = ArticyGlobalVariables.Default;
        if (variables == null)
            return null;

        int stage = variables.QUEST.getGun_Stage;
        switch (stage)
        {
            case 1:
                if (dialogueFragmentStageA == null)
                    Debug.LogWarning("[GetGunEvent] Dialogue fragment A is not assigned.");
                return dialogueFragmentStageA;
            case 2:
                if (dialogueFragmentStageB == null)
                    Debug.LogWarning("[GetGunEvent] Dialogue fragment B is not assigned.");
                return dialogueFragmentStageB;
            default:
                Debug.LogWarning($"[GetGunEvent] Unexpected getGun stage value: {stage}. No stage dialogue will be started.");
                return null;
        }
    }

    private IEnumerator StartDialogue(ArticyRef dialogueRef)
    {
        if (dialogueRef == null)
            yield break;

        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true);

        if (dialogueUI == null)
        {
            Debug.LogWarning("[GetGunEvent] DialogueUI reference is missing. Cannot start dialogue.");
            yield break;
        }

        bool dialogueClosed = false;

        void OnDialogueClosed(DialogueUI ui)
        {
            if (ui != dialogueUI)
                return;

            dialogueClosed = true;
            dialogueUI.DialogueClosed -= OnDialogueClosed;
        }

        dialogueUI.DialogueClosed += OnDialogueClosed;
        dialogueUI.StartDialogue(dialogueRef);

        if (dialogueUI.IsDialogueOpen)
            yield return new WaitUntil(() => dialogueClosed || !dialogueUI.IsDialogueOpen);

        if (!dialogueClosed)
            dialogueUI.DialogueClosed -= OnDialogueClosed;
    }

    private void ResetGunStealingFlag()
    {
        var variables = ArticyGlobalVariables.Default;
        if (variables == null)
            return;

        variables.EVT.event_gunStealing = 0;
    }
}
