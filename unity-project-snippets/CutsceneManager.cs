using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    public Action CutsceneFinished;
    public bool isInCutscene = false;

    public CutsceneSequence cutsceneSequence;

    [SerializeField]
    protected SceneTransition altSceneLoader;
    [SerializeField]
    protected CGTransition altCGLoader;

    [SerializeField]
    protected PlayableDirector director;
    [SerializeField]
    protected DialogueManager dialogueManager;


    

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = DialogueManager.Instance;
    }

    public void SetupCutscene(CutsceneSequence cutscene)
    {
        director.playableAsset = null;
        cutsceneSequence = cutscene;
        cutsceneSequence.ResetCutscene();
        
        Debug.Log("Setting up cutscene: " + cutscene);

        PlayerInputController.Instance.EnableCutsceneControls();
        UIManager.Instance.DisableAllUIs();

        if (cutsceneSequence == null)
        {
            Debug.LogError("Cutscene: " + cutscene + " not found!");
            return;
        }
        
        LoadSceneAndDialogue();
        ActionHandler.OnNewSceneStart += StartCutscene;
        return;
    }

    public async void StartCutscene(string _)
    {
        DateAndTimeManager.Instance.IsTimeTicking = false;
        PlayerInputController.Instance.CanMove = false;
        PlayerInputController.Instance.EnableCutsceneControls();
        PlayerInputController.Instance.GetComponent<BoxCollider2D>().isTrigger = true;
        UIManager.Instance.EnableDialogueUI();

        if (string.IsNullOrEmpty(cutsceneSequence.CurrentCutscene.inkPathName))
        {
            Debug.LogError($"Ink Path required for cutscene: {cutsceneSequence.cutsceneName}, element {cutsceneSequence.cutsceneProgress}");
        }

        if (cutsceneSequence.CurrentTimeline != null)
        {
            dialogueManager.DisableDialogueElements();
            director.playableAsset = cutsceneSequence.CurrentTimeline;
            director.Play();
            Debug.Log("Starting timeline: " + director.playableAsset.name);
        }
        else
        {
            await dialogueManager.StartQuestDialogue(cutsceneSequence.cutsceneName, cutsceneSequence.CurrentCutscene);
        }

        isInCutscene = true;
    }

    public void AdvanceCutscene()
    {
        if (cutsceneSequence.cutsceneProgress < cutsceneSequence.cutscenes.Count - 1)
        {
            cutsceneSequence.cutsceneProgress++;
            LoadSceneAndDialogue();
        }
        else
            EndCutscene();
    }

    public void EndCutscene()
    {
        isInCutscene = false;
        altSceneLoader.ExternalSceneTransitionCall(cutsceneSequence.kickoutScene, cutsceneSequence.kickoutPosition);
        cutsceneSequence = null;
        ActionHandler.OnNewSceneStart -= StartCutscene;
        PlayerInputController.Instance.EnableInGameControls();
        PlayerInputController.Instance.CanMove = true;
        PlayerInputController.Instance.GetComponent<BoxCollider2D>().isTrigger = false;
        UIManager.Instance.EnableInGameUI();
        PlayerInventoryController.Instance.ForceUpdateInventoryUI();
        DateAndTimeManager.Instance.IsTimeTicking = true;
        CutsceneFinished?.Invoke();
    }

    public async void ResumeCutsceneAnimation()
    {
        if (director.state == PlayState.Paused)
            director.Resume();
        DialogueManager.Instance.OnCutsceneDialogueFinished -= ResumeCutsceneAnimation;

        // Might break, will reset the lines
        await DialogueManager.Instance.StartCutsceneDialogue(cutsceneSequence.cutsceneName, cutsceneSequence.CurrentCutscene);
    }

    // Other reasons to pause?
    public async void PauseCutsceneAnimation()
    {
        Debug.Log("pausing cutscene");
        dialogueManager.EnableDialogueElements();
        if (director.state == PlayState.Playing)
            director.Pause();
        await dialogueManager.StartCutsceneDialogue(cutsceneSequence.cutsceneName, cutsceneSequence.CurrentCutscene);
        DialogueManager.Instance.OnCutsceneDialogueFinished += ResumeCutsceneAnimation;
    }

    protected virtual void LoadSceneAndDialogue()
    {
        if (cutsceneSequence.CurrentCutscene.backgroundCG != null)
            altCGLoader.BeginCGTransition(cutsceneSequence.CurrentCutscene.backgroundCG);
        else if (!string.IsNullOrEmpty(cutsceneSequence.CurrentCutscene.sceneName))
            altSceneLoader.ExternalSceneTransitionCall(cutsceneSequence.CurrentCutscene.sceneName, cutsceneSequence.kickoutPosition);
        else
            Debug.LogError("Cutscene not setup properly");
    }

    public void InstantCutsceneFinish()
    {
        PlayerInputController.Instance.playerControls.Disable();
        isInCutscene = false;
        dialogueManager.ExitDialogue();
        EndCutscene();
        UIManager.Instance.DisableAllUIs();
    }

}
