using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISimpleEmitter : MonoBehaviour
{
    [Header("Spawn Settings")]
    public RectTransform spawnPoint;
    public GameObject particlePrefab;
    public int maxParticles = 50;

    public float spawnRate = 10f;
    public float speed = 200f;
    public float lifeTime = 2f;
    public float angleSpread = 20f;

    [Header("Particle Visual Settings")]
    public Color particleColor = Color.white;

    [Tooltip("Начальный масштаб частицы")]
    public float startScale = 1f;

    [Tooltip("Конечный масштаб частицы (в конце жизни)")]
    public float endScale = 0.2f;

    private float spawnTimer = 0f;

    private class UIParticle
    {
        public RectTransform rect;
        public Image image;
        public Vector2 velocity;
        public float life;
        public float maxLife;
    }

    private List<UIParticle> particles = new List<UIParticle>();

    void Update()
    {
        float dt = Time.deltaTime;

        // Spawn logic
        spawnTimer += dt;
        float interval = 1f / spawnRate;

        while (spawnTimer >= interval && particles.Count < maxParticles)
        {
            SpawnParticle();
            spawnTimer -= interval;
        }

        // Update particles
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            UIParticle p = particles[i];
            p.life -= dt;

            if (p.life <= 0f)
            {
                Destroy(p.rect.gameObject);
                particles.RemoveAt(i);
                continue;
            }

            float t = 1f - (p.life / p.maxLife); // 0 → начало | 1 → конец

            // Position
            p.rect.anchoredPosition += p.velocity * dt;

            // Scale interpolation
            float scale = Mathf.Lerp(startScale, endScale, t);
            p.rect.localScale = new Vector3(scale, scale, 1);

            // Fade-out (alpha)
            if (p.image != null)
            {
                Color c = p.image.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                p.image.color = c;
            }
        }
    }

    void SpawnParticle()
    {
        GameObject obj = Instantiate(particlePrefab, this.transform);
        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchoredPosition = spawnPoint.anchoredPosition;
        rect.localScale = new Vector3(startScale, startScale, 1);

        Image img = obj.GetComponent<Image>();

        if (img != null)
        {
            img.color = particleColor; // задаём стартовый цвет (полностью видимый)
        }

        float angle = Random.Range(-angleSpread, angleSpread);
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

        UIParticle particle = new UIParticle
        {
            rect = rect,
            image = img,
            velocity = direction * speed,
            life = lifeTime,
            maxLife = lifeTime
        };

        particles.Add(particle);
    }
}
