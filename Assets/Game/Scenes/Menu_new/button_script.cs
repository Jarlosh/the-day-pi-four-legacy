using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class FlexibleButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Image References")]
    [Tooltip("Изображение, отображаемое когда курсор НЕ наведен.")]
    public GameObject normalStateImage;

    [Tooltip("Изображение, отображаемое когда курсор наведен.")]
    public GameObject hoverStateImage;

    [Header("Text Settings")]
    [Tooltip("Основной текст кнопки.")]
    public TMP_Text targetText;

    [Tooltip("Цвет текста, когда курсор НЕ наведен.")]
    public Color normalTextColor = Color.white;

    [Tooltip("Цвет текста, когда курсор наведен.")]
    public Color hoverTextColor = Color.yellow;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Range(0f, 1f)]
    public float volumeHover = 0.7f;

    [Range(0f, 1f)]
    public float volumeClick = 1f;

    private bool isHovered = false;

    private void Start()
    {
        ResetToNormalState();
    }

    private void OnDisable()
    {
        // ФИКС бага зависания hover при смене панелей
        isHovered = false;
        ResetToNormalState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisual();
        PlaySound(hoverSound, volumeHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound, volumeClick);
    }

    private void ResetToNormalState()
    {
        if (normalStateImage != null) normalStateImage.SetActive(true);
        if (hoverStateImage != null) hoverStateImage.SetActive(false);

        if (targetText != null)
            targetText.color = normalTextColor;
    }

    private void UpdateVisual()
    {
        if (normalStateImage != null)
            normalStateImage.SetActive(!isHovered);

        if (hoverStateImage != null)
            hoverStateImage.SetActive(isHovered);

        if (targetText != null)
            targetText.color = isHovered ? hoverTextColor : normalTextColor;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
