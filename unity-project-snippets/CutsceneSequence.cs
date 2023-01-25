using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cutscene Sequence", menuName = "Cutscene Sequence")]
public class CutsceneSequence : ScriptableObject
{
    public string cutsceneName;
    public short cutsceneProgress = 0;
    //public List<string> scenesToLoad;
    //public List<string> cgNames;
    //public List<string> dialogueMCs;
    //public List<string> dialogues;
    //public List<StartingLocations> startingLocations;

    public List<Cutscene> cutscenes;

    public string kickoutScene;

    public Cutscene CurrentCutscene => cutscenes[cutsceneProgress];
    public Character CurrentCharacter => CharacterManager.Instance.GetCharacter(cutscenes[cutsceneProgress].dialogueMC);
    //public Dialogue CurrentDialogue => CurrentCharacter.FindDialogue(cutscenes[cutsceneProgress].dialogueLines);

    private void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        cutsceneProgress = 0;
    }
}

[System.Serializable]
public struct StartingLocations
{
    public string character;
    public Vector2 location;
    public Vector2 direction;
}

[System.Serializable]
public class Cutscene
{
    [Header("Required Values")]
    public CutsceneType cutsceneType;
    public string dialogueMC;
    public string dialogueLines;
    public float fadeDuration = 1.5f;
    [Header("Scene-Specific Values")]
    public string sceneName;
    public List<StartingLocations> startingLocations;
    [Header("CG-Specific Values")]
    public string cGName;
}

public enum CutsceneType {
    Scene,
    CG
}

