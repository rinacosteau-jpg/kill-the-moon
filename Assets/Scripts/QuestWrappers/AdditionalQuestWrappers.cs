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
        SetStageDescription(1, "� ������ ������� �������, ���� ����� ����� ���������. ������ ���� �� 13:45, ����� �� ������.");
        SetStageDescription(2, "� ������ ������� ���������, ���� ����� ��������� �������. ������ ���� �� 13:45, ����� �� ������.");
        SetStageDescription(3, "������� ��������� �� �����. ����� ���������� ����� ����� ������, �� ����� � ���� �����?");
        SetStageDescription(4, "��������� � ����! ������� ��?");
        SetStageDescription(5, "��������� � ����! � ��� � �����-�� ������� ���� �����.");
        SetStageDescription(6, "� ���� ����� ���������� ������� ���������. � ��� ����� ���� ����� ������� ������ �����...");

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
