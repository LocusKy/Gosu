using UnityEngine;
using UnityEngine.InputSystem;


public class Note : MonoBehaviour
{
    private bool wasHit = false;

    private float spawnTime;
    private SongManager songManager;
private float scheduledHitTime;
public void Initialize(SongManager manager, float hitTime)
{
    songManager = manager;
    scheduledHitTime = hitTime;

    UpdateHitTime();
}

void Start()
{
    spawnTime = Time.time;
}

public float GetNoteTime()
{
    if (songManager != null)
    {
        return songManager.songTime -
               (scheduledHitTime - songManager.approachTime);
    }

    return Time.time - spawnTime;
}
    private float hitTime = 0.8f;

void UpdateHitTime()
{
    if (songManager != null)
    {
        hitTime = songManager.approachTime;
    }
}

    private float perfectWindow = 0.04f;
private float greatWindow = 0.08f;
private float goodWindow = 0.12f;
private float okWindow = 0.16f;
private float earlyMissWindow = 0.25f;
    void Update()
{
    if (GetNoteTime() > hitTime + okWindow)
{
    Debug.Log("MISS!");

    GameManager.Instance.RegisterMiss();
    GameManager.Instance.ShowJudgment("MISS", transform.position);

    Destroy(gameObject);
    return;
}

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

            Camera mainCamera = Camera.main;

            Ray mouseRay = mainCamera.ScreenPointToRay(mouseScreenPosition);

            Plane notePlane = new Plane(Vector3.forward, transform.position);

            if (!notePlane.Raycast(mouseRay, out float distanceAlongRay))
            {
                return;
            }

            Vector2 mouseWorldPosition = mouseRay.GetPoint(distanceAlongRay);

            float distance = Vector2.Distance(mouseWorldPosition, transform.position);

            if (distance <= 0.5f)
{
    float timingDifference = Mathf.Abs(GetNoteTime() - hitTime);

    float timeUntilHit = hitTime - GetNoteTime();

if (timeUntilHit > okWindow && timeUntilHit <= earlyMissWindow)
{
    Debug.Log("EARLY MISS!");

    GameManager.Instance.RegisterMiss();
    GameManager.Instance.ShowJudgment("MISS", transform.position);

    Destroy(gameObject);
    return;
}

    
if (timingDifference <= perfectWindow)
{
    Debug.Log("PERFECT!");
    wasHit = true;
    GameManager.Instance.RegisterHit(300);
    GameManager.Instance.ShowJudgment("PERFECT", transform.position);
}
else if (timingDifference <= greatWindow)
{
    Debug.Log("GREAT!");
    wasHit = true;
    GameManager.Instance.RegisterHit(200);
    GameManager.Instance.ShowJudgment("GREAT", transform.position);
}
else if (timingDifference <= goodWindow)
{
    Debug.Log("GOOD!");
    wasHit = true;
    GameManager.Instance.RegisterHit(100);
    GameManager.Instance.ShowJudgment("GOOD", transform.position);
}
else if (timingDifference <= okWindow)
{
    Debug.Log("OK!");
    wasHit = true;
    GameManager.Instance.RegisterHit(50);
    GameManager.Instance.ShowJudgment("OK", transform.position);
}
else
{
    Debug.Log("Too early or too late!");
}
}
    else
    {
        Debug.Log("Too early or too late!");
    }
}
            if (wasHit)
{
    Destroy(gameObject);
}
        }
public float GetApproachTime()
{
    if (songManager != null)
        return songManager.approachTime;

    return 0.8f;
}

    }

