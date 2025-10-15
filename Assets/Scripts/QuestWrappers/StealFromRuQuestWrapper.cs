using Articy.World_Of_Red_Moon.GlobalVariables;

public sealed class StealFromRuQuestWrapper : QuestWrapper
{
    public StealFromRuQuestWrapper() : base("stealFromRu")
    {
        SetStageDescription(1, "Ratko asked me to steal some kind of magical artifact, and I agreed. I don’t know why I agreed.");
        SetStageDescription(2, "Ratko asked me to steal some kind of magical artifact, and I agreed. I don’t know why I agreed — and now no one even remembers that I did. Well, I can still try to steal it. For some reason.");
        SetStageDescription(3, "I failed to ‘claim’ the artifact. As expected. If I don’t want her to tear me apart, I’d better stop trying. And I don’t want to.");
        SetStageDescription(4, "I failed to ‘claim’ the artifact, but... I can always try again in the next loop. (I still don’t understand why I’m doing this.)");
        SetStageDescription(5, "I’ve got the cubes! Whatever they are. Time to return to Ratko and discuss the reward. Hopefully, he’ll tell me something useful.");
        SetStageDescription(6, "I’ve got the cubes! Whatever they are. I could go back to Ratko and discuss the reward... Except he won’t remember anything. Fine, I’ll think of something. No way I did all this for nothing.");
        SetStageDescription(7, "Now Ratko answers my questions. Well, the ones he wants to answer, anyway. It’s better than nothing.");
        SetStageDescription(8, "If I want Ratko to help me again, I’ll have to bring him those cubes again. Well, at least I know how to get them now.");
        AddStagesToAdvanceOnLoopReset(1, 7);
        MarkStageAsFailed(3, 4);
        MarkStageAsCompleted(7);
    }

    public override int ProcessStageFromArticy(QuestManager.Quest quest, int stage)
    {
        if (stage == 3)
        {
            var ps = ArticyGlobalVariables.Default?.PS;
            if (ps != null && ps.loopCounter > 0)
                return 4;
        }

        if (stage == 5)
        {
            if (quest?.Stage == 1)
                return 5;

            return 6;
        }

        return base.ProcessStageFromArticy(quest, stage);
    }

    public override void OnLoopReset(QuestManager.Quest quest)
    {
        if (quest == null)
            return;

        bool wasFailed = quest.State == QuestState.Failed;

        base.OnLoopReset(quest);

        if (wasFailed)
        {
            quest.Stage = 2;
            quest.State = QuestState.Active;
        }
    }
}
