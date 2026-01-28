using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;

public class PlayersData : MonoBehaviour
{

    public static string[] names;
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        names = new string[Server.MAX_PLAYERS];
       

        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.SETTINGS] += MessageSettings;
    }

    private void OnDestroy()
    {
        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.POSITION] -= MessageSettings;
    }

    public void MessageSettings(Message m)
    {
        Settings s = m as Settings;

        SaveData(s.playerID, s.tankName);
        SetSettings();
    }

    public void SaveData(int id, string name)
    {
        names[id] = name;
   
    }

    public void SetSettings()
    {

    }
}
