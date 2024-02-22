using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class DialogueElementsHandler : MonoBehaviour
{
    [Serializable]
    public struct DialoguePanelElements
    {
        // Parent Elements
        public GameObject dialogueUI;
        public GameObject characterPanel;
        public GameObject speechPanel;
        public GameObject skipIntroPanel;

        // Child Elements
        public TextMeshProUGUI speakerName;
        public TextMeshProUGUI speakerText;
    }
    [field: SerializeField]
    public DialoguePanelElements DialoguePanel { get; private set; }
    [Serializable]
    public struct ChoicePanelElements
    {
        public GameObject choicePanel;
        public List<GameObject> choiceList;
    }
    [field: SerializeField]
    public ChoicePanelElements ChoicePanel { get; private set; }

    [SerializeField] private float _charAdditionsPerSecond;

    public Coroutine ActiveTextAnimator { get; private set; }

    public void ActivateDialogueUI()
    {
        ResetDialogueElements();
        UIManager.Instance.EnableDialogueUI();
        DialoguePanel.dialogueUI.SetActive(true);
    }
    public void DeactivateDialogueUI()
    {
        DialoguePanel.dialogueUI.SetActive(false);
        ResetDialogueElements();
    }

    public void ActivateChoiceUI()
    {
        ResetChoiceElements();
        foreach (GameObject choice in ChoicePanel.choiceList)
            choice.GetComponent<Button>().interactable = true;
        ChoicePanel.choicePanel.SetActive(true);
    }
    public void DeactivateChoiceUI()
    {
        ChoicePanel.choicePanel.SetActive(false);
        ResetChoiceElements();
    }

    public void StartAnimatingText(string text) => ActiveTextAnimator = StartCoroutine(AnimateText(text));

    private IEnumerator AnimateText(string text)
    {
        ActivateDialogueUI();
        DialoguePanel.speakerText.text = "";
        text = text.Replace("*player*", GlobalFunctions.PlayerName).Trim();
        while (!DialoguePanel.speakerText.text.Equals(text))
        {
            DialoguePanel.speakerText.text += text[DialoguePanel.speakerText.text.Length];
            yield return new WaitForSeconds(1 / _charAdditionsPerSecond);
        }
        FinishTextAnimation(text);
    }

    public async Task SetPortraitComponent(string speakerName)
    {
        // setting character portrait
        if (speakerName != "")
        {
            Character currentCharacter = CharacterManager.Instance.GetCharacter(speakerName);

            if (currentCharacter.characterPortrait == null)
            {
                var newPortrait = Addressables.LoadAssetAsync<GameObject>($"UI/Dialogue_Portraits/{currentCharacter.characterName}.prefab");
                await newPortrait.Task;
                if (newPortrait.Result != null)
                    currentCharacter.characterPortrait = Instantiate(newPortrait.Result, DialoguePanel.characterPanel.transform);
            }
            currentCharacter.characterPortrait.SetActive(true);
            SetCharacterPortrait(currentCharacter);
        }
        else
            DialoguePanel.characterPanel.SetActive(false);
        
    }

    public void FinishTextAnimation(string text)
    {
        text = text.Replace("*player*", GlobalFunctions.PlayerName).Trim();
        if (ActiveTextAnimator != null) StopCoroutine(ActiveTextAnimator);
        ActiveTextAnimator = null;
        DialoguePanel.speakerText.text = text;
    }

    public void DisplayChoices(List<string> choiceText)
    {
        ChoicePanel.choiceList[0].GetComponentInChildren<TMP_Text>().text = choiceText[0];

        if (choiceText.Count >= 2)
            ChoicePanel.choiceList[1].GetComponentInChildren<TMP_Text>().text = choiceText[1];
        else
            ChoicePanel.choiceList[1].GetComponent<Button>().interactable = false;
        if (choiceText.Count == 3)
            ChoicePanel.choiceList[2].GetComponentInChildren<TMP_Text>().text = choiceText[2];
        else
            ChoicePanel.choiceList[2].GetComponent<Button>().interactable = false;
    }

    private void ResetDialogueElements()
    {
        DialoguePanel.speakerText.text = "";
        DialoguePanel.speakerName.text = "";
    }

    private void ResetChoiceElements()
    {
        foreach (GameObject choiceObj in ChoicePanel.choiceList)
            choiceObj.GetComponentInChildren<TMP_Text>().text = "";
    }

    private void SetCharacterPortrait(Character character)
    {
        GlobalFunctions.DisableAllSiblingsExcept(DialoguePanel.characterPanel, character.characterPortrait);
        DialoguePanel.characterPanel.SetActive(true);
        character.CharacterBodyRenderer.gameObject.SetActive(true);
        character.CharacterExpressionRenderer.gameObject.SetActive(true);
    }
}
