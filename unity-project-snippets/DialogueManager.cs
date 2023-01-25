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

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    private CharacterManager characterManager;

    public bool isInDialogue = false;
    public float _textAdditionsPerSecond = 60;
    [SerializeField]
    private DialoguePanelElements dialoguePanel;


    [Header("Choice UI")]
    [SerializeField]
    private GameObject choicePanel;
    [SerializeField]
    private List<GameObject> choiceButtons;

    private Story currentStory;
    private Character currentCharacter;
    private Coroutine activeTextAnimator;
    

    private const string SPEAKER_TAG = "speaker";
    private const string EMOTION_TAG = "emotion";

    

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
        characterManager = GetComponent<CharacterManager>();
        dialoguePanel.speechPanel.SetActive(false);
        dialoguePanel.characterPanel.SetActive(false);
        dialoguePanel.skipIntroPanel.SetActive(false);
        choicePanel.SetActive(false);
        //choicePanel.SetActive(false);
    }

    public Story FindQuestStory(string cutsceneName) => new(Resources.Load<TextAsset>("Dialogues/Quests/" + cutsceneName + "/" + cutsceneName).text);

    // For Monologues
    public void EnterDialogueMode(Character character)
    {
        isInDialogue = true;
        PlayerInputController.Instance.CanMove = false;
        currentCharacter = character;
        currentCharacter.gameObject.GetComponent<NPCMovement>().isTalking = true;
        if (activeTextAnimator != null && dialoguePanel.speakerText.text != currentStory.currentText)
        {
            FinishTextAnimation(currentStory.currentText);
            return;
        }
        if (currentStory == null)
        {
            string monologueName = RandomMonologueName(character);
            currentStory.ResetState();
            currentStory.ResetCallstack();
            currentStory.ChoosePathString(monologueName);
        }
        if (currentStory.canContinue)
        {
            activeTextAnimator = StartCoroutine(AnimateText(currentStory.Continue()));
            currentCharacter.GetComponent<Animator>().SetFloat("LastMoveX", -1 * PlayerInputController.Instance.moveDirection.x);
            currentCharacter.GetComponent<Animator>().SetFloat("LastMoveY", -1 * PlayerInputController.Instance.moveDirection.y);
            currentCharacter.GetComponent<Animator>().SetBool(currentCharacter.characterName + "Moving", false);
            TimeManager.instance.pauseTime = true;
            SetDialogueElements(currentStory.currentTags);
            dialoguePanel.characterPanel.SetActive(true);
            dialoguePanel.speechPanel.SetActive(true);
        }
        else
            ExitDialogueMode();
    }

    // For Dialogues/Cutscenes
    public void EnterDialogueMode(string cutsceneName, string dialogueName)
    {
        isInDialogue = true;
        PlayerInputController.Instance.CanMove = false;
        if (cutsceneName == "Intro")
            dialoguePanel.skipIntroPanel.SetActive(true);

        if (activeTextAnimator != null && dialoguePanel.speakerText.text != currentStory.currentText)
        {
            FinishTextAnimation(currentStory.currentText);
            return;
        }
        if (dialogueName != "")
        {
            currentStory = FindQuestStory(cutsceneName);
            currentStory.ChoosePathString(dialogueName);
        }
        if (currentStory.canContinue)
        {
            activeTextAnimator = StartCoroutine(AnimateText(currentStory.Continue()));
            TimeManager.instance.pauseTime = true;
            SetDialogueElements(currentStory.currentTags);
            dialoguePanel.characterPanel.SetActive(true);
            dialoguePanel.speechPanel.SetActive(true);
        }
        else if (currentStory.currentChoices.Count > 0)
            return;
        else
            ExitDialogueMode();
    }

    public void ExitDialogueMode()
    {
        if (currentCharacter != null)
            currentCharacter.gameObject.GetComponent<NPCMovement>().isTalking = false;
        currentCharacter = null;
        activeTextAnimator = null;
        dialoguePanel.speechPanel.SetActive(false);
        dialoguePanel.characterPanel.SetActive(false);
        dialoguePanel.speakerText.text = "";
        currentStory.ResetCallstack();
        currentStory.ResetState();
        currentStory = null;
        TimeManager.instance.pauseTime = false;
        PlayerInputController.Instance.CanMove = true;
        isInDialogue = false;
        if (CutsceneManager.instance.isInCutscene)
            CutsceneManager.instance.EndCutscene();
        dialoguePanel.skipIntroPanel.SetActive(false);
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        if (currentChoices.Count == 0)
        {
            choicePanel.SetActive(false);
            return;
        }
        choicePanel.SetActive(true);
        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choiceButtons[index].SetActive(true);
            choiceButtons[index].GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            index++;
        }
        for (int i = index; i < currentChoices.Count; i++) 
            choiceButtons[i].SetActive(false);
        //StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choiceButtons[0]);
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        choicePanel.SetActive(false);
        EnterDialogueMode("","");
    }

    private IEnumerator AnimateText(string text)
    {
        dialoguePanel.speakerText.text = "";
        text = text.Replace("*player*", GlobalFunctions.PlayerName).Trim();
        while (!dialoguePanel.speakerText.text.Equals(text))
        {
            dialoguePanel.speakerText.text += text[dialoguePanel.speakerText.text.Length];
            yield return new WaitForSeconds(1/_textAdditionsPerSecond);
        }
        FinishTextAnimation(text);
    }

    private void FinishTextAnimation(string text)
    {
        text = text.Replace("*player*", GlobalFunctions.PlayerName).Trim();
        if (activeTextAnimator != null) StopCoroutine(activeTextAnimator);
        activeTextAnimator = null;
        dialoguePanel.speakerText.text = text;
        DisplayChoices();
    }

    // LATER: ACCOUNT FOR WEATHER, RELATIONS, ETC
    private string RandomMonologueName(Character character)
    {
        currentStory = new(Resources.Load<TextAsset>("Dialogues/" + character.characterName + "/" + character.characterName).text);
        if (currentStory == null)
            Debug.LogError(character.characterName + " does not have an associated ink file!");
        return GetRandomKnotName(currentStory.ToJson());
    }

    private static string GetRandomKnotName(string json)
    {
        List<string> keyList = new List<string>();
        JObject storyText = JObject.Parse(json);
        IList<JToken> results = storyText["root"]?.Children().ToList();
        if (results != null && results.Count >= 3)
        {
            Dictionary<string, object> knotDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(results[2].ToString());
            keyList = new List<string>(knotDictionary.Keys);
        }
        var random = new System.Random();
        return keyList[random.Next(keyList.Count)];
    }

    private void SetDialogueElements(List<string> tags)
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
                    if (tagValue == "none")
                    {
                        dialoguePanel.speakerName.text = "";
                        GlobalFunctions.DisableAllSiblingsExcept(dialoguePanel.characterPanel);
                        dialoguePanel.characterPanel.SetActive(false);
                    }
                    else if (tagValue == "*player*")
                    {
                        dialoguePanel.speakerName.text = GlobalFunctions.PlayerName;
                        SetCharacterPanelActive(false);
                    }
                    else if (characterManager.GetCharacter(tagValue) == null)
                        dialoguePanel.speakerName.text = tagValue;
                    else
                    {
                        dialoguePanel.speakerName.text = tagValue;
                        currentCharacter = characterManager.GetCharacter(tagValue);
                        currentCharacter.characterPortrait = currentCharacter.characterPortrait != null ? currentCharacter.characterPortrait : Instantiate(Resources.Load<GameObject>("Characters/_Prefabs" + "/" + currentCharacter.characterName + "[Character]"), dialoguePanel.characterPanel.transform);
                        currentCharacter.characterPortrait.SetActive(true);
                        Debug.Log(currentCharacter.characterName + ": " + tagValue);
                        SetPortrait(currentCharacter);
                    }
                    break;
                case EMOTION_TAG:
                    if (tagValue != "none")
                        currentCharacter.SetExpression(tagValue);
                    break;
                default:
                    Debug.LogWarning("Tag not recognized: " + tag);
                    break;
            }
        }
    }

    private void SetPortrait(Character character)
    {
        GlobalFunctions.DisableAllSiblingsExcept(dialoguePanel.characterPanel, character.characterPortrait);
        dialoguePanel.characterPanel.SetActive(true);
        character.CharacterBodyRenderer.gameObject.SetActive(true);
        character.CharacterExpressionRenderer.gameObject.SetActive(true);
    }

    public void SetSpeechPanelActive(bool active) => dialoguePanel.speechPanel.SetActive(active);
    public void SetCharacterPanelActive(bool active) => dialoguePanel.characterPanel.SetActive(active);

    [Serializable]
    private struct DialoguePanelElements
    {
        // Parent Elements
        public GameObject characterPanel;
        public GameObject speechPanel;
        public GameObject skipIntroPanel;

        // Child Elements
        public TextMeshProUGUI speakerName;
        public TextMeshProUGUI speakerText;
    }

}