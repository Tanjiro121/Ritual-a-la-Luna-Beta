using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonHeartUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;

    [Header("Sprite")]
    [SerializeField] private Sprite heartSprite;

    [Header("Colores")]
    [SerializeField] private Color fullColor = Color.white;
    [SerializeField] private Color emptyColor = Color.black;

    private readonly List<Image> heartImages = new();

    public void BuildHearts(int maxHealth)
    {
        ClearHearts();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            Image img = heart.GetComponent<Image>();

            if (img != null)
            {
                img.sprite = heartSprite;
                img.color = fullColor;
                heartImages.Add(img);
            }
        }
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].color = i < currentHealth ? fullColor : emptyColor;
        }
    }

    private void ClearHearts()
    {
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        heartImages.Clear();
    }
}