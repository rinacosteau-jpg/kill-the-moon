using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using Articy.World_Of_Red_Moon.GlobalVariables;

/// <summary>
/// Simple in-game clock that tracks hours and minutes, updates the UI, and notifies listeners on changes.
/// </summary>
public class GameTime : MonoBehaviour, ILoopResettable {

    [SerializeField] TMP_Text clockText; // UI label to render the current time.
    [SerializeField] private LoopResetInputScript loopReset; // Optional: reference for external reset triggers.

    public static GameTime Instance { get; private set; }

    private const int LoopResetTotalMinutes = 13 * 60;
    private bool loopResetPending;
    private bool hasTriggeredLoopReset;

    private static PropertyInfo articyGameTimeNamespaceAccessor;
    private static PropertyInfo articyGameTimeValueProperty;

    public int Hours { get; set; } = 12;
    public int Minutes { get; set; } = 0;

    public event Action<int, int> OnTimeChanged;

    void Awake() => Instance = this;

    /// <summary>
    /// Advances time by the specified number of minutes, clamping the day to the loop cut-off (13:00).
    /// Invokes <see cref="OnTimeChanged"/> and updates the UI.
    /// </summary>
    public void AddMinutes(int delta) {
        int previousTotalMinutes = (Hours * 60) + Minutes;
        int targetTotalMinutes = previousTotalMinutes + delta;

        if (targetTotalMinutes > LoopResetTotalMinutes)
            targetTotalMinutes = LoopResetTotalMinutes;

        if (targetTotalMinutes < 0)
            targetTotalMinutes = 0;

        Hours = targetTotalMinutes / 60;
        Minutes = targetTotalMinutes % 60;

        int appliedDelta = targetTotalMinutes - previousTotalMinutes;
        SyncWaitForAoRead(appliedDelta);
        Update();
        OnTimeChanged?.Invoke(Hours, Minutes);

        if (!hasTriggeredLoopReset && targetTotalMinutes == LoopResetTotalMinutes)
            HandleLoopResetCutoff();
    }

    public override string ToString() => $"{Hours:D2}:{Minutes:D2}";

    public void Update() {
        SyncArticyGameTime();

        if (clockText != null)
            clockText.text = GameTime.Instance.ToString();

        if (loopResetPending && !IsAnyDialogueOpen())
            PerformLoopReset();
    }

    private void SyncArticyGameTime()
    {
        var globals = ArticyGlobalVariables.Default;
        if (globals == null)
            return;

        if (!TryResolveArticyGameTime(globals))
            return;

        object namespaceInstance;
        try
        {
            namespaceInstance = articyGameTimeNamespaceAccessor.GetValue(globals);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameTime] Failed to access Articy namespace for gameTime: {e.Message}");
            articyGameTimeNamespaceAccessor = null;
            articyGameTimeValueProperty = null;
            return;
        }

        if (namespaceInstance == null)
        {
            articyGameTimeNamespaceAccessor = null;
            articyGameTimeValueProperty = null;
            return;
        }

        int minutesSinceNoon = Mathf.Max(0, (Hours * 60 + Minutes) - (12 * 60));

        try
        {
            if (articyGameTimeValueProperty.PropertyType == typeof(int))
                articyGameTimeValueProperty.SetValue(namespaceInstance, minutesSinceNoon);
            else if (articyGameTimeValueProperty.PropertyType == typeof(float))
                articyGameTimeValueProperty.SetValue(namespaceInstance, (float)minutesSinceNoon);
            else if (articyGameTimeValueProperty.PropertyType == typeof(double))
                articyGameTimeValueProperty.SetValue(namespaceInstance, (double)minutesSinceNoon);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameTime] Failed to push gameTime to Articy: {e.Message}");
            articyGameTimeNamespaceAccessor = null;
            articyGameTimeValueProperty = null;
        }
    }

    private static bool TryResolveArticyGameTime(ArticyGlobalVariables globals)
    {
        if (articyGameTimeNamespaceAccessor != null && articyGameTimeValueProperty != null)
            return true;

        if (globals == null)
            return false;

        const BindingFlags namespaceFlags = BindingFlags.Instance | BindingFlags.Public;
        const BindingFlags variableFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

        foreach (var nsProp in globals.GetType().GetProperties(namespaceFlags))
        {
            if (!nsProp.CanRead)
                continue;

            var nsType = nsProp.PropertyType;
            if (nsType == null)
                continue;

            var gameTimeProp = nsType.GetProperty("gameTime", variableFlags);
            if (gameTimeProp == null || !gameTimeProp.CanWrite)
                continue;

            var propertyType = gameTimeProp.PropertyType;
            if (propertyType != typeof(int) && propertyType != typeof(float) && propertyType != typeof(double))
                continue;

            articyGameTimeNamespaceAccessor = nsProp;
            articyGameTimeValueProperty = gameTimeProp;
            return true;
        }

        return false;
    }

    // Increment RCNT.waitForAoRead alongside time when active; normalize -1 -> 1; stop after reaching threshold.
    private static void SyncWaitForAoRead(int deltaMinutes)
    {
        var rcnt = ArticyGlobalVariables.Default?.RCNT;
        if (rcnt == null) return;

        if (rcnt.waitForAoRead == -1)
            rcnt.waitForAoRead = 1;

        if (rcnt.waitForAoRead != 0 && rcnt.waitForAoRead < 21)
        {
            rcnt.waitForAoRead += deltaMinutes;
            if (rcnt.waitForAoRead < 0) rcnt.waitForAoRead = 0; // guard
        }
    }

    private void HandleLoopResetCutoff()
    {
        hasTriggeredLoopReset = true;

        if (IsAnyDialogueOpen())
        {
            loopResetPending = true;
            return;
        }

        PerformLoopReset();
    }

    private bool IsAnyDialogueOpen()
    {
        var dialogues = FindObjectsOfType<DialogueUI>(true);
        foreach (var dialogue in dialogues)
        {
            if (dialogue != null && dialogue.IsDialogueOpen)
                return true;
        }

        return false;
    }

    private void PerformLoopReset()
    {
        loopResetPending = false;

        if (loopReset != null)
        {
            loopReset.LoopReset();
        }
        else
        {
            LoopResetInputScript.TryLoopReset();
        }
    }

    public void OnLoopReset()
    {
        loopResetPending = false;
        hasTriggeredLoopReset = false;
        Update();
    }
}
