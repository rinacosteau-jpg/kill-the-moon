using System.Collections;
using TMPro;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject doorObject;
    [SerializeField] private float openZRotation = 90f;
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Locked Feedback")]
    [SerializeField] private TMP_Text lockedInteractionLabel;
    [SerializeField] private CanvasGroup lockedInteractionCanvasGroup;
    [SerializeField] private float lockedInteractionFadeDuration = 1f;

    private bool startIsOpen;
    private Transform doorTransform;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;
    private Coroutine lockedInteractionRoutine;
    private DialogueInteractable dialogueInteractable;

    private void Awake()
    {
        if (doorObject == null && transform.childCount > 0)
            doorObject = transform.GetChild(0).gameObject;

        doorTransform = doorObject != null ? doorObject.transform : transform;
        if (doorTransform != null)
            originalRotation = doorTransform.localRotation;

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

        ApplyState(!isOpen);
    }

    public void OnLoopReset()
    {
        ApplyState(startIsOpen, true);
    }

    public void ForceOpen()
    {
        ApplyState(true);
    }

    private void ApplyState(bool open, bool instant = false)
    {
        bool wasOpen = isOpen;
        isOpen = open;

        if (doorTransform != null)
        {
            Quaternion targetRotation = originalRotation;
            if (open)
                targetRotation = originalRotation * Quaternion.Euler(0f, 0f, openZRotation);

            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }

            if (instant || rotationDuration <= 0f)
            {
                doorTransform.localRotation = targetRotation;
            }
            else
            {
                rotationCoroutine = StartCoroutine(RotateDoor(targetRotation));
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

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        Quaternion startRotation = doorTransform.localRotation;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            doorTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
        rotationCoroutine = null;
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
