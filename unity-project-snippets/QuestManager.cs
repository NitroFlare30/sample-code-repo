using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public Dictionary<string, Quest> unactivatedQuests = new Dictionary<string, Quest>();
    public Dictionary<string, Quest> availableQuests = new Dictionary<string, Quest>();
    public Dictionary<string, Quest> finishedQuests = new Dictionary<string, Quest>();

    public GameObject questList;
    public List<GameObject> questUIHolder = new List<GameObject>();

    public List<QuestData> questData = new List<QuestData>();

    public Quest mainStoryline;
    public Quest pinnedQuest;

    public bool skipIntro = false;

    [SerializeField]
    private GameObject _pinnedQuestUI;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Waiter());
    }

    public void SetQuestState(Quest[] quests)
    {
        if (SceneController.instance.newGame)
        {
            foreach (Quest quest in quests)
            {
                quest.currentStep = -1;
                quest.isActive = false;
                quest.isFinished = false;
            }
        }
        else
        {
            for (int i = 0; i < quests.Length; i++)
            {
                quests[i].currentStep = (short)questData[i].step;
                quests[i].isActive = questData[i].active;
                quests[i].isFinished = questData[i].finished;
                if(quests[i].isActive)
                {
                    if(pinnedQuest == null)
                    {
                        pinnedQuest = quests[i];
                        SetPinnedQuest(quests[i]);
                    }
                    quests[i].checkpoints[quests[i].currentStep].Init();
                    quests[i].SetQuestUI();
                    questUIHolder.Add(quests[i].questUIObject);
                }
            }
        }
        
    }

    public IEnumerator Waiter()
    {
        //CutsceneManager.instance.nonspeechUI.SetActive(false);
        yield return new WaitForSeconds(0.1f);

        
        unactivatedQuests = FindAllQuests();
        if (!skipIntro)
        {
            // MOVE TO SLEEPCHECK eventually
            InitializeAvailableQuests();
        }

    }

    public Dictionary<string, Quest> FindAllQuests()
    {
        Dictionary<string, Quest> foundQuests = new Dictionary<string, Quest>();
        Quest[] allQuests = Resources.LoadAll<Quest>("Quests");

        // DEBUG FORCE RESET QUEST
        SetQuestState(allQuests);

        foreach (Quest quest in allQuests)
        {
            // Place to add qualifiers for quests to load
            if (!quest.isFinished)
            {
                foundQuests.Add(quest.questName, quest);
                // Debug.Log("Found quest " + quest.questName);
            }
        }
        Debug.Log("Found " + foundQuests.Count + " quests");
        return foundQuests;
    }

    public void InitializeAvailableQuests()
    {
        List<string> questsToRemove = new List<string>();
        foreach (Quest quest in unactivatedQuests.Values)
        {
            if (quest.CheckForAvailability())
            {
                availableQuests.Add(quest.questName, quest);
                quest.questUIObject = Instantiate((GameObject)Resources.Load("Prefabs/UI/QuestUIPrefab"), questList.transform);
                quest.SetQuestUI();
                questUIHolder.Add(quest.questUIObject);
                questsToRemove.Add(quest.questName);
            }
        }

        foreach (string str in questsToRemove)
        {
            unactivatedQuests.Remove(str);
        }

        foreach (Quest quester in availableQuests.Values)
        {
            if (!quester.isFinished && !quester.isActive && quester.questName != "TerisQuest")
                quester.Init();
        }
    }

    public void QuestFinished(Quest quest)
    {
        availableQuests.Remove(quest.questName);
        questUIHolder.Remove(quest.questUIObject);
        Destroy(quest.questUIObject);
        finishedQuests.Add(quest.questName, quest);
        InitializeAvailableQuests();
        if (pinnedQuest == quest)
            pinnedQuest = null;
        if (availableQuests.Count != 0)
            foreach(Quest q in availableQuests.Values)
                if (q.isActive && !q.isFinished)
                {
                    SetPinnedQuest(q);
                    break;
                }
    }

    public Quest GetQuest(string questName)
    {
        Quest quest;
        if (availableQuests.TryGetValue(questName, out quest))
        {
            Debug.Log("Quest " + questName + " is available but not yet started");
            return quest;
        }
        else if (unactivatedQuests.TryGetValue(questName, out quest))
        {
            Debug.Log("Quest " + questName + " not yet available");
            return quest;
        }
        else if (finishedQuests.TryGetValue(questName, out quest))
        {
            Debug.Log("Quest " + questName + " has already been completed");
            return quest;
        }
        Debug.LogWarning("Quest " + questName + " not found!");
        return null;
    }

    public void SetPinnedQuest(Quest quest)
    {
        if (pinnedQuest != null)
        {
            pinnedQuest = quest;
            _pinnedQuestUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = quest.questName;
            _pinnedQuestUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = quest.CurrentCheckpoint.description;
        }
    }

    public void QuestToDict(List<Quest> avaliable, List<Quest> finished)
    {
        foreach(Quest quest in avaliable)
        {
            availableQuests[quest.questName] = quest;
        }
        foreach (Quest quest in finished)
        {
            finishedQuests[quest.questName] = quest;
        }
    }

    public List<QuestData> GrabQuestData()
    {
        Quest[] quests = Resources.LoadAll<Quest>("Quests");
        questData.Clear();
        foreach(Quest quest in quests)
        {
            questData.Add(new QuestData(quest.currentStep, quest.isActive, quest.isFinished));
        }
        return questData;
    }

}
[System.Serializable]
public class QuestData
{
    public int step;
    public bool active;
    public bool finished;

    public QuestData(int step, bool active, bool finished)
    {
        this.step = step;
        this.active = active;
        this.finished = finished;
    }
}