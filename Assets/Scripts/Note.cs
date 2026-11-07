using UnityEngine;
using UnityEngine.InputSystem;


public class Note : MonoBehaviour
{
    private bool wasHit = false;

    private float spawnTime;

void Start()
{
    spawnTime = Time.time;
}

public float GetNoteTime()
{
    return Time.time - spawnTime;
}

    private float hitTime = 0.8f;

    private float perfectWindow = 0.05f;
    private float goodWindow = 0.10f;
    private float badWindow = 0.15f;

    void Update()
{
    if (wasHit)
        return;
        if (GetNoteTime() > hitTime + badWindow)
{
    Debug.Log("MISS!");
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

    if (timingDifference <= perfectWindow)
    {
        Debug.Log("PERFECT!");
        wasHit = true;
    }
    else if (timingDifference <= goodWindow)
    {
        Debug.Log("GOOD!");
        wasHit = true;
    }
    else if (timingDifference <= badWindow)
    {
        Debug.Log("BAD!");
        wasHit = true;
    }
    else
    {
        Debug.Log("Too early or too late!");
    }
}
            else
            {
                Debug.Log("MISSED!");
            }
            if (wasHit)
{
    Destroy(gameObject);
}
        }
    }

}