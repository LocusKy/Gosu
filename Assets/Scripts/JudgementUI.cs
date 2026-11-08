
using System.Collections;
using TMPro;
using UnityEngine;

public class JudgmentUI : MonoBehaviour
{
    public TextMeshProUGUI judgmentText;
    public CanvasGroup canvasGroup;

    public float animationDuration = 0.5f;

    private Coroutine currentAnimation;

    public void ShowJudgment(string judgment, Vector3 worldPosition)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        judgmentText.text = judgment;
        Vector2 screenPosition =
    Camera.main.WorldToScreenPoint(worldPosition);

RectTransform canvasRect =
    GetComponentInParent<Canvas>().GetComponent<RectTransform>();

RectTransformUtility.ScreenPointToLocalPointInRectangle(
    canvasRect,
    screenPosition,
    null,
    out Vector2 localPosition
);

judgmentText.rectTransform.anchoredPosition =
    localPosition;

        switch (judgment)
        {
            case "PERFECT":
                judgmentText.color = new Color(0.46f, 0.89f, 1f);
                break;

            case "GREAT":
                judgmentText.color = new Color(0.72f, 0.63f, 1f);
                break;

            case "GOOD":
                judgmentText.color = new Color(0.54f, 0.90f, 0.69f);
                break;

            case "OK":
                judgmentText.color = new Color(1f, 0.75f, 0.47f);
                break;

            case "MISS":
                judgmentText.color = new Color(1f, 0.47f, 0.53f);
                break;
        }

        currentAnimation = StartCoroutine(AnimateJudgment());
    }

    private IEnumerator AnimateJudgment()
    {
        float elapsed = 0f;

        canvasGroup.alpha = 1f;
        judgmentText.rectTransform.localScale = Vector3.one * 1.25f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / animationDuration);

            judgmentText.rectTransform.localScale =
                Vector3.one * Mathf.Lerp(1.25f, 1f, progress);

            canvasGroup.alpha = 1f - progress;

            yield return null;
        }

        canvasGroup.alpha = 0f;
        judgmentText.rectTransform.localScale = Vector3.one;
        currentAnimation = null;
        
        Destroy(gameObject);
    }
}
