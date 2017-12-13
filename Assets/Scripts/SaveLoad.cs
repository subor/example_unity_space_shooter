using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoad
{
    public void Save<T>(T saveData, string persistentDataPath, string filename)
    {
        var path = Path.Combine(persistentDataPath, filename);

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

    public T Load<T>(string persistentDataPath, string filename)
    {
        var path = Path.Combine(persistentDataPath, filename);

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