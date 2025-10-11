using System.Collections;
using UnityEngine;
using Articy.Unity;
using Articy.Unity.Interfaces;

public class WatchedDialogueSequence : MonoBehaviour
{
    [Header("Dialogue References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ArticyRef watchDialogue;
    [SerializeField] private ArticyRef guardDialogue;

    [Header("Guard Dialogue")]
    [SerializeField] private float guardDialogueDelay = 3f;

    [Header("Screen Shading")]
    [SerializeField] private ScreenShading screenShading;

    [Header("World References")]
    [SerializeField] private DoorInteractable doorToOpen;
    [SerializeField] private Transform characterToMove;
    [SerializeField] private Transform targetTransform;

    private IFlowObject watchedFlowObject;
    private bool watchedDialogueActive;
    private bool suppressNextStartCheck;
    private bool guardDialogueActive;
    private Coroutine guardDialogueRoutine;
    private Vector3 originalCharacterPosition;
    private Quaternion originalCharacterRotation;
    private bool hasOriginalCharacterTransform;
    private bool hasAppliedScreenShading;
    private bool wantsScreenShade;
    private bool waitingForScreenShadingInstance;

    private void Reset()
    {
        dialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void Awake()
    {
        CacheWatchedFlowObject();
        CacheOriginalCharacterTransform();
        ReleaseScreenShading();
    }

    private void OnValidate()
    {
        CacheWatchedFlowObject();
        if (!Application.isPlaying)
            CacheOriginalCharacterTransform();
    }

    private void OnEnable()
    {
        CacheWatchedFlowObject();
        EnsureOriginalCharacterTransformCached();
        ReleaseScreenShading();
        Subscribe();
    }

    private void OnDisable()
    {
        CancelGuardDialogueRoutine();
        ReleaseScreenShading();
        Unsubscribe();
        StopWaitingForScreenShading();
    }

    private void Subscribe()
    {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();

        if (dialogueUI == null)
        {
            Debug.LogWarning($"[{nameof(WatchedDialogueSequence)}] DialogueUI not assigned.");
            return;
        }

        dialogueUI.DialogueStarted += OnDialogueStarted;
        dialogueUI.DialogueClosed += OnDialogueClosed;
    }

    private void Unsubscribe()
    {
        if (dialogueUI == null)
            return;

        dialogueUI.DialogueStarted -= OnDialogueStarted;
        dialogueUI.DialogueClosed -= OnDialogueClosed;
    }

    private void CacheWatchedFlowObject()
    {
        if (watchDialogue == null)
        {
            watchedFlowObject = null;
            return;
        }

        var articyObject = watchDialogue.GetObject();
        var flowObject = articyObject as IFlowObject;
        if (articyObject != null && flowObject == null)
        {
            Debug.LogWarning($"[{nameof(WatchedDialogueSequence)}] watchDialogue does not reference an IFlowObject.");
        }

        watchedFlowObject = flowObject;
    }

    private void CacheOriginalCharacterTransform()
    {
        if (characterToMove == null)
        {
            hasOriginalCharacterTransform = false;
            return;
        }

        originalCharacterPosition = characterToMove.position;
        originalCharacterRotation = characterToMove.rotation;
        hasOriginalCharacterTransform = true;
    }

    private void EnsureOriginalCharacterTransformCached()
    {
        if (hasOriginalCharacterTransform || characterToMove == null)
            return;

        CacheOriginalCharacterTransform();
    }

    private void RestoreCharacterToOriginal()
    {
        if (!hasOriginalCharacterTransform || characterToMove == null)
            return;

        characterToMove.SetPositionAndRotation(originalCharacterPosition, originalCharacterRotation);
    }

    private void CancelGuardDialogueRoutine()
    {
        if (guardDialogueRoutine == null)
            return;

        StopCoroutine(guardDialogueRoutine);
        guardDialogueRoutine = null;
        ReleaseScreenShading();
    }

    private void OnDialogueStarted(DialogueUI ui)
    {
        if (ui != dialogueUI)
            return;

        if (suppressNextStartCheck)
        {
            suppressNextStartCheck = false;
            watchedDialogueActive = false;
            return;
        }

        if (watchedFlowObject == null && watchDialogue != null)
            CacheWatchedFlowObject();

        var currentStart = ui.CurrentStartObject;
        watchedDialogueActive = FlowObjectsMatch(currentStart, watchedFlowObject);
    }

    private void OnDialogueClosed(DialogueUI ui)
    {
        if (ui != dialogueUI)
            return;

        if (guardDialogueActive)
        {
            guardDialogueActive = false;
            RestoreCharacterToOriginal();
            return;
        }

        if (!watchedDialogueActive)
            return;

        watchedDialogueActive = false;

        if (doorToOpen != null)
            doorToOpen.ForceOpen();

        if (characterToMove != null && targetTransform != null)
            characterToMove.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);

        CancelGuardDialogueRoutine();

        if (dialogueUI != null && guardDialogue != null)
            guardDialogueRoutine = StartCoroutine(BeginGuardDialogueSequence());
    }

    private IEnumerator BeginGuardDialogueSequence()
    {
        ApplyScreenShading();

        float delay = Mathf.Max(guardDialogueDelay, 0f);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        ReleaseScreenShading();

        if (dialogueUI != null && guardDialogue != null)
        {
            suppressNextStartCheck = true;
            guardDialogueActive = true;
            dialogueUI.StartDialogue(guardDialogue);
        }

        guardDialogueRoutine = null;
    }

    private ScreenShading ResolveScreenShading()
    {
        if (screenShading != null)
            return screenShading;

        screenShading = ScreenShading.Instance != null
            ? ScreenShading.Instance
            : FindObjectOfType<ScreenShading>(true);

        if (screenShading == null)
            Debug.LogWarning($"[{nameof(WatchedDialogueSequence)}] ScreenShading not found.");

        return screenShading;
    }

    private void ApplyScreenShading()
    {
        wantsScreenShade = true;

        if (hasAppliedScreenShading)
            return;

        var shading = ResolveScreenShading();
        if (shading == null)
        {
            BeginWaitingForScreenShading();
            return;
        }

        shading.Shade();
        hasAppliedScreenShading = true;
        StopWaitingForScreenShading();
    }

    private void ReleaseScreenShading()
    {
        wantsScreenShade = false;

        var shading = ResolveScreenShading();
        if (shading == null)
        {
            hasAppliedScreenShading = false;
            StopWaitingForScreenShading();
            return;
        }

        if (hasAppliedScreenShading)
        {
            shading.Unshade();
            hasAppliedScreenShading = false;
        }

        StopWaitingForScreenShading();
    }

    private void HandleScreenShadingInstanceChanged(ScreenShading instance)
    {
        screenShading = instance;

        if (instance == null)
        {
            hasAppliedScreenShading = false;
            return;
        }

        if (!wantsScreenShade)
        {
            StopWaitingForScreenShading();
            return;
        }

        hasAppliedScreenShading = false;
        ApplyScreenShading();
    }

    private void BeginWaitingForScreenShading()
    {
        if (waitingForScreenShadingInstance)
            return;

        ScreenShading.InstanceChanged += HandleScreenShadingInstanceChanged;
        waitingForScreenShadingInstance = true;
    }

    private void StopWaitingForScreenShading()
    {
        if (!waitingForScreenShadingInstance)
            return;

        ScreenShading.InstanceChanged -= HandleScreenShadingInstanceChanged;
        waitingForScreenShadingInstance = false;
    }

    private static bool FlowObjectsMatch(IFlowObject currentStart, IFlowObject watchedObject)
    {
        if (ReferenceEquals(currentStart, watchedObject))
            return currentStart != null;

        if (currentStart == null || watchedObject == null)
            return false;

        if (currentStart is ArticyObject articyCurrent && watchedObject is ArticyObject articyWatched)
            return articyCurrent.Id.Equals(articyWatched.Id);

        return currentStart.Equals(watchedObject);
    }
}
