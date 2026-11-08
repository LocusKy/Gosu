
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public JudgmentUI judgmentPrefab;
public Transform judgmentContainer;
   public TextMeshProUGUI accuracyText;

    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;

    public int perfectHits = 0;
public int greatHits = 0;
public int goodHits = 0;
public int okHits = 0;
public int misses = 0;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterHit(int points)
    {
        score += points;
        combo++;

        if (combo > maxCombo)
        {
            maxCombo = combo;
        }

       if (points == 300)
    perfectHits++;
else if (points == 200)
    greatHits++;
else if (points == 100)
    goodHits++;
else if (points == 50)
    okHits++;
        Debug.Log("Score: " + score + " | Combo: " + combo);
        UpdateScoreUI();
        UpdateComboUI();
        UpdateAccuracyUI();
    }

    public void RegisterMiss()
    {
        misses++;
        combo = 0;
        UpdateComboUI();
        UpdateAccuracyUI();

        Debug.Log("MISS! Combo reset.");
    }
    
    void UpdateScoreUI()
{
    scoreText.text = score.ToString("D8");
}

void UpdateComboUI()
{
    comboText.text = combo + "x";
}


public float CalculateAccuracy()
{
    int totalNotes = perfectHits + greatHits + goodHits + okHits + misses;

    if (totalNotes == 0)
        return 100f;

    int earnedPoints =
        perfectHits * 300 +
        greatHits * 200 +
        goodHits * 100 +
        okHits * 50;

    int maximumPoints = totalNotes * 300;

    return (float)earnedPoints / maximumPoints * 100f;
}

void UpdateAccuracyUI()
{
    accuracyText.text = CalculateAccuracy().ToString("F2") + "%";
}

public void ShowJudgment(string judgment, Vector3 worldPosition)
{
    JudgmentUI newJudgment = Instantiate(
        judgmentPrefab,
        judgmentContainer
    );

    newJudgment.ShowJudgment(judgment, worldPosition);
}
}
