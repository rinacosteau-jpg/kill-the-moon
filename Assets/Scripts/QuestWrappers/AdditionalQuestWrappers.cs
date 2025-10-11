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
        SetStageDescription(1, "Я должна отвлечь Теодора, пока Ратко крадёт револьвер. Начать надо до 13:45, иначе не успеем.");
        SetStageDescription(2, "Я должна отвлечь Теодора, пока Ратко крадёт револьвер. Начать надо до 13:45, иначе не успеем.");
        SetStageDescription(3, "Вернуть револьвер не вышло. Можно попытаться снова после сброса, но какой в этом смысл?");
        SetStageDescription(4, "Револьвер у меня! Надолго ли?");
        SetStageDescription(5, "Револьвер у меня! А ещё я зачем-то вернула вещи Ратко.");
        SetStageDescription(6, "Я могу снова попытаться вернуть револьвер.");
    }
}
