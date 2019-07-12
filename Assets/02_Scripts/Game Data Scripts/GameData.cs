using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;
    private Board board;

    // Start is called before the first frame update
    void Awake()
    {
        if(gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
            board = FindObjectOfType<Board>();
        }
        else
        {
            Destroy(this.gameObject);
        }
        Load();
    }

    public void Save()
    {
        //Create a binary formatter which can read binary files
        BinaryFormatter formatter = new BinaryFormatter();

        //Create a route from the program to the file
        FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.OpenOrCreate);

        //Create blank save data
        SaveData data = new SaveData();
        data = saveData;

        //Actually save the data in the file
        formatter.Serialize(file, data);

        //Close the data stream
        file.Close();
        print("Saved");
    }

    public void Load()
    {
        //Check if the save game file exists
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            //Create a Binary Formatter
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
            GameStartManager.instance.PlayGame();
            print("Loaded");
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDisable()
    {
        Save();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
