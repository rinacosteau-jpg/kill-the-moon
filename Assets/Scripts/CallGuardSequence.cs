using System.Collections;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class CallGuardSequence : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ArticyRef guardDialogueStart;

    [Header("Doors")]
    [SerializeField] private DoorInteractable[] doorsToUnlock;

    [Header("Character Movement")]
    [SerializeField] private Transform characterToMove;
    [SerializeField] private Transform destinationTransform;
    [SerializeField] private Vector3 fallbackDestinationPosition;
    [SerializeField] private bool useFallbackRotation;
    [SerializeField] private Vector3 fallbackDestinationEuler;

    [Header("Player Control")]
    [SerializeField] private PlayerMovementScript playerMovement;
    [SerializeField] private PlayerInteractScript playerInteract;
    [SerializeField] private float playerControlBlockDuration = 3f;

    [Header("Behaviour")]
    [SerializeField] private bool resetCallGuardFlag = true;

    [SerializeField] private ScreenShading screenShading;

    private bool isSequenceRunning;
    private bool lastCallGuardValue;
    private Vector3 originalCharacterPosition;
    private Quaternion originalCharacterRotation;
    private bool hasStoredCharacterTransform;

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
        CacheCharacterTransform();
    }

    private void OnEnable()
    {
        lastCallGuardValue = GetCallGuardValue();
    }

    private void CacheCharacterTransform()
    {
        if (characterToMove == null)
        {
            hasStoredCharacterTransform = false;
            return;
        }

        originalCharacterPosition = characterToMove.position;
        originalCharacterRotation = characterToMove.rotation;
        hasStoredCharacterTransform = true;
    }

    private bool GetCallGuardValue()
    {
        return ArticyGlobalVariables.Default?.RFLG?.callGuard ?? false;
    }

    private void Update()
    {
        bool currentValue = GetCallGuardValue();
        if (!isSequenceRunning && !lastCallGuardValue && currentValue)
        {
            StartCoroutine(RunSequence());
        }

        lastCallGuardValue = currentValue;
    }

    private IEnumerator RunSequence()
    {
        isSequenceRunning = true;

        if (resetCallGuardFlag && ArticyGlobalVariables.Default != null)
        {
            ArticyGlobalVariables.Default.RFLG.callGuard = false;
            lastCallGuardValue = false;
        }

        CacheCharacterTransform();
        SetPlayerControlBlocked(true);

        screenShading.Shade();
        

        float delay = Mathf.Max(playerControlBlockDuration, 0f);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        UnlockDoors();
        MoveCharacterToDestination();

        screenShading.Unshade();

        yield return StartGuardDialogue();

        RestoreCharacter();
        SetPlayerControlBlocked(false);
        isSequenceRunning = false;
    }

    private void SetPlayerControlBlocked(bool blocked)
    {
        if (playerMovement != null)
            playerMovement.enabled = !blocked;

        if (playerInteract != null)
            playerInteract.enabled = !blocked;
    }

    private void UnlockDoors()
    {
        if (doorsToUnlock == null)
            return;

        for (int i = 0; i < doorsToUnlock.Length; i++)
        {
            DoorInteractable door = doorsToUnlock[i];
            if (door == null)
                continue;

            door.SetLocked(false);
            door.ForceOpen();
        }
    }

    private void MoveCharacterToDestination()
    {
        if (characterToMove == null)
            return;

        if (destinationTransform != null)
        {
            characterToMove.SetPositionAndRotation(destinationTransform.position, destinationTransform.rotation);
            return;
        }

        characterToMove.position = fallbackDestinationPosition;
        if (useFallbackRotation)
            characterToMove.rotation = Quaternion.Euler(fallbackDestinationEuler);
    }

    private IEnumerator StartGuardDialogue()
    {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true);

        if (dialogueUI == null || guardDialogueStart == null)
            yield break;

        bool dialogueClosed = false;

        void OnDialogueClosed(DialogueUI ui)
        {
            if (ui != dialogueUI)
                return;

            dialogueClosed = true;
            dialogueUI.DialogueClosed -= OnDialogueClosed;
        }

        dialogueUI.DialogueClosed += OnDialogueClosed;
        dialogueUI.StartDialogue(guardDialogueStart);

        if (dialogueUI.IsDialogueOpen)
            yield return new WaitUntil(() => dialogueClosed || !dialogueUI.IsDialogueOpen);

        if (!dialogueClosed)
            dialogueUI.DialogueClosed -= OnDialogueClosed;
    }

    private void RestoreCharacter()
    {
        if (characterToMove == null)
            return;

        if (hasStoredCharacterTransform)
            characterToMove.SetPositionAndRotation(originalCharacterPosition, originalCharacterRotation);
    }
}
