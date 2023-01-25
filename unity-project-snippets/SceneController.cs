using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI sceneChangeNotice;
    public CanvasGroup textCanvas;
    public CanvasGroup playerCanvas;

    public int oldLoadedScene;
    private int sceneToLoad;
    private string sceneToLoadName;
    private const int MASTER_SCENE_INT = 1;
    private string currentCG;
    public string CurrentlyLoadedScene => SceneManager.GetSceneByBuildIndex(oldLoadedScene).name;
    private bool isInIntroScene = true;
    public bool newGame = false;
    public bool isInCutscene = false;
    public string firstName;
    public string farmName;
    public NewSceneInfo viableSceneToLoad = null;

    private float fadeTime = 1.5f;

    public GameObject characterMenu;

    public void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        oldLoadedScene = 0;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        canvasGroup.alpha = 1;
        LeanTween.alphaCanvas(canvasGroup, 0, 2f);
    }

    public void LoadFromTitle()
    {
        LeanTween.alphaCanvas(canvasGroup, 1f, 1.5f).setOnComplete(DisplayCharacterName);
    }

    private void DisplayCharacterName()
    {
        characterMenu.SetActive(true);
        LeanTween.alphaCanvas(playerCanvas, 1f, 1.5f);
    }

    public void LoadSavedFileFromTitle()
    {
        LeanTween.alphaCanvas(canvasGroup, 1, 1.5f).setOnComplete(LoadFirstSceneContinue);
    }

    public void LoadFirstScene()
    {
        newGame = true;
        SceneManager.LoadScene(MASTER_SCENE_INT, LoadSceneMode.Additive);
        LoadCG("forestNight");
    }

    public void LoadFirstSceneSkipIntro()
    {
        SceneManager.LoadScene(MASTER_SCENE_INT, LoadSceneMode.Additive);
        newGame = false;
        SceneManager.LoadScene("Town", LoadSceneMode.Additive);
    }

    private void LoadFirstSceneContinue()
    {
        SceneManager.LoadScene(MASTER_SCENE_INT, LoadSceneMode.Additive);
        SceneManager.LoadScene("PlayerHouse", LoadSceneMode.Additive);
    }

    public void LoadBung() {
        SceneManager.LoadScene(MASTER_SCENE_INT, LoadSceneMode.Additive);
        SceneManager.LoadScene("FarmBoi", LoadSceneMode.Additive);
    }

    public void LoadScene(int levelIndex)
    {
        sceneToLoad = levelIndex;
        LeanTween.alphaCanvas(canvasGroup, 1, fadeTime).setOnComplete(OnFadeCompleteInt);
    }

    public void LoadScene(string levelName, float duration = 1.5f)
    {
        sceneToLoadName = levelName;
        fadeTime = duration;
        AudioManager.instance.AudioSceneChangeNotifier(levelName);
        //PlayerInputController.Instance.CanMove = false;
        LeanTween.alphaCanvas(canvasGroup, 1, fadeTime).setOnComplete(OnFadeCompleteString);
    }

    public void LoadCG(string cgName, float duration = 3f)
    {
        sceneToLoadName = "CGDisplay";
        fadeTime = duration;
        // Hardcoded for now
        AudioManager.instance.AudioSceneChangeNotifier("CGDisplay");
        LeanTween.alphaCanvas(canvasGroup, 1, fadeTime).setOnComplete(OnFadeCompleteString);
        currentCG = cgName;
    }

    public void OnFadeCompleteInt()
    {
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
    }

    public void OnFadeCompleteString()
    {
        SceneManager.LoadScene(sceneToLoadName, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded Scene: " + scene.name);
        if (mode == LoadSceneMode.Additive)
        {
            if (scene.buildIndex != MASTER_SCENE_INT)
            {
                SceneManager.SetActiveScene(scene);
                ActionHandler.SceneChangeHalfDone();
                SceneManager.UnloadSceneAsync(oldLoadedScene);
                oldLoadedScene = scene.buildIndex;
            }
            else
            {
                if (!newGame)
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName("MasterScene"));
                    FindObjectOfType<QuestManager>().skipIntro = true;
                    AudioManager.instance.AudioSceneChangeNotifier("Town");
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName("TitleScreen"));
                }
            }
        }
    }

    public void FadeIntoScene()
    {
        LeanTween.alphaCanvas(canvasGroup, 0, fadeTime).setOnComplete(FadeCaller);
    }
    private void FadeCaller() => ActionHandler.SceneChangeCompleted(SceneManager.GetActiveScene().name);

    public void OnSceneUnloaded(Scene unloadedScene)
    {
        Debug.Log("Unloaded Scene: " + unloadedScene.name);
        //SceneManager.SetActiveScene(SceneManager.GetSceneAt(2));
        //Debug.Log("New Active Scene: " + SceneManager.GetActiveScene().name);
        if (SceneManager.GetActiveScene().name == "CGDisplay")
        {
            GameObject.Find("CG").GetComponent<Image>().sprite = Resources.Load<Sprite>("CGs/" + currentCG);
            SetPlayerPosition(unloadedScene.name);
            LeanTween.cancel(textCanvas.gameObject);
            textCanvas.alpha = 0;
            FadeIntoScene();
            return;
        }
        SetPlayerPosition(unloadedScene.name);
        LeanTween.cancel(textCanvas.gameObject);
        textCanvas.alpha = 0;
        FadeIntoScene();
    }

    public void SetPlayerPosition(string oldSceneName)
    {
        SceneStartPoint pointGO;
        if (CharacterStats.Instance.GetStatValue(StatName.Player_Stamina) <= 0)
        {
            pointGO = GameObject.Find("StartingPointFromPassOut").GetComponent<SceneStartPoint>();
        }
        else if (SceneManager.GetActiveScene().name == "CGDisplay")
            return;
        else if (oldSceneName == "CGDisplay")
            pointGO = GameObject.Find("StartingPoint").GetComponent<SceneStartPoint>();
        else if (isInCutscene)
        {
            pointGO = GameObject.Find("StartingPoint").GetComponent<SceneStartPoint>();
        }
        else if (oldSceneName == SceneManager.GetActiveScene().name)
            pointGO = GameObject.Find("StartingPointFromTitleScreen").GetComponent<SceneStartPoint>();
        else if (isInIntroScene && oldSceneName == "Ocean")
        {
            pointGO = GameObject.Find("StartingPointFromTitleScreen").GetComponent<SceneStartPoint>();
            PlayerInputController.Instance.CanMove = true;
            isInIntroScene = false;
        }
        else if (oldSceneName == "SpaceShip")
            pointGO = GameObject.Find("StartingPoint").GetComponent<SceneStartPoint>();
        else if (oldSceneName != null)
        {
            pointGO = GameObject.Find("StartingPointFrom" + oldSceneName).GetComponent<SceneStartPoint>();
        }
        else
            pointGO = GameObject.Find("StartingPointFromTitleScreen").GetComponent<SceneStartPoint>();
        Vector2 startPoint = pointGO.transform.position;
        if (!isInCutscene)
            GameObject.Find("Player").transform.position = startPoint;
        CameraController camera = GameObject.Find("Main Camera").GetComponent<CameraController>();
        camera.setMinPosition(pointGO.newMin);
        camera.setMaxPosition(pointGO.newMax);
    }
}
