using System.Collections;
using System.Collections.Generic;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;
using TMPro;
using UnityEngine;
using static Articy.World_Of_Red_Moon.GlobalVariables.ArticyGlobalVariables;
using static Unity.Cinemachine.CinemachineOrbitalTransposer;

/// <summary>
/// Controls the opening sequence: dialogue A → skill selection → dialogue B.
/// Keeps the screen shaded until the sequence progresses past dialogue B.
/// </summary>
public class StartSequence : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool skipSequence;

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private SkillSelectionUI skillSelectionUI;

    [Header("Dialogue Starts")]
    [SerializeField] private ArticyRef dialogueStartA;
    [SerializeField] private ArticyRef dialogueStartB;
    [SerializeField] private ArticyRef dialogueStartC;

    [Header("Movement Trigger")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float distanceAfterDialogueB = 20f;

    [Header("UI")]
    [SerializeField] private ScreenShading screenShading;
    [SerializeField] private PlayerInteractScript playerInteract;
    [SerializeField] private TMP_Text interactionBlockedLabel;
    [SerializeField] private CanvasGroup interactionBlockedCanvasGroup;
    [SerializeField] private float interactionBlockedFadeDuration = 1f;
    [SerializeField] private HintsPanelController hintsPanel;
    [SerializeField] private string inventoryHintMessage = "Use I to open Inventory, Esc to close";


    private enum SequenceStep
    {
        None,
        DialogueA,
        SkillSelection,
        DialogueB,
        WaitingForMovement,
        DialogueC,
        Completed
    }

    private SequenceStep currentStep = SequenceStep.None;
    private Vector3 lastTrackedPosition;
    private bool hasLastTrackedPosition;
    private float movementStartDistance;
    private Coroutine interactionBlockedRoutine;
    private bool hasShownInventoryHint;
    private bool hasAppliedScreenShading;
    private bool wantsScreenShade;
    private bool waitingForScreenShadingInstance;

    public static float TotalDistanceTraveled { get; private set; }


    private void Awake()
    {
        InventoryStorage.Clear();
        TotalDistanceTraveled = 0f;
        hasLastTrackedPosition = false;
        hasShownInventoryHint = false;

        if (interactionBlockedCanvasGroup != null)
            interactionBlockedCanvasGroup.alpha = 0f;

        if (interactionBlockedLabel != null)
        {
            interactionBlockedLabel.text = string.Empty;
            interactionBlockedLabel.color = Color.white;
        }

        if (!skipSequence) {
            var screenShading = ScreenShading.Instance;
            if (screenShading != null)
                screenShading.Shade();
        }
            

        //ScreenShading.Shade();
    }

    private void Update()
    {
        if (skipSequence)
            return;

        UpdateTotalDistance();

        if (currentStep != SequenceStep.WaitingForMovement)
            return;

        if (playerTransform == null)
        {
            Debug.LogError("[StartSequence] Player transform is missing while waiting for movement.");
            FinishSequence();
            return;
        }

        float traveledDistance = TotalDistanceTraveled - movementStartDistance;
        if (traveledDistance >= distanceAfterDialogueB)
            StartDialogueC();
    }

    private void OnEnable()
    {
        foreach (ArticyLanguage language in ArticyDatabase.Localization.Languages) {
            Debug.Log($"{language.ArticyLanguageId}, {language.CultureName}, {language.LanguageName}");
        }
        ArticyDatabase.Localization.SetLanguage("en");
        //Debug.Log((kk.Language).ToString());
        //kk.
        InventoryStorage.OnFirstItemAdded += HandleFirstItemAdded;

        if (skipSequence)
            return;

        if (dialogueUI != null)
            dialogueUI.DialogueClosed += HandleDialogueClosed;
        if (skillSelectionUI != null)
            skillSelectionUI.Confirmed += HandleSkillsConfirmed;
        if (playerInteract != null)
            playerInteract.InteractionWhileBlocked += HandleInteractionWhileBlocked;
    }

    private void OnDisable()
    {
        InventoryStorage.OnFirstItemAdded -= HandleFirstItemAdded;

        if (skipSequence)
            return;

        if (dialogueUI != null)
            dialogueUI.DialogueClosed -= HandleDialogueClosed;
        if (skillSelectionUI != null)
            skillSelectionUI.Confirmed -= HandleSkillsConfirmed;
        if (playerInteract != null)
            playerInteract.InteractionWhileBlocked -= HandleInteractionWhileBlocked;

        StopWaitingForScreenShading();
    }

    private void Start()
    {
        if (skipSequence)
            return;

        BeginSequence();
    }

    private void BeginSequence()
    {
        if (skipSequence)
            return;

        if (currentStep != SequenceStep.None)
            return;

        ShadeScreen();
        BlockInteractions();

        if (dialogueUI == null)
        {
            Debug.LogError("[StartSequence] DialogueUI reference is missing.");
            FinishSequence();
            return;
        }

        if (dialogueStartA == null)
        {
            Debug.LogError("[StartSequence] Dialogue start A is not assigned.");
            FinishSequence();
            return;
        }

        currentStep = SequenceStep.DialogueA;
        dialogueUI.StartDialogue(dialogueStartA);
    }

    private void HandleDialogueClosed(DialogueUI ui)
    {
        if (ui != dialogueUI)
            return;

        switch (currentStep)
        {
            case SequenceStep.DialogueA:
                StartSkillSelection();
                break;
            case SequenceStep.DialogueB:
                UnshadeScreen();
                StartWaitingForMovement();
                break;
            case SequenceStep.DialogueC:
                ShowFlashlightHint();
                FinishSequence();
                break;
        }
    }

    private void StartSkillSelection()
    {
        currentStep = SequenceStep.SkillSelection;

        if (skillSelectionUI == null)
        {
            Debug.LogError("[StartSequence] SkillSelectionUI reference is missing.");
            StartDialogueB();
            return;
        }

        skillSelectionUI.Open();
    }

    private void HandleSkillsConfirmed()
    {
        if (currentStep != SequenceStep.SkillSelection)
            return;

        StartDialogueB();
    }

    private void StartDialogueB()
    {
        currentStep = SequenceStep.DialogueB;

        if (dialogueUI == null)
        {
            Debug.LogError("[StartSequence] DialogueUI reference is missing when starting dialogue B.");
            FinishSequence();
            return;
        }

        if (dialogueStartB == null)
        {
            Debug.LogError("[StartSequence] Dialogue start B is not assigned.");
            FinishSequence();
            return;
        }

        dialogueUI.StartDialogue(dialogueStartB);
    }

    private void StartWaitingForMovement()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[StartSequence] Player transform is not assigned for movement tracking.");
            FinishSequence();
            return;
        }

        currentStep = SequenceStep.WaitingForMovement;
        movementStartDistance = TotalDistanceTraveled;
        InitializeTrackingPosition();

        if (distanceAfterDialogueB <= 0f)
            StartDialogueC();
    }

    private void StartDialogueC()
    {
        if (currentStep == SequenceStep.DialogueC)
            return;

        currentStep = SequenceStep.DialogueC;

        if (dialogueUI == null)
        {
            Debug.LogError("[StartSequence] DialogueUI reference is missing when starting dialogue C.");
            FinishSequence();
            return;
        }

        if (dialogueStartC == null)
        {
            Debug.LogError("[StartSequence] Dialogue start C is not assigned.");
            FinishSequence();
            return;
        }

        dialogueUI.StartDialogue(dialogueStartC);
    }

    private void BlockInteractions()
    {
        if (playerInteract != null)
            playerInteract.SetInteractionsBlocked(true);
    }

    private void UnblockInteractions()
    {
        if (playerInteract != null)
            playerInteract.SetInteractionsBlocked(false);
    }

    private void HandleInteractionWhileBlocked(IReadOnlyList<IInteractable> _)
    {
        if (interactionBlockedLabel == null || interactionBlockedCanvasGroup == null)
            return;

        const string message = "It's too dark here.";

        if (string.IsNullOrEmpty(message))
            return;

        interactionBlockedLabel.text = message;
        interactionBlockedLabel.color = Color.white;

        if (interactionBlockedRoutine != null)
            StopCoroutine(interactionBlockedRoutine);

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

    private void FinishSequence()
    {
        if (currentStep == SequenceStep.Completed)
            return;

        currentStep = SequenceStep.Completed;

        UnshadeScreen();

        if (dialogueUI != null)
            dialogueUI.DialogueClosed -= HandleDialogueClosed;
        if (skillSelectionUI != null)
            skillSelectionUI.Confirmed -= HandleSkillsConfirmed;
        if (playerInteract != null)
            playerInteract.InteractionWhileBlocked -= HandleInteractionWhileBlocked;

        UnblockInteractions();

        HideInteractionBlockedFeedback();
    }

    private void HideInteractionBlockedFeedback()
    {
        if (interactionBlockedRoutine != null)
        {
            StopCoroutine(interactionBlockedRoutine);
            interactionBlockedRoutine = null;
        }

        if (interactionBlockedCanvasGroup != null)
            interactionBlockedCanvasGroup.alpha = 0f;

        if (interactionBlockedLabel != null)
            interactionBlockedLabel.text = string.Empty;
    }

    private ScreenShading ResolveScreenShading()
    {
        if (screenShading != null)
            return screenShading;

        screenShading = ScreenShading.Instance != null
            ? ScreenShading.Instance
            : FindObjectOfType<ScreenShading>(true);

        if (screenShading == null)
            Debug.LogWarning("[StartSequence] ScreenShading reference is missing.");

        return screenShading;
    }

    private void ShadeScreen()
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

    private void UnshadeScreen()
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
        ShadeScreen();
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

    private void UpdateTotalDistance()
    {
        if (playerTransform == null)
        {
            hasLastTrackedPosition = false;
            return;
        }

        Vector3 currentPosition = playerTransform.position;
        if (!hasLastTrackedPosition)
        {
            lastTrackedPosition = currentPosition;
            hasLastTrackedPosition = true;
            return;
        }

        float frameDistance = Vector3.Distance(currentPosition, lastTrackedPosition);
        if (frameDistance > 0f)
        {
            TotalDistanceTraveled += frameDistance;
            lastTrackedPosition = currentPosition;
        }
    }

    private void InitializeTrackingPosition()
    {
        if (playerTransform == null)
        {
            hasLastTrackedPosition = false;
            return;
        }

        lastTrackedPosition = playerTransform.position;
        hasLastTrackedPosition = true;
    }

    private void ShowFlashlightHint()
    {
        if (hintsPanel == null)
            return;

        hintsPanel.SetHint("Press L to activate flashlight");
        hintsPanel.ShowPanel();
    }

    private void HandleFirstItemAdded()
    {
        if (hintsPanel == null || hasShownInventoryHint)
            return;

        hintsPanel.SetHint(inventoryHintMessage);
        hintsPanel.ShowPanel();
        hasShownInventoryHint = true;
    }
}
