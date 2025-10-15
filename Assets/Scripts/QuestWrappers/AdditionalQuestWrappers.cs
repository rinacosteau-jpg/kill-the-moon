public sealed class BackgroundCheckQuestWrapper : QuestWrapper
{
    public BackgroundCheckQuestWrapper() : base("BackgroundCheck")
    {
    }
}

public sealed class FindMemoriesQuestWrapper : QuestWrapper
{
    public FindMemoriesQuestWrapper() : base("FindMemories")
    {
    }
}

public sealed class AdvertiseQuestWrapper : QuestWrapper
{
    public AdvertiseQuestWrapper() : base("advertise")
    {
    }
}

public sealed class GetArtefactQuestWrapper : QuestWrapper
{
    public GetArtefactQuestWrapper() : base("getArtefact")
    {
    }
}

public sealed class PreventMurderAttemptQuestWrapper : QuestWrapper
{
    public PreventMurderAttemptQuestWrapper() : base("preventMurderAttempt")
    {
    }
}

public sealed class GetGunQuestWrapper : QuestWrapper
{
    public GetGunQuestWrapper() : base("getGun")
    {
        SetStageDescription(1, "I need to distract Theodor while Ratko steals the revolver. We have to start before 13:45, or we won't make it in time.");
        SetStageDescription(2, "I need to steal the revolver while Ratko distracts Theodor. We have to start before 13:45, or we won't make it in time.");
        SetStageDescription(3, "Returning the revolver didn’t work. I could try again after the reset… but what’s the point?");
        SetStageDescription(4, "The revolver is mine! But for how long?");
        SetStageDescription(5, "The revolver is mine! And for some reason, I gave Ratko’s stuff back.");
        SetStageDescription(6, "I can try to get the revolver back again. And for that, I’ll have to help Ratko once more...");

        MarkStageAsFailed(3);
        MarkStageAsCompleted(4,5);
    }

    public override void OnLoopReset(QuestManager.Quest quest) {
        if (quest == null)
            return;

        if (quest.Stage != 6 && quest.Stage!=0) {
            quest.Stage = 6;
            quest.State = QuestState.Active;
        }
    }
}
