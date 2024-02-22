using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class QuestManager : MonoBehaviour
{

    public static QuestManager Instance;
    public PinnedQuestUI questUI;

    private List<Quest> allQuests;

    private List<Quest> dormantQuests = new();
    private List<Quest> activeQuests = new();
    private List<Quest> completeQuests = new();

    [SerializeField]
    private Quest initialQuest;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;



        GetComponentInParent<ManagerInitializer>().InitGameManagers += Init;
    }

    private void Init()
    {
        allQuests = new List<Quest>();

        Addressables.LoadAssetsAsync<Quest>("Quest", quest => allQuests.Add(quest));

        dormantQuests = allQuests;

        foreach (Quest quest in allQuests)
        {
            quest.QuestActivated += OnQuestActivated;
            quest.QuestFinished += OnQuestFinished;
            quest.Init();
            //quest.CheckPrerequisites();
        }

        initialQuest.Awaken();
    }

    private void OnQuestActivated(Quest quest)
    {
        dormantQuests.Remove(quest);
        activeQuests.Add(quest);
    }

    private void OnQuestFinished(Quest quest)
    {
        activeQuests.Remove(quest);
        completeQuests.Add(quest);
    }
}