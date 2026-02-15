using UnityEngine;

[System.Serializable]
public class GameData
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

    public int playerStage; // leaving it here first, we'll definitely need some form of this eventually
    // public Weapons[] playerWeapons // we need to save this somehow

    // Starting New Game Values
    public GameData()
    {
        masterVolume = 1.0f;
        bgmVolume = 1.0f;
        sfxVolume = 1.0f;

        cameraSensitivity = 1f;
        qualityMode = QUALITYMODE.HIGH;

        playerStage = 0; // Starting stage
    }

    // Reset everything but the settings
    public void ResetData()
    {
        playerStage = 0;
    }
}
