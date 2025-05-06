using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Transform SpawnPoint;
    [SerializeField] GameObject[] EnemyPrefabs;
    [SerializeField] float spawnCooldown = 3;
    [SerializeField] public int enemiesToSpawn = 30; 

    private float timer = 0;
    private GameObject enemyParentObject;
   

    private void Awake()
    {
        SpawnPoint = Grid.Instance.startPoint;

        EnemyPrefabs = Resources.LoadAll<GameObject>("Enemy");
        enemyParentObject = Helper.GetEnemyParentObject();
    }

    private void Start()
    {
        InvokeRepeating("SpawnEnemy", 0f, spawnCooldown);
    }

    // Update is called once per frame
    void Update()
    {
        //timer += Time.deltaTime;

        //if (timer > spawnCooldown)
        //{
        //    Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)], new Vector3(SpawnPoint.position.x, 0.3f, SpawnPoint.position.z), Quaternion.identity);
        //    timer = 0;
        //}
    }

    void SpawnEnemy()
    {
        if(enemiesToSpawn > 0)
        {
            GameObject enemy;

            enemy = Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)], new Vector3(SpawnPoint.position.x, 0.3f, SpawnPoint.position.z), Quaternion.identity);
            //Instantiate(EnemyPrefabs[0], new Vector3(SpawnPoint.position.x, 0.3f, SpawnPoint.position.z), Quaternion.identity);
            enemy.transform.parent = enemyParentObject.transform;

            enemiesToSpawn--;
        }
    }
}
