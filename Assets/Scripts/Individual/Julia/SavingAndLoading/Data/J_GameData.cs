using System.Collections.Generic;

[System.Serializable]
public class J_GameData
{
    public float masterVolume;
    public float bgmVolume;
    public float sfxVolume;

    public Dictionary<string, bool> completedStages = new Dictionary<string, bool>();
    public string currentStage;

    // Starting New Game Values
    public J_GameData()
    {
        masterVolume = 1.0f;
        bgmVolume = 1.0f;
        sfxVolume = 1.0f;

        completedStages.Add(J_GameManager.MENU_SCENE, true);
        completedStages.Add(J_GameManager.DOG_SCENE, false);
        completedStages.Add(J_GameManager.DOG_ARENA_SCENE, false);
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
        completedStages[J_GameManager.DOG_ARENA_SCENE] = false;
        completedStages[J_GameManager.KID_SCENE] = false;
        completedStages[J_GameManager.MONSTER_SCENE] = false;
        completedStages[J_GameManager.REST_SCENE] = false;

        currentStage = J_GameManager.MENU_SCENE;
    }
}
