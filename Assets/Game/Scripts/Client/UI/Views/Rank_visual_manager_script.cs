using Game.Client.UI;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        [Header("UI Settings")]
        public Color uiColor = Color.white;         // цвет основного изображения
        public Sprite backgroundSprite;             // фон

        [Header("UI Particle Settings")]
        public Color particleColor = Color.white;   // цвет UI-частиц
        public float particleSpeed = 200f;          // скорость движения частиц
        public float particleRate = 10f;            // частота спавна
    }

    [Header("References")]
    public Image mainImage;                 // основное изображение
    public Image backgroundImage;           // фоновое изображение
    public UISimpleEmitter uiEmitter;       // наш UI-эмиттер

    [Header("Stages")]
    public Stage[] stages;

    private int currentStage = -1;

    private void Start()
    {
        SetStage(0);
    }

    private void Update()
    {
        // переключение стадий клавишами 1-9
        for (int i = 0; i < stages.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetStage(i);
            }
        }
    }

    public void SetStage(int index)
    {
        if (index < 0 || index >= stages.Length)
            return;

        currentStage = index;
        Stage stage = stages[index];

        // Главный UI элемент
        if (mainImage != null)
        {
            Color safeColor = stage.uiColor;

            // защита от невидимой альфы
            if (safeColor.a <= 0.01f)
                safeColor.a = 1f;

            mainImage.color = safeColor;
        }

        // Фон
        if (backgroundImage != null)
            backgroundImage.sprite = stage.backgroundSprite;

        // Эмиттер UI-частиц
        if (uiEmitter != null)
        {
            uiEmitter.particleColor = stage.particleColor;
            uiEmitter.speed = stage.particleSpeed;
            uiEmitter.spawnRate = stage.particleRate;
        }

        Debug.Log("Stage switched to: " + (index + 1));
    }
}
