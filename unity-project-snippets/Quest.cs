using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    public string description;
    public bool isActive = false;
    public bool isFinished = false;
    public short currentStep = 0;

    // Is hidden from UI, is to "begin the quest"
    public Checkpoint checkpointToStartQuest;

    [SerializeField]
    public List<Checkpoint> checkpoints;
    public Checkpoint CurrentCheckpoint =>  currentStep >= 0 ? checkpoints[currentStep] : checkpointToStartQuest;

    // Only non-null if quest is active
    public GameObject questUIObject;


    //Optional Parameters DO LATER

    public Requirements requirements;

    [System.Serializable]
    public struct Requirements
    {
        // For character-unlock
        public string unlockCharacter;
        public FriendshipLevel requiredFriendshipLvl;
        public RelationshipLevel requiredRelationshipLvl;
        public Quest requiredQuestToBeFinished;

        // For main story unlock, -1 for no requirement check
    }


    #region Methods

    private void Start()
    {
        checkpointToStartQuest.parentQuest = this;
        foreach (Checkpoint checkpoint in checkpoints)
            checkpoint.parentQuest = this;
    }

    public bool CheckForAvailability()
    {
        if (RelationshipManager.instance.GetFriendshipLevel(requirements.unlockCharacter) >= requirements.requiredFriendshipLvl && RelationshipManager.instance.GetRelationshipLevel(requirements.unlockCharacter) >= requirements.requiredRelationshipLvl)
        {
            if (requirements.requiredQuestToBeFinished == null)
                return true;
            else if (requirements.requiredQuestToBeFinished != null && requirements.requiredQuestToBeFinished.isFinished)
                return true;
            //if (QuestManager.instance.mainStoryline.currentStep >= requirements.mainStoryProgressRequired)
            //{
            //    return true;
            //}
        }
        return false;
    }

    public void Init()
    {
        currentStep = -1;
        isActive = true;
        isFinished = false;
        Debug.Log("Quest " + questName + " initialized!");
        checkpointToStartQuest.Init();
        if(QuestManager.instance.pinnedQuest == this || QuestManager.instance.pinnedQuest == null)
            QuestManager.instance.SetPinnedQuest(this);
    }

    public void Evaluate()
    {
        ActionHandler.QuestUpdated(questName);
        if (currentStep == -1 && checkpointToStartQuest.completed)
        {
            currentStep++;
            CurrentCheckpoint.Init();
            SetQuestUI();
            return;
        }

        if (CurrentCheckpoint.completed)
        {
            currentStep++;
            if (currentStep >= checkpoints.Count)
            {
                Complete();
                return;
            }
            CurrentCheckpoint.Init();
            QuestManager.instance.SetPinnedQuest(this);
        }
    }

    public void Complete()
    {
        isActive = false;
        isFinished = true;
        Debug.Log("Quest " + questName + " finished!");
        QuestManager.instance.QuestFinished(this);
    }

    public void SetQuestUI()
    {
        questUIObject = questUIObject != null ? questUIObject : Instantiate((GameObject)Resources.Load("Prefabs/UI/QuestUIPrefab"), QuestManager.instance.questList.transform);
        questUIObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = questName;
        questUIObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = CurrentCheckpoint.description.Length < 64 ? CurrentCheckpoint.description : CurrentCheckpoint.description.Substring(0, 64) + "...";
    }

    #endregion

}
