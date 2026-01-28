using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;

public class Revive : MonoBehaviour
{
    public GameObject player;

    public void RevivePlayer()
    {
        player.SetActive(true);
        MessageManager.SendMessage(new ReviveMessage(player.GetComponent<PlayerController>().GetPlayerId()));
        EnemyManager.instance.ResetIA();        
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject != player)
        {

            RevivePlayer();
            Destroy(gameObject);
        }

    }
}

