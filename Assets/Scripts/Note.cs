using UnityEngine;
using UnityEngine.InputSystem;

public class Note : MonoBehaviour
{
    private bool wasHit = false;

    void Update()
    {
        if (wasHit)
            return;

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
                wasHit = true;
                Debug.Log("HIT!");
            }
            else
            {
                Debug.Log("MISSED!");
            }
        }
    }

    public void ApproachFinished()
{
    if (!wasHit)
    {
        Debug.Log("MISS!");
        Destroy(gameObject);
    }
}
}