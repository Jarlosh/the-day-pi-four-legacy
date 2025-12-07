using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ImageFadeCycler : MonoBehaviour
{
    [Header("Основное")]
    public Image targetImage;              // UI Image
    public List<Sprite> sprites;           // Список изображений
    public Color tintColor = Color.white;  // Цвет подкрашивания

    [Header("Настройки времени")]
    public float fadeInDuration = 1f;
    public float visibleDuration = 1.5f;
    public float fadeOutDuration = 1f;

    private int currentIndex = 0;
    private Coroutine cycleRoutine;

    void Start()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        StartCycle();
    }

    public void StartCycle()
    {
        if (cycleRoutine != null)
            StopCoroutine(cycleRoutine);

        cycleRoutine = StartCoroutine(CycleImages());
    }

    private IEnumerator CycleImages()
    {
        while (true)
        {
            // Устанавливаем цвет и спрайт
            targetImage.color = new Color(tintColor.r, tintColor.g, tintColor.b, 0f);
            targetImage.sprite = sprites[currentIndex];

            // FADE IN
            yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

            // Показ
            yield return new WaitForSeconds(visibleDuration);

            // FADE OUT
            yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

            // Следующий спрайт
            currentIndex = (currentIndex + 1) % sprites.Count;
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            targetImage.color = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            yield return null;
        }
    }

    // Можно менять цвет во время работы
    public void SetTint(Color newColor)
    {
        tintColor = newColor;
    }
}
