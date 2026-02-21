using System.Collections.Generic;

[System.Serializable]
public class J_GameData
{
    public float masterVolume;
    public float bgmVolume;
    public float sfxVolume;
    public float cameraSensitivity;

    public enum QUALITYMODE { 
        LOW = 0,
        MEDIUM,
        HIGH
    }
    public QUALITYMODE qualityMode;

    public Dictionary<string, bool> completedStages = new Dictionary<string, bool>();
    public string currentStage;
    
    // public Weapons[] currentPlayerWeapons // we need to save this somehow

    // Starting New Game Values
    public J_GameData()
    {
        masterVolume = 1.0f;
        bgmVolume = 1.0f;
        sfxVolume = 1.0f;

        cameraSensitivity = 1f;
        qualityMode = QUALITYMODE.HIGH;

        completedStages.Add(J_GameManager.MENU_SCENE, true);
        completedStages.Add(J_GameManager.DOG_SCENE, false);
        completedStages.Add(J_GameManager.KID_SCENE, false);
        completedStages.Add(J_GameManager.MONSTER_SCENE, false);
        completedStages.Add(J_GameManager.REST_SCENE, false);

        currentStage = J_GameManager.MENU_SCENE; // Starting stage
    }

    // Reset everything but the settings
    public void ResetData()
    {

        completedStages[J_GameManager.MENU_SCENE] = true;
        completedStages[J_GameManager.DOG_SCENE] = false;
        completedStages[J_GameManager.KID_SCENE] = false;
        completedStages[J_GameManager.MONSTER_SCENE] = false;
        completedStages[J_GameManager.REST_SCENE] = false;

        currentStage = J_GameManager.MENU_SCENE;
    }
}
