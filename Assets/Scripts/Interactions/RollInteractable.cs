using System.Collections;
using TMPro;
using UnityEngine;

public class RollInteractable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private bool canClose = true;
    [SerializeField] private GameObject rollObject;
    [SerializeField] private float openZOffset = 1f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Locked Feedback")]
    [SerializeField] private TMP_Text lockedInteractionLabel;
    [SerializeField] private CanvasGroup lockedInteractionCanvasGroup;
    [SerializeField] private float lockedInteractionFadeDuration = 1f;

    private bool startIsOpen;
    private Transform rollTransform;
    private Vector3 originalPosition;
    private Coroutine moveCoroutine;
    private Coroutine lockedInteractionRoutine;
    private DialogueInteractable dialogueInteractable;

    private void Awake()
    {
        if (rollObject == null && transform.childCount > 0)
            rollObject = transform.GetChild(0).gameObject;

        rollTransform = rollObject != null ? rollObject.transform : transform;
        if (rollTransform != null)
            originalPosition = rollTransform.localPosition;

        dialogueInteractable = GetComponent<DialogueInteractable>();

        startIsOpen = isOpen;
        ApplyState(isOpen, true);

        InitializeLockedInteractionMessage();
    }

    public void Interact()
    {
        if (isLocked)
        {
            ShowLockedInteractionMessage();
            TryStartLockedDialogue();
            return;
        }

        bool targetOpen = !isOpen;
        if (!targetOpen && isOpen && !canClose)
            return;

        ApplyState(targetOpen);
    }

    public void OnLoopReset()
    {
        ApplyState(startIsOpen, true, true);
    }

    public void ForceOpen()
    {
        ApplyState(true);
    }

    private void ApplyState(bool open, bool instant = false, bool force = false)
    {
        bool wasOpen = isOpen;
        if (!force && !open && wasOpen && !canClose)
            return;

        isOpen = open;

        if (rollTransform != null)
        {
            Vector3 targetPosition = originalPosition;
            if (open)
                targetPosition = originalPosition + new Vector3(0f, 0f, openZOffset);

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            if (instant || moveDuration <= 0f)
            {
                rollTransform.localPosition = targetPosition;
            }
            else
            {
                moveCoroutine = StartCoroutine(MoveRoll(targetPosition));
            }
        }

        if (audioSource != null)
        {
            if (open && !wasOpen && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
            else if (!open && wasOpen && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }
        }
    }

    private IEnumerator MoveRoll(Vector3 targetPosition)
    {
        Vector3 startPosition = rollTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            rollTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rollTransform.localPosition = targetPosition;
        moveCoroutine = null;
    }

    private void InitializeLockedInteractionMessage()
    {
        if (lockedInteractionCanvasGroup != null)
            lockedInteractionCanvasGroup.alpha = 0f;

        if (lockedInteractionLabel != null)
        {
            lockedInteractionLabel.text = string.Empty;
            lockedInteractionLabel.color = Color.white;
        }
    }

    private void ShowLockedInteractionMessage()
    {
        if (lockedInteractionLabel == null || lockedInteractionCanvasGroup == null)
            return;

        lockedInteractionLabel.text = "Door is locked";
        lockedInteractionLabel.color = Color.white;

        if (lockedInteractionRoutine != null)
        {
            StopCoroutine(lockedInteractionRoutine);
            lockedInteractionRoutine = null;
        }

        lockedInteractionRoutine = StartCoroutine(FadeLockedInteractionMessage());
    }

    private IEnumerator FadeLockedInteractionMessage()
    {
        if (lockedInteractionCanvasGroup == null)
            yield break;

        float duration = Mathf.Max(lockedInteractionFadeDuration, Mathf.Epsilon);
        lockedInteractionCanvasGroup.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            lockedInteractionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        lockedInteractionCanvasGroup.alpha = 0f;
        lockedInteractionRoutine = null;
    }

    private void TryStartLockedDialogue()
    {
        if (dialogueInteractable == null)
            return;

        dialogueInteractable.Interact();
    }

    private void OnDisable()
    {
        if (lockedInteractionRoutine != null)
        {
            StopCoroutine(lockedInteractionRoutine);
            lockedInteractionRoutine = null;
        }

        if (lockedInteractionCanvasGroup != null)
            lockedInteractionCanvasGroup.alpha = 0f;

        if (lockedInteractionLabel != null)
            lockedInteractionLabel.text = string.Empty;
    }
}
