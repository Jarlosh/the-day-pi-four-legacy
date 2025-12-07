using UnityEngine;

public class UIRotate : MonoBehaviour
{
    public float rotationSpeed = 100f; // градусов в секунду

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
