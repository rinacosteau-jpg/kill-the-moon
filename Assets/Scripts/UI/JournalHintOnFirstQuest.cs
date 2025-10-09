using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows a journal hint the first time any quest becomes active.
/// </summary>
public class JournalHintOnFirstQuest : MonoBehaviour
{
    [SerializeField] private HintsPanelController hintsPanel;
    [SerializeField] private string journalHintMessage = "Use J to open Journal, Esc to close";

    private readonly HashSet<string> activeQuestNames = new HashSet<string>(StringComparer.Ordinal);
    private bool hasShownHint;

    private void Awake()
    {
        if (hintsPanel == null)
            hintsPanel = GetComponent<HintsPanelController>();
    }

    private void OnEnable()
    {
        QuestManager.OnQuestChanged += HandleQuestChanged;
        RefreshActiveQuestCache();

        if (!hasShownHint && activeQuestNames.Count > 0)
            ShowHint();
    }

    private void OnDisable()
    {
        QuestManager.OnQuestChanged -= HandleQuestChanged;
    }

    private void HandleQuestChanged(QuestManager.Quest quest)
    {
        if (quest == null || hasShownHint)
            return;

        bool wasActive = activeQuestNames.Contains(quest.Name);

        if (quest.State == QuestState.Active)
        {
            activeQuestNames.Add(quest.Name);

            if (!wasActive && activeQuestNames.Count == 1)
                ShowHint();
        }
        else if (wasActive)
        {
            activeQuestNames.Remove(quest.Name);
        }
    }

    private void RefreshActiveQuestCache()
    {
        activeQuestNames.Clear();

        foreach (var quest in QuestManager.GetAllQuests())
        {
            if (quest != null && quest.State == QuestState.Active)
                activeQuestNames.Add(quest.Name);
        }
    }

    private void ShowHint()
    {
        hasShownHint = true;

        if (hintsPanel == null)
        {
            Debug.LogWarning("[JournalHintOnFirstQuest] Hints panel reference is missing.");
            return;
        }

        hintsPanel.SetHint(journalHintMessage);
        hintsPanel.ShowPanel();
    }
}
