using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class MessageManager : MonoBehaviour
{
    //Sent messages that haven't recieved acknowledgements for
    static List<Message> sentMessages = new();
    static List<Message> recievedMessages = new();

    //Other PCs recieved messages
    static List<uint> acks = new();
    static uint frameCounter = 0;
    static bool sendAcks = false;
    const uint ACKS_FRAME_WAIT = 10;
    const uint RESEND_FRAME_WAIT = 15;

    //All objects that need to be sent messages
    [HideInInspector] public static Dictionary<MessageType, Action<Message>> messageDistribute = new();

    //Current Acknowledgement number
    static uint currentID = 0;

    //Network
    [HideInInspector] public static Socket socket;
    [HideInInspector] public static EndPoint remote;
    [HideInInspector] public static int playerID;

    static Thread messageReciever = new(MessageReciever);



    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (messageDistribute.Count == 0)
        {
            for (int i = 0; i < (int)MessageType._MESSAGE_TYPE_COUNT; i++)
            {
                messageDistribute.Add((MessageType)i, null);
            }
        }
    }

    private void Start()
    {
        //Subscribe to acknowledgement messages
        messageDistribute[MessageType.ACKNOWLEDGEMENTS] += OnAcknowledgementsRecieved;
    }

    private void Update()
    {
        //Send Acknowledgements
        bool resetCounter = false;
        frameCounter++;
        if (frameCounter % ACKS_FRAME_WAIT == 0)
        {
            if (sendAcks) AcknowledgementSend();
            resetCounter = true;
        }
        if (frameCounter % RESEND_FRAME_WAIT == 0)
        {
            MessageReSend();
            if (resetCounter) frameCounter = 0;
        }

        if (recievedMessages.Count == 0) return;

        //Process recieved messages
        lock (recievedMessages)
        {
            foreach (Message m in recievedMessages) OnRecievedMessage(m);
            recievedMessages.Clear();
        }
    }

    private void OnApplicationQuit()
    {
        StopComunication();
    }

    static uint NextID()
    {
        return ++currentID;
    }

    //Message type & other data has to be set before
    public static void SendMessage(Message message)
    {
        if (messageDistribute.Count == 0) return;

        message.time = Time.time;
        message.id = NextID();
        message.playerID = playerID;

        sentMessages.Add(message);

        //Send message
        byte[] messageData = Serializer.ToBytes(message);
        socket.SendTo(messageData, messageData.Length, SocketFlags.None, remote);
    }

    public static void SendMessage(MessageType type)
    {
        SendMessage(new Message(type));
    }

    static void OnRecievedMessage(Message message)
    {
        //Send message to subscribed objects
        messageDistribute[message.type]?.Invoke(message);
    }

    static void OnAcknowledgementsRecieved(Message m)
    {
        Acknowledgements a = m as Acknowledgements;

        //Remove the Acknowledged messages
        lock (acks)
        {
            for (int i = sentMessages.Count - 1; i >= 0; i--)
            {
                if (a.acks.Contains(sentMessages[i].id))
                {
                    sentMessages.RemoveAt(i);
                }
            }
        }
    }

    void StopComunication()
    {
        messageReciever.Abort();
        sendAcks = false;
        //TODO: Kill Sockets
    }

    public static void StartComunication()
    {
        messageReciever.Start();
        sendAcks = true;
    }

    static void AcknowledgementSend()
    {
        lock (acks)
        {
            if (acks.Count > 0) SendMessage(new Acknowledgements(acks));
        }

        acks.Clear();
    }

    static void MessageReSend()
    {
        //Re-Send non Acknowledged messages
        for (int i = 0; i < sentMessages.Count; i++)
        {
            if (Time.time - sentMessages[i].time > 0.2f) SendMessage(sentMessages[i]);
        }
    }

    static void MessageReciever()
    {

        while (true)
        {
            byte[] data = new byte[1024];
            int size;

            try
            {
                size = socket.ReceiveFrom(data, ref remote);
            }
            catch
            {
                Debug.Log("Stopped recieving messages.");
                return;
            }

            Message m = Serializer.FromBytes(data, size);

            //Add to process later
            lock (recievedMessages)
            {
                recievedMessages.Add(m);
            }

            lock (acks)
            {
                acks.Add(m.id);
            }
        }
    }
}
