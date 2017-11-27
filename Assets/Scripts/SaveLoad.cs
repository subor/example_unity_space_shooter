using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad
{
    public static void Save<T>(T saveData, string filename) 
    {
        var path = Path.Combine(Application.persistentDataPath, filename);

        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Debug.Log("Saving game to: " + path);

        var bf = new BinaryFormatter();
        var file = File.Create(path);
        bf.Serialize(file, saveData);
        file.Close();
    }

    public static T Load<T>(string filename)
    {
        var path = Path.Combine(Application.persistentDataPath, filename);

        Debug.Log("Loading game from: " + path);

        if (File.Exists(path))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(path, FileMode.Open);

            var loadData = (T)bf.Deserialize(file);
            file.Close();

            return loadData;
        }

        throw new FileNotFoundException();
    }
}