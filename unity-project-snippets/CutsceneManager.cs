using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    
    public static CutsceneManager instance;

    public CanvasGroup canvasGroup;
    public CutsceneCheckpoint checkpoint;

    private SceneController sceneController;

    private DialogueManager dialogueManager;

    public CutsceneCheckpoint cutsceneCheckpoint;
    public CutsceneSequence CurrentCutsceneSequence { get; set; }

    public bool isInCutscene = false;
    public bool suspendSceneLoad = false;

    public GameObject UI;
    public GameObject nonspeechUI;
    public GameObject introUI;

    private Coroutine activeCoroutine = null;


    public void SetLoadedCutscene(CutsceneSequence cutscene)
    {
        CurrentCutsceneSequence = cutscene;
    }

    public void BeginCutsceneSequence()
    {
        if (CurrentCutsceneSequence.CurrentCutscene.sceneName != "CGDisplay")
            foreach (NPCMovement npc in FindObjectsOfType<NPCMovement>())
            {
                npc.isInCutscene = true;
                npc.GetComponent<SpriteRenderer>().enabled = false;
            }
        if (cutsceneCheckpoint.parentQuest.questName != "Town Tour")
            introUI.SetActive(false);
        TimeManager.instance.inCutscene = true;
        ActionHandler.OnSceneEnded -= ResetActors;
        ActionHandler.OnSceneEnded += LoadProperPositioning;
        ActionHandler.OnNewSceneStart += StartCutscene;
        nonspeechUI.SetActive(false);
    }

    public void StartCutscene(string sceneName)
    {
        isInCutscene = true;
        dialogueManager.EnterDialogueMode(CurrentCutsceneSequence.cutsceneName, CurrentCutsceneSequence.CurrentCutscene.dialogueLines);
    }

    public void BridgeCutscene()
    {
        //TimeManager.instance.inCutscene = true;
        if (cutsceneCheckpoint.parentQuest.questName != "Town Tour")
            introUI.SetActive(false);
        ActionHandler.OnSceneEnded -= ResetActors;
        ActionHandler.OnSceneEnded += LoadProperPositioning;
        ActionHandler.OnNewSceneStart += StartCutscene;
        nonspeechUI.SetActive(false);
        LoadProperScenes();
    }

    public void EndCutscene()
    {
        isInCutscene = false;
        suspendSceneLoad = false;
        if (CurrentCutsceneSequence.cutsceneProgress >= CurrentCutsceneSequence.cutscenes.Count - 1)
        {
            EndSequence();
        }
        else
        {
            nonspeechUI.SetActive(false);
            CurrentCutsceneSequence.cutsceneProgress++;
            LoadProperScenes();
        }
    }

    public void EndSequence(bool force = false)
    {
        ActionHandler.OnNewSceneStart -= StartCutscene;

        // SkipIntro only
        if (force)
        {
            //ActionHandler.OnSceneEnded += ResetActors;
            ActionHandler.OnSceneEnded -= LoadProperPositioning;
            sceneController.LoadScene("PlayerHouse");
            return;
        }

        if (CurrentCutsceneSequence.kickoutScene != "")
        {
            ActionHandler.OnSceneEnded += ResetActors;
            ActionHandler.OnSceneEnded -= LoadProperPositioning;
            sceneController.LoadScene(CurrentCutsceneSequence.kickoutScene);
            return;
        }
        else
        {
            ActionHandler.CutsceneConcluded(CurrentCutsceneSequence.cutsceneName);
            BridgeCutscene();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        sceneController = GameObject.Find("SceneManager").GetComponent<SceneController>();
        dialogueManager = GetComponent<DialogueManager>();
    }

    public void LoadProperScenes()
    {
        if (CurrentCutsceneSequence.CurrentCutscene.cutsceneType == CutsceneType.CG)
            sceneController.LoadCG(CurrentCutsceneSequence.CurrentCutscene.cGName, CurrentCutsceneSequence.CurrentCutscene.fadeDuration);
        else
            sceneController.LoadScene(CurrentCutsceneSequence.CurrentCutscene.sceneName, CurrentCutsceneSequence.CurrentCutscene.fadeDuration);
    }

    // For setting initial actor positions
    public void LoadProperPositioning()
    {
        List<StartingLocations> startingLocations = CurrentCutsceneSequence.CurrentCutscene.startingLocations;
        for (int i = 0; i < startingLocations.Count; i++)
        {
            GameObject currentChar = GameObject.Find(startingLocations[i].character);
            NPCMovement npc = currentChar.GetComponent<NPCMovement>();
            if (npc)
            {
                npc.GetComponent<SpriteRenderer>().enabled = true;
                npc.isInCutscene = true;
            }
            currentChar.transform.position = startingLocations[i].location;
            currentChar.GetComponent<Animator>().SetFloat("LastMoveX", startingLocations[i].direction.x);
            currentChar.GetComponent<Animator>().SetFloat("LastMoveY", startingLocations[i].direction.y);
        }
    }

    public void ResetActors()
    {
        Debug.Log("Resetting Actors");
        UI.SetActive(true);
        nonspeechUI.SetActive(true);
        PlayerInputController.Instance.CanMove = true;
        ActionHandler.CutsceneConcluded(CurrentCutsceneSequence.cutsceneName);
        cutsceneCheckpoint.Complete();
        foreach (NPCMovement npc in FindObjectsOfType<NPCMovement>())
        {
            npc.isInCutscene = false;
            npc.GetComponent<SpriteRenderer>().enabled = false;
            npc.gameObject.transform.position = npc.startPosition.position;
        }
        TimeManager.instance.inCutscene = false;
        dialogueManager.SetSpeechPanelActive(false);
        
    }

    public void SkipIntro()
    {
        QuestManager.instance.availableQuests.TryGetValue("Town Tour", out Quest introquest);
        ActionHandler.OnSceneEnded -= LoadProperPositioning;
        ActionHandler.OnNewSceneStart -= StartCutscene;
        introquest.Complete();
        QuestManager.instance.availableQuests.TryGetValue("Back to Basics", out Quest tutorialquest);
        EndSequence(true);
        tutorialquest.CurrentCheckpoint.Complete();
        foreach (NPCMovement npc in FindObjectsOfType<NPCMovement>())
        {
            npc.isInCutscene = false;
            npc.GetComponent<SpriteRenderer>().enabled = false;
            npc.gameObject.transform.position = npc.startPosition.position;
        }
        TimeManager.instance.inCutscene = false;
        isInCutscene = false;
        UI.SetActive(true);
        nonspeechUI.SetActive(true);
        PlayerInputController.Instance.CanMove = true;
        dialogueManager.StopAllCoroutines();
        dialogueManager.SetSpeechPanelActive(false);
        dialogueManager.SetCharacterPanelActive(false);
        dialogueManager.ExitDialogueMode();
    }
}
