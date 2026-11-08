using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    public AudioClip audioClip;
    public SongManager songManager;

    void  Awake()
    {
        if (audioClip == null)
        {
            Debug.LogError("No audio clip assigned!");
            return;
        }

        Debug.Log("Analyzing: " + audioClip.name);

        Debug.Log("Duration: " + audioClip.length + " seconds");
        Debug.Log("Sample Rate: " + audioClip.frequency + " Hz");

        float[] samples = new float[audioClip.samples * audioClip.channels];

        bool success = audioClip.GetData(samples, 0);

        
if (success)
{
    Debug.Log("Successfully loaded " + samples.Length + " audio samples!");

    // Analyze the audio in 10-millisecond chunks.
    int chunkSize = Mathf.Max(1, audioClip.frequency / 100);
    int totalChunks = audioClip.samples / chunkSize;

    float[] energyLevels = new float[totalChunks];

    for (int i = 0; i < totalChunks; i++)
    {
        float energy = 0f;

        for (int j = 0; j < chunkSize; j++)
        {
            int sampleFrame = i * chunkSize + j;

            for (int channel = 0; channel < audioClip.channels; channel++)
            {
                int index = sampleFrame * audioClip.channels + channel;
                float sample = samples[index];

                energy += sample * sample;
            }
        }

        energyLevels[i] = energy / (chunkSize * audioClip.channels);
    }

    Debug.Log("Calculated " + energyLevels.Length + " energy measurements!");
    List<float> onsetTimes = new List<float>();

for (int i = 2; i < energyLevels.Length; i++)
{
    float previousEnergy = energyLevels[i - 1];
    float currentEnergy = energyLevels[i];

    float energyIncrease = currentEnergy - previousEnergy;

    if (energyIncrease > 0.002f)
    {
        float onsetTime =
            i * (float)chunkSize / audioClip.frequency;

        // Ignore detections within 50 milliseconds
// of the previous detected onset.
if (onsetTimes.Count == 0 ||
    onsetTime - onsetTimes[onsetTimes.Count - 1] >= 0.05f)
{
    onsetTimes.Add(onsetTime);
}
    }
}

Debug.Log("Detected onsets: " + onsetTimes.Count);

for (int i = 0; i < Mathf.Min(10, onsetTimes.Count); i++)
{
    Debug.Log("Onset at: " + onsetTimes[i].ToString("F3"));
}
    
int detectedPeaks = 0;
List<float> peakTimes = new List<float>();
for (int i = 1; i < energyLevels.Length - 1; i++)
{
    float current = energyLevels[i];
    float previous = energyLevels[i - 1];
    float next = energyLevels[i + 1];

    // Detect local peaks above a minimum energy level.
    if (current > previous &&
        current >= next &&
        current > 0.01f)
    {
        detectedPeaks++;
        float peakTime = i * (float)chunkSize / audioClip.frequency;
peakTimes.Add(peakTime);

        // Print only the first 20 detected peaks.
        if (detectedPeaks <= 20)
        {
            float timestamp = i * 0.01f;
            Debug.Log("Detected peak at " +
                timestamp.ToString("F2") + " seconds");
        }
    }
}

Debug.Log("Total detected peaks: " + detectedPeaks);


if (peakTimes.Count >= 2)
{
    List<float> intervals = new List<float>();

    for (int i = 1; i < peakTimes.Count; i++)
    {
        float interval = peakTimes[i] - peakTimes[i - 1];
        intervals.Add(interval);
    }

    intervals.Sort();

    int middle = intervals.Count / 2;

    float medianInterval;

    if (intervals.Count % 2 == 0)
    {
        medianInterval = (intervals[middle - 1] + intervals[middle]) / 2f;
    }
    else
    {
        medianInterval = intervals[middle];
    }

    float detectedBPM = 60f / medianInterval;
    float detectedBeatInterval = 60f / detectedBPM;
    
List<float> refinedOnsets = new List<float>();


foreach (float onset in onsetTimes)
{
    int onsetChunk = Mathf.RoundToInt(
        onset * audioClip.frequency / chunkSize
    );

    int searchStart = Mathf.Max(0, onsetChunk - 10);
    int searchEnd = Mathf.Min(
        onsetChunk,
        energyLevels.Length - 1
    );

    // Find the highest energy in the search window.
    float maximumEnergy = 0f;

    for (int i = searchStart; i <= searchEnd; i++)
    {
        maximumEnergy = Mathf.Max(
            maximumEnergy,
            energyLevels[i]
        );
    }

    // Find the earliest significant energy rise.
    float threshold = maximumEnergy * 0.15f;
    int bestChunk = searchEnd;

    for (int i = searchStart; i <= searchEnd; i++)
    {
        if (energyLevels[i] >= threshold)
        {
            bestChunk = i;
            break;
        }
    }

    float refinedTime =
        bestChunk * (float)chunkSize / audioClip.frequency;

    refinedOnsets.Add(refinedTime);
}

Debug.Log("First refined onset: " +
    refinedOnsets[0].ToString("F3"));
    
int firstOnsetChunk = Mathf.RoundToInt(
    onsetTimes[0] * audioClip.frequency / chunkSize
);

for (int i = firstOnsetChunk - 10; i <= firstOnsetChunk; i++)
{
    if (i < 0 || i >= energyLevels.Length)
        continue;

    float time = i * (float)chunkSize / audioClip.frequency;

    Debug.Log(
        "Time: " + time.ToString("F3") +
        energyLevels[i].ToString("E8")
    );
}


    
List<float> onsetOffsets = new List<float>();

foreach (float onset in onsetTimes)
{
    float offset = onset % detectedBeatInterval;
    onsetOffsets.Add(offset);
}

onsetOffsets.Sort();

float medianOffset = onsetOffsets[onsetOffsets.Count / 2];

Debug.Log(
    "Median onset offset: " +
    medianOffset.ToString("F3") +
    " seconds"
);

    float firstPeakTime = peakTimes[0];

float estimatedOffset = firstPeakTime % detectedBeatInterval;

Debug.Log(
    "Estimated beat offset: " +
    estimatedOffset.ToString("F3") +
    " seconds"
);

    Debug.Log("Median-based BPM: " + detectedBPM.ToString("F2"));
    if (songManager != null)
{
    songManager.bpm = detectedBPM;
    songManager.beatOffset = medianOffset;

    Debug.Log(
        "Applied to SongManager | BPM: " +
        detectedBPM.ToString("F2") +
        " | Offset: " +
        medianOffset.ToString("F3")
    );
}
}


}

        else
        {
            Debug.LogError("Failed to read audio samples!");
        }
    }
}
