using System.Collections;
using _MessageType;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{

    float speed = 50f; 
    // Start is called before the first frame update
    void Start()
    {
        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.PAUSE] += MessagePause;
        MessageManager.messageDistribute[MessageType.UNPAUSE] += MessageUnpause;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MessagePause(Message message)
    {
        StopBullets();
    }

    void MessageUnpause(Message message)
    {
        ReplayBullets();
    }


    public void StopBullets()
    {
        //Stop bullet on Pause

        foreach (Rigidbody2D rb in GetComponentsInChildren<Rigidbody2D>())
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void ReplayBullets()
    {
        //Replay bullets on Unpause

        foreach (Rigidbody2D rb in GetComponentsInChildren<Rigidbody2D>())
        {
            rb.velocity = rb.transform.up * speed;
        }
    }



}
