using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string _dataDirPath = "";
    private string _dataFileName = "";
    private bool _useEncryption = false;
    private readonly string _encryptionCodeWord = "word";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        _dataDirPath = dataDirPath;
        _dataFileName = dataFileName;
        _useEncryption = useEncryption;
    }

    public GameData Load()
    {
        // Accounts for different OS's having different path separators
        string fullPath = Path.Combine(_dataDirPath, _dataFileName);

        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Optionally decrypt the data
                if (_useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // Deserailize data from JSON back to C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);    
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            } 
        }

        return loadedData;
    }

    public void Save(GameData data)
    {
        // Accounts for different OS's having different path separators
        string fullPath = Path.Combine(_dataDirPath, _dataFileName);

        try
        {
            // Create the directory path the file will be written to in case it doesn't exist on the computer
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize the C# game data object into JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            // Optionally encrypt the data
            if (_useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // Write the serialized data to the file
            // Using blocks ensure the connection to that file is closed once we're done reading or writing to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save to file: " + fullPath + "\n" + e);
        }
    }

    private string EncryptDecrypt (string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; ++i)
        {
            modifiedData += (char) (data[i] ^ _encryptionCodeWord[i % _encryptionCodeWord.Length]);
        }
        return modifiedData;
    }
}
