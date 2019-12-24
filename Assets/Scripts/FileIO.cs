using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class FileIO
{
    private static readonly string path = Application.persistentDataPath + "/";
    private static readonly string fileType = ".json";

    public static void Save<T>(string fileName, T obj)
    {
        try
        {
            using (var sw = File.CreateText(path + fileName + fileType))
            {
                new JsonSerializer().Serialize(sw, obj);
                Debug.Log("File writen to: " + path + fileName + fileType);
            }
        }
        catch
        {
            Debug.LogError("Error writing to file: " + path + fileName + fileType);
        }
    }

    public static T Load<T>(string fileName)
    {
        try
        {
            T data;
            using (var r = new StreamReader(path + fileName + fileType))
            {
                using (var jr = new JsonTextReader(r))
                {
                    data = new JsonSerializer().Deserialize<T>(jr);
                    Debug.Log("File loaded from: " + path + fileName + fileType);
                }
            }
            return data;
        }
        catch 
        {
            Debug.LogError("Error loading from file: " + path + fileName + fileType);
        }
        return default;
    }
}
