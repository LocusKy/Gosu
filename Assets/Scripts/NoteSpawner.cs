
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public float spawnInterval = 1f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0f;
        }
    }

    void SpawnNote()
    {
        float randomX = Random.Range(-3f, 3f);
        float randomY = Random.Range(-2f, 2f);

        Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);

        Instantiate(notePrefab, spawnPosition, Quaternion.identity);
    }
}
