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

    [Header("Specific Enemies")]
    [SerializeField] private GameObject slime1;
    [SerializeField] private GameObject slime2;

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
            SpawnEnemies(stages[currentStage].enemies, 1);

            spawnTimer = 0f;
            SetNextSpawnTime();
        }
    }
    
    void SpawnEnemies(GameObject[] prefabs, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Instantiate(prefab, GetRandomSpawnPosition(), Quaternion.identity);
        }
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        BoxCollider2D box = spawnPoints[spawnIndex].GetComponent<BoxCollider2D>();

        return new Vector2(
            Random.Range(box.bounds.min.x, box.bounds.max.x),
            Random.Range(box.bounds.min.y, box.bounds.max.y)
        );
    }
    
    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(4f, 6f);
    }
    
    public void SpawnSlime1()
    {
        SpawnEnemies(new GameObject[] { slime1 }, 5);
    }

    public void SpawnSlime2()
    {
        SpawnEnemies(new GameObject[] { slime2 }, 5);
    }
}