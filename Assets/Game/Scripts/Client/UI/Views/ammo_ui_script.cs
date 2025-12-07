using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    public Image cellTemplate;            // Префаб ячейки (выключенный объект-шаблон)

    [Header("Sprites")]
    public Sprite emptyCellSprite;        // Пустая ячейка
    public Sprite filledCellSprite;       // Ячейка с патроном

    private List<Image> ammoCells = new List<Image>();
    private int currentMax = 0;

    // Вызывает кодер
    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        // Если изменилось максимальное количество — перестраиваем UI
        if (maxAmmo != currentMax)
        {
            RebuildCells(maxAmmo);
            currentMax = maxAmmo;
        }

        // Обновляем спрайты
        for (int i = 0; i < ammoCells.Count; i++)
        {
            ammoCells[i].sprite = (i < currentAmmo) ? filledCellSprite : emptyCellSprite;
        }
    }

    private void RebuildCells(int maxAmmo)
    {
        // Удаляем старые
        foreach (var img in ammoCells)
            Destroy(img.gameObject);

        ammoCells.Clear();

        // Создаём новые
        for (int i = 0; i < maxAmmo; i++)
        {
            Image newCell = Instantiate(cellTemplate, transform);
            newCell.gameObject.SetActive(true);
            ammoCells.Add(newCell);
        }
    }
}
