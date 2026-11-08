using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    public AudioClip audioClip;
    public SongManager songManager;
    public float preferredMinBPM = 100f;
    public float preferredMaxBPM = 200f;
    private float selectedBPM;

float DetectBeatOffset(
    float[] onsetStrength,
    float secondsPerChunk,
    float bpm)
{
    float beatDuration = 60f / bpm;

    float bestOffset = 0f;
    float bestScore = float.MinValue;

    // Test offsets at 5-millisecond intervals.
    for (float offset = 0f;
         offset < beatDuration;
         offset += 0.005f)
    {
        float score = 0f;
        int beatCount = 0;

        // Compare onset strength at each beat position.
        for (float time = offset;
             time < onsetStrength.Length * secondsPerChunk;
             time += beatDuration)
        {
            int index = Mathf.RoundToInt(
                time / secondsPerChunk
            );

            if (index >= 0 && index < onsetStrength.Length)
            {
                float strongestOnset = 0f;

// Search within 20 milliseconds of the predicted beat.
for (int j = -2; j <= 2; j++)
{
    int nearbyIndex = index + j;

    if (nearbyIndex >= 0 &&
        nearbyIndex < onsetStrength.Length)
    {
        strongestOnset = Mathf.Max(
            strongestOnset,
            onsetStrength[nearbyIndex]
        );
    }
}

score += strongestOnset;
beatCount++;
            }
        }

        if (beatCount > 0)
        {
            score /= beatCount;
        }

        if (score > bestScore)
        {
            bestScore = score;
            bestOffset = offset;
        }
    }

    return bestOffset;
}

float DetectBPM(float[] energyLevels, float secondsPerChunk)
{
    float bestCorrelation = float.MinValue;
    float bestBPM = 120f;
    List<(float bpm, float score)> candidates =
    new List<(float bpm, float score)>();

    // Search for tempos between 60 and 200 BPM.
    for (float bpm = 60f; bpm <= 200f; bpm += 0.5f)
    {
        float beatDuration = 60f / bpm;

        
float exactLag = beatDuration / secondsPerChunk;

int wholeLag = Mathf.FloorToInt(exactLag);
float fraction = exactLag - wholeLag;

float correlation = 0f;

for (int i = 0; i < energyLevels.Length - wholeLag - 1; i++)
{
    // Interpolate between neighboring audio chunks.
    float shiftedValue = Mathf.Lerp(
        energyLevels[i + wholeLag],
        energyLevels[i + wholeLag + 1],
        fraction
    );

    correlation += energyLevels[i] * shiftedValue;
}


correlation /= Mathf.Max(1, energyLevels.Length - wholeLag - 1);

candidates.Add((bpm, correlation));

        if (correlation > bestCorrelation)
        {
            bestCorrelation = correlation;
            bestBPM = bpm;
        }
    
    }
float halfTimeScore = 0f;
float doubleTimeScore = 0f;

int halfLag = Mathf.RoundToInt(
    (60f / bestBPM) / secondsPerChunk
);

int doubleLag = Mathf.RoundToInt(
    (60f / (bestBPM * 2f)) / secondsPerChunk
);

for (int i = 0; i < energyLevels.Length - halfLag; i++)
{
    halfTimeScore += energyLevels[i] * energyLevels[i + halfLag];
}

for (int i = 0; i < energyLevels.Length - doubleLag; i++)
{
    doubleTimeScore += energyLevels[i] * energyLevels[i + doubleLag];
}

Debug.Log("Half-time correlation: " + halfTimeScore);
Debug.Log("Double-time correlation: " + doubleTimeScore);

candidates.Sort((a, b) => b.score.CompareTo(a.score));

Debug.Log("TOP 10 BPM CANDIDATES:");

for (int i = 0; i < Mathf.Min(10, candidates.Count); i++)
{
    Debug.Log(
        "BPM: " + candidates[i].bpm +
        " | Correlation: " + candidates[i].score.ToString("F6")
    );
}

float selectedBPM = bestBPM;

// Prefer double-time when the detected tempo is
// below our preferred range.
if (selectedBPM < preferredMinBPM)
{
    float doubleBPM = selectedBPM * 2f;

    if (doubleBPM >= preferredMinBPM &&
        doubleBPM <= preferredMaxBPM)
    {
        selectedBPM = doubleBPM;
    }
}

Debug.Log("Selected BPM: " + selectedBPM.ToString("F2"));

    return selectedBPM;
}

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
    float secondsPerChunk = (float)chunkSize / audioClip.frequency;


float[] onsetStrength = new float[energyLevels.Length];

for (int i = 1; i < energyLevels.Length; i++)
{
    float energyChange = energyLevels[i] - energyLevels[i - 1];

    // Only keep increases in energy.
    onsetStrength[i] = Mathf.Max(0f, energyChange);
}

float autocorrelationBPM = DetectBPM(
    onsetStrength,
    secondsPerChunk
);
selectedBPM = autocorrelationBPM;

float gridOffset = DetectBeatOffset(
    onsetStrength,
    secondsPerChunk,
    selectedBPM
);

Debug.Log(
    "Beat-grid offset: " +
    gridOffset.ToString("F3") +
    " seconds"
);

Debug.Log(
    "Autocorrelation BPM: " +
    autocorrelationBPM.ToString("F2")
);
float doubleTimeBPM = autocorrelationBPM * 2f;

Debug.Log(
    "Possible double-time BPM: " +
    doubleTimeBPM.ToString("F2")
);
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
    float detectedBeatInterval = 60f / selectedBPM;
    
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

int[] offsetBuckets = new int[8];

foreach (float onset in onsetTimes)
{
    float phase = (onset % detectedBeatInterval)
                  / detectedBeatInterval;

    int bucket = Mathf.Clamp(
        Mathf.FloorToInt(phase * 8),
        0,
        7
    );

    offsetBuckets[bucket]++;
}

for (int i = 0; i < offsetBuckets.Length; i++)
{
    float bucketTime = i * detectedBeatInterval / 8f;

    Debug.Log(
        "Offset around " + bucketTime.ToString("F3") +
        "s: " + offsetBuckets[i] + " onsets"
    );
}


    float firstPeakTime = peakTimes[0];

float estimatedOffset = firstPeakTime % detectedBeatInterval;

Debug.Log(
    "Estimated beat offset: " +
    estimatedOffset.ToString("F3") +
    " seconds"
);

    Debug.Log("Median-based BPM: " + detectedBPM.ToString("F2"));
 

if (songManager != null &&
    selectedBPM >= 60f &&
    selectedBPM <= 240f)
{
    songManager.bpm = selectedBPM;
    songManager.beatOffset = gridOffset;

    Debug.Log(
        "Applied automatic timing | BPM: " +
        selectedBPM.ToString("F2") +
        " | Offset: " +
        gridOffset.ToString("F3")
    );
}
else if (songManager != null)
{
    Debug.LogWarning(
        "Automatic BPM detection failed. Keeping manual timing."
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
