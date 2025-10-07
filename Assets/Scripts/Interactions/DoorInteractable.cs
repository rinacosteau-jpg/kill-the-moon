using System.Collections;
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

    private bool startIsOpen;
    private Transform doorTransform;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        if (doorObject == null && transform.childCount > 0)
            doorObject = transform.GetChild(0).gameObject;

        doorTransform = doorObject != null ? doorObject.transform : transform;
        if (doorTransform != null)
            originalRotation = doorTransform.localRotation;

        startIsOpen = isOpen;
        ApplyState(isOpen, true);
    }

    public void Interact()
    {
        if (isLocked)
            return;

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
}
