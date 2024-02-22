using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/New Quest", order = 0)]
public class Quest : ScriptableObject
{
    #region Quest Info

    [Serializable]
    public class Info
    {
        public string name;
        public string description;
        public string startingCharacterName;
    }
    [Header("Quest Info")] public Info info;

    [field: SerializeReference]
    public List<Checkpoint> Checkpoints;

    [Serializable]
    public class Reward
    {
        public int rewardMoney;
        public List<QuantifiedItem> rewardItems;
    }
    public Reward reward;

    [field: SerializeField]
    public Prerequisites prerequisites;
    [Serializable]
    public class Prerequisites
    {
        public Quest questRequirement;
        public int questProgressRequirement;
        [Header("")]
        public string questGiverCharacterName;
        public short relationshipLevelNeeded;
        public Item triggerItem;

        public bool CheckRequirements()
        {
            if (questRequirement != null && (questRequirement.status == QuestStatus.Dormant || questRequirement.GetCurrentStage() < questProgressRequirement))
                return false;

            // Bypass Friendship Checks if empty - leave after other reqs
            if (string.IsNullOrEmpty(questGiverCharacterName))
                return true;

            if (RelationshipManager.Instance.GetRelationshipLevel(questGiverCharacterName) < relationshipLevelNeeded)
                return false;
            return true;
        }
    }

    #endregion

    [Header("Status")]
    [SerializeField]
    private QuestStatus status;
    [SerializeField]
    private int currentStage;

    public Action<Quest> QuestActivated;
    public Action<Quest> QuestFinished;

    public Checkpoint CurrentCheckpoint => Checkpoints[currentStage];
    public QuestStatus GetCurrentStatus() => status;
    public int GetCurrentStage() => currentStage;

    public void Init()
    {
        status = QuestStatus.Dormant;
        currentStage = -1;
    }

    public void CheckPrerequisites()
    {
        if (prerequisites.CheckRequirements())
            Awaken();
    }

    /// <summary>
    /// When the quest becomes active
    /// </summary>
    public void Awaken()
    {
        status = QuestStatus.Active;
        currentStage = 0;
        CurrentCheckpoint.Init();
        CurrentCheckpoint.CheckpointUpdated += OnCheckpointUpdated;
        CurrentCheckpoint.CheckpointSatisfied += OnCheckpointSatisfied;
        QuestActivated?.Invoke(this);
        QuestUI.Instance.AddToQuestList(this);
        if (string.IsNullOrEmpty(QuestManager.Instance.questUI.PinnedQuestName))
        {
            QuestManager.Instance.questUI.SetPinnedQuest(this);
            QuestUI.Instance.GetQuestItemUI(this).Pin();
        }
            
    }

    private void OnCheckpointSatisfied()
    {
        if (currentStage >= Checkpoints.Count)
        {
            status = QuestStatus.Completed;
            currentStage = -1;
            QuestFinished?.Invoke(this);
            QuestUI.Instance.RemoveFromQuestList(this);
        }
        else
        {
            currentStage++;
            CurrentCheckpoint.Init();
            if (QuestManager.Instance.questUI.PinnedQuestName == info.name)
                QuestManager.Instance.questUI.SetPinnedQuest(this);
        }
    }

    private void OnCheckpointUpdated()
    {
        //TODO: rework pinned quest ui objects
        //QuestManager.Instance.questUI.SetPinnedQuest(this);
    }

    public abstract class Checkpoint : ScriptableObject
    {
        public abstract void Init();
        public abstract void Evaluate();
        public abstract void Complete();
        public abstract string GenerateDescription();
        public abstract bool IsSatisfied();

        public Action CheckpointUpdated;
        public Action CheckpointSatisfied;
    }

    public enum QuestStatus
    {
        Dormant, // Reqs NOT met, not started
        Active,
        Completed
    }
}