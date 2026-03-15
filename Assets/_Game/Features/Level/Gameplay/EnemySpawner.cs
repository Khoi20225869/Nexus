using UnityEngine;

[System.Serializable]
public class EnemyStage
{
    public GameObject[] enemies;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyStage[] stages;
    [SerializeField] private Transform[] spawnPoints;

    private float spawnTimer;
    private float nextSpawnTime;
    private int currentStage = 0;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnTime)
        {
            SpawnEnemy();
            spawnTimer = 0f;
            SetNextSpawnTime();
        }
    }

    void SpawnEnemy()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        BoxCollider2D box = spawnPoints[spawnIndex].GetComponent<BoxCollider2D>();

        Vector2 spawnPos = new Vector2(
            Random.Range(box.bounds.min.x, box.bounds.max.x),
            Random.Range(box.bounds.min.y, box.bounds.max.y)
        );

        GameObject[] enemyPool = stages[currentStage].enemies;
        GameObject enemyPrefab = enemyPool[Random.Range(0, enemyPool.Length)];

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(4f, 6f);
    }
}