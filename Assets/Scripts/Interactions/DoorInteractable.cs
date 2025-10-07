using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject doorObject;
    [SerializeField] private float openZRotation = 90f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;

    private bool startIsOpen;
    private Transform doorTransform;
    private Quaternion originalRotation;

    private void Awake()
    {
        if (doorObject == null && transform.childCount > 0)
            doorObject = transform.GetChild(0).gameObject;

        doorTransform = doorObject != null ? doorObject.transform : transform;
        if (doorTransform != null)
            originalRotation = doorTransform.localRotation;

        startIsOpen = isOpen;
        ApplyState(isOpen);
    }

    public void Interact()
    {
        if (isLocked)
            return;

        ApplyState(!isOpen);
    }

    public void OnLoopReset()
    {
        ApplyState(startIsOpen);
    }

    public void ForceOpen()
    {
        ApplyState(true);
    }

    private void ApplyState(bool open)
    {
        bool wasOpen = isOpen;
        isOpen = open;

        if (doorTransform != null)
        {
            Quaternion targetRotation = originalRotation;
            if (open)
                targetRotation = originalRotation * Quaternion.Euler(0f, 0f, openZRotation);

            doorTransform.localRotation = targetRotation;
        }

        if (open && !wasOpen && audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);
    }
}
