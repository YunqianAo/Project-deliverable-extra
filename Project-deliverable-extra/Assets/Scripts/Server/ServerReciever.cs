using _MessageType;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.VersionControl;
using UnityEngine;

public class ServerReceiver : MonoBehaviour
{
    [HideInInspector] public static Socket socket;
    [HideInInspector] public static EndPoint remote;

    [HideInInspector] public static ServerMessager[] messagers;

    static Thread receiver;

    // Start antes del update del primer frame
    void Start()
    {
        receiver = new Thread(MessageReceiver);
        receiver.Start();
    }

    static void MessageReceiver()
    {
        while (true)
        {
            byte[] data = new byte[8192];
            int size;

            try
            {
                size = socket.ReceiveFrom(data, ref remote);
            }
            catch (System.Exception e)
            {
                Debug.Log("Stopped recieving messages: " + e.ToString());
                return;
            }

            Message m = Serializer.FromBytes(data, size);

            //Mensaje al messager correspondiente
            messagers[m.playerID].OnMessageReceived(m);

            Debug.Log("Message received from Player " + m.playerID + "\n" + m.ToString());
        }
    }

    public static void SendMessageToEveryone(int playerID, Message m)
    {
        for (int i = 0; i < messagers.Length; i++)
        {
            if (i == playerID) continue;

            //Mensaje a todos menos al que lo envio
            messagers[i].SendMessage(m);
            Debug.Log("Distributing message from ( " + playerID + " ) to ( " + i + " )");
        }
    }


}