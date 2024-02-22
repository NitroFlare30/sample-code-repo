using System.Collections;
using UnityEngine;
using System;
using TMPro;
using Ink.Runtime;
using System.Collections.Generic;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueInkInterpretor dialogueInterpretor;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private DialogueElementsHandler dialogueElementsHandler;

    private const string SPEAKER_TAG = "speaker";
    private const string EMOTION_TAG = "emotion";
    private const string RELATIONSHIP_POINTS_TAG = "relation_pts"; // Ink Format -- #relation_pts:Nikki/5 or #relation_pts:Jet/-5

    public Action<string> OnStandardDialogueFinished;
    public Action OnCutsceneDialogueFinished;
    public Action<string> OnQuestCharacterInteract;

    [SerializeField] private Story currentStory;
    [SerializeField] private Character currentCharacter;

    public bool IsCurrentlyInDialogue => dialogueElementsHandler.DialoguePanel.dialogueUI.activeInHierarchy;
    public bool IsCurrentlyAnimatingText => dialogueElementsHandler.ActiveTextAnimator != null;
    public bool IsCurrentlyAwaitingChoice = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        dialogueElementsHandler.DeactivateDialogueUI();
        currentStory = null;
    }

    public async Task StartGenericDialogue(string characterName)
    {
        currentStory = dialogueInterpretor.GetStory(characterName, Dialogue_Type.Generic);
        dialogueInterpretor.SetRandomLines(currentStory);
        await AdvanceDialogue();
    }

    public async Task StartQuestDialogue(string questName, Cutscene cutscene)
    {
        if (currentStory == null)
        {
            currentStory = dialogueInterpretor.GetStory(questName, Dialogue_Type.Quest);
            dialogueInterpretor.SetCutsceneLines(currentStory, cutscene);
        }

        await AdvanceDialogue();
    }

    public async Task StartCutsceneDialogue(string questName, Cutscene cutscene)
    {
        if (currentStory == null)
        {
            currentStory = dialogueInterpretor.GetStory(questName, Dialogue_Type.Cutscene);
            if (currentStory == null)
                Debug.LogError($"Couldn't find cutscene story for: {questName}");
            dialogueInterpretor.SetCutsceneLines(currentStory, cutscene);
        }

        await AdvanceDialogue();
    }

    public async Task StartHeartEventDialogue(string characterName, Cutscene cutscene)
    {
        if (currentStory == null)
        {
            currentStory = dialogueInterpretor.GetHeartEventStory(characterName, RelationshipManager.Instance.GetRelationshipLevel(characterName));
            if (currentStory == null)
                Debug.LogError($"Couldn't find cutscene story for: {characterName}, {cutscene.inkPathName}");
            dialogueInterpretor.SetCutsceneLines(currentStory, cutscene);
        }

        await AdvanceDialogue();
    }

    public async Task StartSpecificDialogue(string characterName, string pathName)
    {
        currentStory = dialogueInterpretor.GetStory(characterName, Dialogue_Type.Miscellaneous);
        dialogueInterpretor.SetMiscellaneousLines(currentStory, pathName);
        await AdvanceDialogue();
    }

    public async Task AdvanceDialogue()
    {
        // Don't allow text advance with pending choice
        if (IsCurrentlyAwaitingChoice)
            return;

        // Allow line-skipping
        if (IsCurrentlyAnimatingText)
        {
            dialogueElementsHandler.FinishTextAnimation(currentStory.currentText);
            return;
        }

        if (currentStory.canContinue)
        {
            DateAndTimeManager.Instance.IsTimeTicking = false;
            string newTextToDisplay = currentStory.Continue();
            await SetDialogueElements(currentStory.currentTags);
            dialogueElementsHandler.StartAnimatingText(newTextToDisplay);
            if (currentStory.currentChoices.Any())
            {
                // Enable choice UI
                dialogueElementsHandler.ActivateChoiceUI();
                dialogueElementsHandler.DisplayChoices(currentStory.currentChoices.Select(x => x.text).ToList());
                IsCurrentlyAwaitingChoice = true;
            }
        }
        else
            ExitDialogue();
    }

    public void ExitDialogue()
    {
        ResetInkElements();
        currentStory = null;
        currentCharacter = null;
        DateAndTimeManager.Instance.IsTimeTicking = true;
        PlayerInputController.Instance.CanMove = true;

        OnCutsceneDialogueFinished?.Invoke();

        PlayerInputController.Instance.EnableInGameControls();
        dialogueElementsHandler.DeactivateDialogueUI();
        UIManager.Instance.EnableInGameUI();

        if (dialogueElementsHandler.DialoguePanel.skipIntroPanel != null)
            Destroy(dialogueElementsHandler.DialoguePanel.skipIntroPanel);

        if (CutsceneManager.Instance.isInCutscene)
            CutsceneManager.Instance.AdvanceCutscene();
        
    }

    public async Task MakeDialogueChoice(int index)
    {
        IsCurrentlyAwaitingChoice = false;
        dialogueElementsHandler.DeactivateChoiceUI();
        currentStory.ChooseChoiceIndex(index);
        await AdvanceDialogue();
    }

    #region Helper Methods
    private void ResetInkElements()
    {
        currentStory?.ResetCallstack();
        currentStory?.ResetState();
        currentStory = null;
    }

    public void EnableDialogueElements() =>
        dialogueElementsHandler.ActivateDialogueUI();

    public void DisableDialogueElements() =>
        dialogueElementsHandler.DeactivateDialogueUI();

    private async Task SetDialogueElements(List<string> tags)
    {
        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
                Debug.LogError("Tag could not be parsed: " + tag);
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch(tagKey)
            {
                case SPEAKER_TAG:
                    string speakerName = "";
                    if (tagValue == "none")
                        speakerName = "";
                    else if (tagValue == "*player*")
                        speakerName = GlobalFunctions.PlayerName;
                    else { 
                        speakerName = tagValue;
                        currentCharacter = CharacterManager.Instance.GetCharacter(speakerName);
                    }
                    await dialogueElementsHandler.SetPortraitComponent(speakerName);
                    break;

                case EMOTION_TAG:
                    if (tagValue != "none")
                        await currentCharacter.SetExpression(tagValue);
                    break;

                case RELATIONSHIP_POINTS_TAG:
                    List<string> relInfo = tagValue.Split('/').ToList();
                    if (relInfo.Count != 2)
                    {
                        Debug.LogError($"Unable to parse relationship pts from dialogue: {relInfo}");
                        return;
                    }
                    string targetCharacter = relInfo[0];
                    short relPtsToAdd = short.Parse(relInfo[1]);
                    RelationshipManager.Instance.GetRelation(targetCharacter).AddRelationPoints(relPtsToAdd);
                    Debug.Log($"Added {relPtsToAdd} pts to {targetCharacter}");
                    break;

                default:
                    Debug.LogWarning("Tag not recognized: " + tag);
                    break;
            }
        }
    }
    #endregion
}