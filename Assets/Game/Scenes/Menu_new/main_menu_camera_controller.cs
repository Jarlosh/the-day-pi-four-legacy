using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets")]
    public Transform mainMenuView;
    public Transform settingsView;
    public Transform creditsView;

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float rotateSpeed = 1.5f;
    public bool smoothTransition = true;

    [Header("Idle Camera Motion")]
    public bool enableIdleMotion = true;
    public float idleAmplitude = 0.05f;
    public float idleFrequency = 0.3f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 idleStartPos;

    private void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        idleStartPos = transform.position;
    }

    private void Update()
    {
        HandleMovement();
        HandleIdleMotion();
    }

    private void HandleMovement()
    {
        if (!smoothTransition)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            return;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    private void HandleIdleMotion()
    {
        if (!enableIdleMotion)
            return;

        float offset = Mathf.Sin(Time.time * idleFrequency) * idleAmplitude;

        transform.position = new Vector3(
            transform.position.x,
            idleStartPos.y + offset,
            transform.position.z
        );
    }

    // --- Public API ---

    public void MoveTo(Transform point)
    {
        if (point == null)
            return;

        targetPosition = point.position;
        targetRotation = point.rotation;
        idleStartPos = point.position;
    }

    public void GoToMainMenu() { MoveTo(mainMenuView); }
    public void GoToSettings() { MoveTo(settingsView); }
    public void GoToCredits() { MoveTo(creditsView); }
}
