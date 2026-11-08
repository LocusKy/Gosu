
using UnityEngine;

public class SongManager : MonoBehaviour
{
    public AudioSource audioSource;

    public float bpm = 120f;
    public float beatOffset = 0.13f;
    public float approachTime = 0.8f;

    public double songStartDSPTime;

    public float songTime { get; private set; }

    public float SecondsPerBeat
    {
        get { return 60f / bpm; }
    }

    void Start()
    {
        // Schedule the music to begin 1 second from now.
        songStartDSPTime = AudioSettings.dspTime + 1.0;

        audioSource.PlayScheduled(songStartDSPTime);
    }

    void Update()
    {
        // Calculate song time using Unity's audio clock.
        double elapsedTime =
            AudioSettings.dspTime - songStartDSPTime;

        songTime = Mathf.Max(0f, (float)elapsedTime);
    }
}
