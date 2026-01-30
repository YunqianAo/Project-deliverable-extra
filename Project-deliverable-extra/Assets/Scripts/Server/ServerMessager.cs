using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ServerMessager : MonoBehaviour
{
    public int playerID;
    [HideInInspector] public Socket socket;
    [HideInInspector] public EndPoint remote;

    //Sent messages that haven't recieved acknowledgements for
    List<Message> sentMessages = new();
    List<Message> recievedMessages = new();

    //Other PCs recieved messages
    List<uint> acks = new();
    uint frameCounter = 0;
    bool sendAcks = true;
    const uint ACKS_FRAME_WAIT = 10;
    const uint RESEND_FRAME_WAIT = 15;

    //Current Acknowledgement number
    uint currentID = 0;

    void StopComunication()
    {
        sendAcks = false;
    }

    void Update()
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
            foreach (Message m in recievedMessages) ProcessReceivedMessages(m);
            recievedMessages.Clear();
        }
    }

    //  RECEIVE MESSAGES

    public void OnMessageReceived(Message m)
    {
        lock (recievedMessages)
        {
            recievedMessages.Add(m);
        }

        lock (acks)
        {
            acks.Add(m.id);
        }
    }

    void ProcessReceivedMessages(Message m)
    {

        if (m.type == MessageType.ACKNOWLEDGEMENTS)
        {
            OnAcknowledgementsRecieved(m);
            return;
        }

        if (m.type == MessageType.PING)
        {
            Message pong = new Message(MessageType.PONG)
            {
                id = m.id,
                playerID = m.playerID,
                time = m.time 
            };

            byte[] data = Serializer.ToBytes(pong);
            socket.SendTo(data, data.Length, System.Net.Sockets.SocketFlags.None, remote);
            return;
        }

        ServerReceiver.SendMessageToEveryone(playerID, m);
    }

    //  SEND MESSAGES

    uint NextID()
    {
        return ++currentID;
    }
    public void SendMessage(Message message)
    {
        //Send Message to my PLAYER

        message.time = Time.time;
        message.id = NextID();

        sentMessages.Add(message);

        //Send message
        byte[] messageData = Serializer.ToBytes(message);
        socket.SendTo(messageData, messageData.Length, SocketFlags.None, remote);
    }

    //  ACKNOWLEDGEMENTS

    void OnAcknowledgementsRecieved(Message m)
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

    void AcknowledgementSend()
    {
        //Send ACKS to my player

        lock (acks)
        {
            if (acks.Count > 0) SendMessage(new Acknowledgements(acks));
        }

        acks.Clear();
    }

    void MessageReSend()
    {
        //Re-Send non Acknowledged messages
        for (int i = 0; i < sentMessages.Count; i++)
        {
            if (Time.time - sentMessages[i].time > 0.2f) SendMessage(sentMessages[i]);
        }
    }
}
