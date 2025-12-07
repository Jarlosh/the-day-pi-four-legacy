using UnityEngine;

public class CrosshairScaler : MonoBehaviour
{
    [Header("Target Scales")]
    public Vector3 defaultScale = Vector3.one;
    public Vector3 zoomScale = new Vector3(1.3f, 1.3f, 1f);
    public Vector3 recoilScale = new Vector3(1.7f, 1.7f, 1f);

    [Header("Speeds")]
    public float zoomSpeed = 6f;       // скорость увеличения при ПКМ
    public float recoilSpeed = 12f;    // скорость отдачи
    public float returnSpeed = 8f;     // скорость возврата в дефолт

    private Vector3 targetScale;
    private float currentSpeed;

    private bool isRightClickHeld = false;
    private bool isLeftClickFired = false;

    private void Start()
    {
        targetScale = defaultScale;
        transform.localScale = defaultScale;
    }

    private void Update()
    {
        HandleInputs();
        UpdateScale();
    }

    private void HandleInputs()
    {
        // ПКМ
        if (Input.GetMouseButtonDown(1))
            isRightClickHeld = true;

        if (Input.GetMouseButtonUp(1))
            isRightClickHeld = false;

        // ЛКМ (выстрел)
        if (Input.GetMouseButtonDown(0))
            isLeftClickFired = true;

        // Выбор цели и скорости
        if (isRightClickHeld)
        {
            targetScale = zoomScale;
            currentSpeed = zoomSpeed;
        }
        else if (isLeftClickFired)
        {
            targetScale = recoilScale;
            currentSpeed = recoilSpeed;
        }
        else
        {
            targetScale = defaultScale;
            currentSpeed = returnSpeed;
        }

        // Когда отдача почти достигнута — прекращаем "удар"
        if (isLeftClickFired && Vector3.Distance(transform.localScale, recoilScale) < 0.02f)
            isLeftClickFired = false;
    }

    private void UpdateScale()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * currentSpeed
        );
    }
}
