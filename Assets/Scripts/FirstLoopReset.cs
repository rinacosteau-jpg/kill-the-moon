using System.Collections;
using System.Collections.Generic;
using Articy.World_Of_Red_Moon.GlobalVariables;
using TMPro;
using UnityEngine;

/// <summary>
/// Applies first loop reset behaviour: locks configured doors and blocks NPC interactions
/// until the player talks to Tasha and Articy signals the doors should unlock.
/// </summary>
public class FirstLoopReset : MonoBehaviour, ILoopResettable
{
    [Header("Door Locking")]
    [SerializeField] private DoorInteractable[] doorsToLock;

    [Header("Interaction Blocking")]
    [SerializeField] private PlayerInteractScript playerInteract;
    [SerializeField] private NPCInteractable tashaNpc;
    [SerializeField] private TMP_Text interactionBlockedLabel;
    [SerializeField] private CanvasGroup interactionBlockedCanvasGroup;
    [SerializeField] private float interactionBlockedFadeDuration = 1f;
    [SerializeField] private string interactionBlockedMessage = "I should talk to Tasha first.";

    private bool hasAppliedFirstLoopReset;
    private bool doorsUnlockedByFlag;
    private bool interactionBlockActive;
    private Coroutine interactionBlockedRoutine;

    private void Awake()
    {
        InitializeInteractionBlockedMessage();
    }

    private void OnEnable()
    {
        if (playerInteract != null)
            playerInteract.InteractionWhileBlocked += HandleInteractionWhileBlocked;
    }

    private void OnDisable()
    {
        if (playerInteract != null)
            playerInteract.InteractionWhileBlocked -= HandleInteractionWhileBlocked;

        DisableInteractionBlock();
        StopInteractionBlockedRoutine();
    }

    private void Update()
    {
        if (!hasAppliedFirstLoopReset)
            return;

        var globals = ArticyGlobalVariables.Default;
        var rflg = globals?.RFLG;
        if (rflg == null)
            return;

        if (!doorsUnlockedByFlag && rflg.loop1unlock)
            UnlockDoors();

        if (interactionBlockActive && rflg.talkedTasha)
            DisableInteractionBlock();
    }

    public void OnLoopReset()
    {
        if (hasAppliedFirstLoopReset)
            return;

        hasAppliedFirstLoopReset = true;

        LockDoors();
        EnableInteractionBlock();
    }

    private void LockDoors()
    {
        if (doorsToLock == null)
            return;

        foreach (var door in doorsToLock)
        {
            if (door == null)
                continue;

            door.SetLocked(true);
        }
    }

    private void UnlockDoors()
    {
        doorsUnlockedByFlag = true;

        if (doorsToLock == null)
            return;

        foreach (var door in doorsToLock)
        {
            if (door == null)
                continue;

            door.SetLocked(false);
        }
    }

    private void EnableInteractionBlock()
    {
        if (interactionBlockActive)
            return;

        if (playerInteract == null)
        {
            Debug.LogWarning($"[{nameof(FirstLoopReset)}] PlayerInteractScript is not assigned.");
            return;
        }

        playerInteract.SetInteractionsBlocked(true, CanBypassInteractionBlock);
        interactionBlockActive = true;
    }

    private void DisableInteractionBlock()
    {
        if (!interactionBlockActive)
            return;

        if (playerInteract != null)
            playerInteract.SetInteractionsBlocked(false);

        interactionBlockActive = false;
        StopInteractionBlockedRoutine();
        InitializeInteractionBlockedMessage();
    }

    private bool CanBypassInteractionBlock(IInteractable interactable)
    {
        if (interactable == null)
            return false;

        if (interactable == tashaNpc)
            return true;

        return interactable is not NPCInteractable;
    }

    private void HandleInteractionWhileBlocked(IReadOnlyList<IInteractable> interactables)
    {
        if (!interactionBlockActive)
            return;

        if (interactables == null)
            return;

        bool hasBlockedNpc = false;
        foreach (var interactable in interactables)
        {
            if (interactable == null)
                continue;

            if (interactable == tashaNpc)
                return;

            if (interactable is NPCInteractable)
            {
                hasBlockedNpc = true;
                break;
            }
        }

        if (!hasBlockedNpc)
            return;

        ShowInteractionBlockedMessage();
    }

    private void InitializeInteractionBlockedMessage()
    {
        if (interactionBlockedCanvasGroup != null)
            interactionBlockedCanvasGroup.alpha = 0f;

        if (interactionBlockedLabel != null)
        {
            interactionBlockedLabel.text = string.Empty;
            interactionBlockedLabel.color = Color.white;
        }
    }

    private void ShowInteractionBlockedMessage()
    {
        if (interactionBlockedLabel == null || interactionBlockedCanvasGroup == null)
            return;

        interactionBlockedLabel.text = interactionBlockedMessage;
        interactionBlockedLabel.color = Color.white;

        StopInteractionBlockedRoutine();
        interactionBlockedRoutine = StartCoroutine(FadeInteractionBlockedMessage());
    }

    private IEnumerator FadeInteractionBlockedMessage()
    {
        if (interactionBlockedCanvasGroup == null)
            yield break;

        float duration = Mathf.Max(interactionBlockedFadeDuration, Mathf.Epsilon);
        interactionBlockedCanvasGroup.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            interactionBlockedCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        interactionBlockedCanvasGroup.alpha = 0f;
        interactionBlockedRoutine = null;
    }

    private void StopInteractionBlockedRoutine()
    {
        if (interactionBlockedRoutine != null)
        {
            StopCoroutine(interactionBlockedRoutine);
            interactionBlockedRoutine = null;
        }
    }
}
