using UnityEngine;

public interface J_IDataPersistence
{
    void LoadData(J_GameData data);

    void SaveData(ref J_GameData data);
}
