using UnityEngine;

public class ApproachRing : MonoBehaviour
{
    public int segments = 64;
    public float radius = 0.5f;
    public float approachTime = 0.8f;
    public float startScale = 3f;

    private float elapsed;

    void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.positionCount = segments + 1;
        line.loop = true;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
        }

        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(elapsed / approachTime);
        float scale = Mathf.Lerp(startScale, 1f, progress);

        transform.localScale = Vector3.one * scale;

        if (progress >= 1f)
        {
            GetComponentInParent<Note>().ApproachFinished();
            enabled = false;
        }
    }
}