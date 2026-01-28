using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int enemiesPerWave = 5;
    [SerializeField] float spanInterval = 10f;
    private bool isActive = true;
    private int currentWave = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnWaves()
    {
        while (isActive)
        {
            currentWave++;
            for (int i = 0; i< enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spanInterval);
            }
            isActive = false;
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy =  Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        EnemyManager.instance.AddEnemy(enemy);
        
    }
}
