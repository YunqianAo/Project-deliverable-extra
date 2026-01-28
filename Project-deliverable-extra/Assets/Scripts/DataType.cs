using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _MessageType
{
    public enum MessageType
    {
        _NONE,
        POSITION,
        ENEMYPOSITION,
        ANIMATIONSTATE,
        ACKNOWLEDGEMENTS,
        SHOOT,
        HITENEMY,
        KILLENEMY,
        HITPLAYER,
        KILL,
        REVIVE,
        PAUSE,
        UNPAUSE,
        RESET,
        WIN,
        LOSE,
        SETTINGS,
        PING,
        PONG,

        _MESSAGE_TYPE_COUNT

    }
    public class Message 
    {
        public Message(MessageType type) => this.type = type;
        public uint id;
        public float time;
        public int playerID;
        public MessageType type;
    }

    public class PingMessage : Message
    {
        public float timestamp;
        public PingMessage(float time) : base(MessageType.PING) {

            this.timestamp = time;
        }
    }

    public class Position : Message
    {
        public Position(Vector3 pos, float rot) : base(MessageType.POSITION)
        {
            this.pos = pos;
            this.rot = rot;
            this.timestamp = Time.time;
        }

        public Vector3 pos;
        public float rot;
        public float timestamp;
    }

    public class AnimationStateMessage : Message
    {
        public int PlayerId { get; private set; }
        public float Horizontal { get; private set; }
        public float Vertical { get; private set; }
       

        public AnimationStateMessage(int playerId, float horizontal, float vertical): base(MessageType.ANIMATIONSTATE)
        {
            PlayerId = playerId;
            Horizontal = horizontal;
            Vertical = vertical;
            
        }
    }

    public class KillEnemyMessage : Message
    {
        public int EnemyID;

        public KillEnemyMessage(int EnemyID) : base(MessageType.KILLENEMY)
        {
            this.EnemyID = playerID;
        }
    }


    public class HitEnemy : Message
    {
        public HitEnemy(int hitEnemyID) : base(MessageType.HITENEMY)
        {
            this.hitEnemyID = hitEnemyID;
        }

        public int hitEnemyID;
    }
    public class Shoot : Message
    {
        public Shoot(Vector3 hit, int playerID) : base(MessageType.SHOOT)
        {
            this.playerID = playerID;
            this.hitPoint = hit;
        }
        public Vector3 hitPoint;

    }
    public class HitPlayer : Message
    {
        public HitPlayer(int hitPlayerID) : base(MessageType.HITPLAYER)
        {
            this.hitPlayerID = hitPlayerID;
        }

        public int hitPlayerID;
    }

    public class Settings : Message
    {
        public Settings(string tankName, Color color) : base(MessageType.SETTINGS)
        {
            this.tankName = tankName;
            this.color = color;
        }

        public string tankName;
        public Color color;
    }

    public class KillMessage : Message
    {
        public int PlayerID;

        public KillMessage(int playerID) : base(MessageType.KILL)
        {
            this.PlayerID = playerID;
        }
    }
    public class ReviveMessage : Message
    {
        public int PlayerID;

        public ReviveMessage(int playerID) : base(MessageType.REVIVE)
        {
            this.PlayerID = playerID;
        }
    }
    public class Acknowledgements : Message
    {
        public Acknowledgements(List<uint> acks) : base(MessageType.ACKNOWLEDGEMENTS)
        {
            this.acks = acks;
        }

        public List<uint> acks;
    }


    public class Serializer
    {
        //From Data to Json string
        public static string ToJson(Message m)
        {
            return JsonUtility.ToJson(m);
        }

        //From Data to Bytes
        public static byte[] ToBytes(Message m)
        {
            return Encoding.ASCII.GetBytes(ToJson(m));
        }

        //From bytes to Data
        public static Message FromBytes(byte[] data, int size)
        {
            return FromJson(Encoding.ASCII.GetString(data, 0, size));
        }



        //From Json string to Data
        public static Message FromJson(string json)
        {
            Message m = JsonUtility.FromJson<Message>(json);

            //Check to reDeserialize in case of inherited class
            switch (m.type)
            {
                case MessageType.ACKNOWLEDGEMENTS:
                    {   
                        m = JsonUtility.FromJson<Acknowledgements>(json);
                        break;
                    }
                case MessageType.ANIMATIONSTATE:
                    {
                        m = JsonUtility.FromJson<AnimationStateMessage>(json);
                        break;
                    }
                case MessageType.POSITION:
                    {
                        m = JsonUtility.FromJson<Position>(json);
                        break;
                    }
                case MessageType.SHOOT:
                    {
                        m = JsonUtility.FromJson<Shoot>(json);
                        break;
                    }
                case MessageType.HITPLAYER:
                    {
                        m = JsonUtility.FromJson<HitPlayer>(json);
                        break;
                    }
                case MessageType.PING:
                    {
                        m = JsonUtility.FromJson<PingMessage>(json);
                        break;
                    }
                case MessageType.KILL:
                    {
                        m = JsonUtility.FromJson<KillMessage>(json);
                        break;
                    }
                case MessageType.REVIVE:
                    {
                        m = JsonUtility.FromJson<ReviveMessage>(json);
                        break;
                    }
                case MessageType.KILLENEMY:
                    {
                        m = JsonUtility.FromJson<KillEnemyMessage>(json);
                        break;
                    }
                case MessageType.HITENEMY:
                    {
                        m = JsonUtility.FromJson<HitEnemy>(json);
                        break;
                    }
            }

            return m;
        }

        internal static T FromBytes<T>(byte[] data)
        {
            throw new NotImplementedException();
        }
    }

}
