using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public float spawnInterval = 1f;
    public SongManager songManager;

    public float minNoteDistance = 1.5f;
public float maxNoteDistance = 3f;

private Vector2 lastNotePosition;
private bool hasSpawnedNote = false;

private List<GameObject> activeNotes = new List<GameObject>();

    private float timer = 0f;

    private int nextBeatIndex = 0;
    void Start()
{
    if (songManager == null || songManager.bpm <= 0f)
        return;

    nextBeatIndex = Mathf.Max(
        0,
        Mathf.CeilToInt(
            (songManager.approachTime - songManager.beatOffset)
            / songManager.SecondsPerBeat
        )
    );
}


void Update()
{
    if (songManager == null || songManager.bpm <= 0f)
{
    return;
}

// Wait until the scheduled music start time.
if (AudioSettings.dspTime < songManager.songStartDSPTime)
{
    return;
}

if (!songManager.audioSource.isPlaying)
{
    return;
}
    float nextHitTime =
        songManager.beatOffset +
        nextBeatIndex * songManager.SecondsPerBeat;

    float nextSpawnTime =
        nextHitTime - songManager.approachTime;

    if (songManager.songTime >= nextSpawnTime)
{
    SpawnNote(nextHitTime);
    nextBeatIndex++;
}
}
  
bool IsTooCloseToActiveNotes(Vector2 position)
{
    // Remove notes that have already been destroyed.
    activeNotes.RemoveAll(note => note == null);

    // Check the distance to every active note.
    foreach (GameObject note in activeNotes)
    {
        float distance = Vector2.Distance(
            position,
            note.transform.position
        );

        if (distance < minNoteDistance)
        {
            return true;
        }
    }

    return false;
}

void SpawnNote(float scheduledTime)
{
    Vector2 spawnPosition = Vector2.zero;
    bool foundPosition = false;

    if (!hasSpawnedNote)
    {
        spawnPosition = new Vector2(
            Random.Range(-3f, 3f),
            Random.Range(-2f, 2f)
        );

        hasSpawnedNote = true;
    }
    else
    {
        // Try up to 100 times to find a suitable position.
        for (int i = 0; i < 100; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(-3f, 3f),
                Random.Range(-2f, 2f)
            );

            float distance = Vector2.Distance(
                candidate,
                lastNotePosition
            );

            if (distance >= minNoteDistance &&
               distance <= maxNoteDistance &&
               !IsTooCloseToActiveNotes(candidate))
{
    spawnPosition = candidate;
    foundPosition = true;
    break;
}
            
        }
    }
if (hasSpawnedNote && !foundPosition)
{
    Debug.LogWarning("No valid spawn position found!");
    return;
}
    lastNotePosition = spawnPosition;

   GameObject newNote = Instantiate(
    notePrefab,
    new Vector3(spawnPosition.x, spawnPosition.y, 0f),
    Quaternion.identity
);
Note noteScript = newNote.GetComponent<Note>();

noteScript.Initialize(songManager, scheduledTime);
activeNotes.Add(newNote);
}

}
